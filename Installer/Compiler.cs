using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Installer.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace Installer
{
	internal class Compiler
	{
		public ZipArchive ProjectArchive { get; }
		public Installation Installation { get; }
		public string ProjectContent { get; } = string.Empty;
		public string[] Exclude;
		public string[] Defines = Array.Empty<string>();

		public static readonly CSharpCompilationOptions CompilationOptions =
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithOverflowChecks(true)
				.WithOptimizationLevel(OptimizationLevel.Release);

		public Compiler(ZipArchive projectArchive, Installation installation, params string[] exclude)
		{
			ProjectArchive = projectArchive;
			Installation = installation;
			Exclude = exclude;

			var entry = projectArchive.Entries.FirstOrDefault(e => e.Name == "NLog.EFT.Trainer.csproj");
			if (entry == null) 
				return;

			using var stream = entry.Open();
			using var reader = new StreamReader(stream);
			ProjectContent = reader.ReadToEnd();
		}

		private IEnumerable<string> GetSourceFiles()
		{
			var matches = Regex.Matches(ProjectContent, "<Compile\\s+Include=\"(?<file>.*)\"\\s*/>");

			foreach (Match match in matches)
			{
				if (!match.Success)
					continue;

				var file = match.Groups["file"].Value;
				if (!Exclude.Contains(file))
					yield return file;
			}
		}

		public bool TryGetMetadataReference(string assemblyName, [NotNullWhen(true)] out MetadataReference? reference)
		{
			reference = null;

			if (TryGetAssemblyPath(assemblyName, out var path))
				reference = MetadataReference.CreateFromFile(path);

			if (reference == null && TryGetAssemblyBytes(assemblyName, out var stream))
				reference = MetadataReference.CreateFromImage(stream);

			return reference != null;
		}

		public bool TryGetAssemblyPath(string assemblyName, out string path)
		{
			path = Path.Combine(Installation.Managed, $"{assemblyName}.dll");
			return File.Exists(path);
		}

		public static bool TryGetAssemblyBytes(string assemblyName, [NotNullWhen(true)] out byte[]? buffer)
		{
			try
			{
				buffer = Resources.ResourceManager.GetObject(assemblyName) as byte[];
			} catch
			{
				buffer = null;
			}
			return buffer != null;
		}

		public IEnumerable<MetadataReference> GetReferences()
		{
			yield return MetadataReference.CreateFromFile(Path.Combine(Installation.Managed, "mscorlib.dll"));

			var matches = Regex.Matches(ProjectContent, "<Reference\\s+Include=\"(?<assemblyName>.*)\"\\s*/?>");

			foreach (Match match in matches)
			{
				if (!match.Success)
					continue;

				var assemblyName = match.Groups["assemblyName"].Value;
				if (TryGetMetadataReference(assemblyName, out var reference))
					yield return reference;
			}
		}

		public IEnumerable<SyntaxTree> GetSyntaxTrees()
		{
			var options = CSharpParseOptions
				.Default
				.WithLanguageVersion(LanguageVersion.CSharp9)
				.WithPreprocessorSymbols(Defines);

			foreach (var file in GetSourceFiles())
			{
				var entry = ProjectArchive.Entries.FirstOrDefault(e => e.FullName.EndsWith(file.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
				if (entry == null)
					continue;

				using var stream = entry.Open();
				using var reader = new StreamReader(stream);

				var text = reader.ReadToEnd();
				var sourceText = SourceText.From(text, Encoding.UTF8);
				yield return SyntaxFactory.ParseSyntaxTree(sourceText, options, file);
			}
		}

		public CSharpCompilation Compile()
		{
			const string assemblyName = "NLog.EFT.Trainer";
			return CSharpCompilation.Create(assemblyName, GetSyntaxTrees(), GetReferences(), CompilationOptions);
		}
	}
}
