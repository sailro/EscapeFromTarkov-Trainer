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
using Spectre.Console; // used in #DEBUG ifdef

#nullable enable

namespace Installer
{
	internal class Compiler
	{
		private ZipArchive ProjectArchive { get; }
		private Installation Installation { get; }
		private string ProjectContent { get; }

		private string[] Exclude { get; }
		private string[] Defines { get; } = [];

		private static CSharpCompilationOptions CompilationOptions { get; } =
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithOverflowChecks(true)
				.WithOptimizationLevel(OptimizationLevel.Release);

		public Compiler(ZipArchive projectArchive, CompilationContext context)
		{
			ProjectArchive = projectArchive;
			Installation = context.Installation;
			Exclude = context.Exclude;
			ProjectContent = string.Empty;

			var entry = projectArchive.Entries.FirstOrDefault(e => e.Name == context.Project) ?? throw new ArgumentException($"Project {context.Project} not found!");
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
				if (!Exclude.Contains(file, StringComparer.OrdinalIgnoreCase))
					yield return file;
				else
				{
#if DEBUG
					AnsiConsole.MarkupLine($"[grey]>> Excluding {file.EscapeMarkup()}.[/]");
#endif
				}
			}
		}

		private bool TryGetMetadataReference(string assemblyName, [NotNullWhen(true)] out MetadataReference? reference)
		{
			reference = null;

			if (TryGetAssemblyPath(assemblyName, out var path))
			{
				reference = MetadataReference.CreateFromFile(path);
#if DEBUG
				AnsiConsole.MarkupLine($"[grey]>> Resolved {assemblyName} to {path}.[/]");
#endif
			}

			if (reference == null && TryGetAssemblyBytes(assemblyName, out var stream))
			{
#if DEBUG
				AnsiConsole.MarkupLine($"[grey]>> Using memory image for {assemblyName}.[/]");
#endif
				reference = MetadataReference.CreateFromImage(stream);
			}

#if DEBUG
			if (reference == null)
			{
				AnsiConsole.MarkupLine($"[grey]>> Unable to resolve {assemblyName}.[/]");
			}
#endif

			return reference != null;
		}

		private bool TryGetAssemblyPath(string assemblyName, out string path)
		{
			path = Path.Combine(Installation.Managed, $"{assemblyName}.dll");
			if (!File.Exists(path))
				path = Path.Combine(Installation.BepInExCore, $"{assemblyName}.dll");

			return File.Exists(path);
		}

		private static bool TryGetAssemblyBytes(string assemblyName, [NotNullWhen(true)] out byte[]? buffer)
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

		private IEnumerable<MetadataReference> GetReferences()
		{
			yield return MetadataReference.CreateFromFile(Path.Combine(Installation.Managed, "mscorlib.dll"));

			var matches = Regex.Matches(ProjectContent, "<(Project)?Reference\\s+Include=\"(?<assemblyName>.*)\"\\s*/?>");

			foreach (Match match in matches)
			{
				if (!match.Success)
					continue;

				var assemblyName = match.Groups["assemblyName"].Value;
				// We expect project reference to be compiled first
				assemblyName = Path
					.GetFileName(assemblyName)
					.Replace(".csproj", string.Empty);

				if (TryGetMetadataReference(assemblyName, out var reference))
					yield return reference;
			}
		}

		private IEnumerable<SyntaxTree> GetSyntaxTrees()
		{
			var options = CSharpParseOptions
				.Default
				.WithLanguageVersion(LanguageVersion.Latest)
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

		public CSharpCompilation Compile(string assemblyName)
		{
			var syntaxTrees = GetSyntaxTrees()
				.ToArray();

			var references = GetReferences()
				.ToArray();
			
			return CSharpCompilation.Create(assemblyName, syntaxTrees, references, CompilationOptions);
		}
	}
}
