using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

		public static readonly CSharpCompilationOptions CompilationOptions =
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithOverflowChecks(true)
				.WithOptimizationLevel(OptimizationLevel.Release);

		public Compiler(ZipArchive projectArchive, Installation installation)
		{
			ProjectArchive = projectArchive;
			Installation = installation;

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
				if (match.Success)
					yield return match.Groups["file"].Value;
			}
		}

		public IEnumerable<MetadataReference> GetReferences()
		{
			yield return MetadataReference.CreateFromFile(Path.Combine(Installation.Managed, "mscorlib.dll"));

			var matches = Regex.Matches(ProjectContent, "<Reference\\s+Include=\"(?<reference>.*)\"\\s*/?>");

			foreach (Match match in matches)
			{
				if (!match.Success)
					continue;

				var reference = match.Groups["reference"].Value;
				yield return MetadataReference.CreateFromFile(Path.Combine(Installation.Managed, $"{reference}.dll"));
			}
		}

		public IEnumerable<SyntaxTree> GetSyntaxTrees()
		{
			var options = CSharpParseOptions
				.Default
				.WithLanguageVersion(LanguageVersion.CSharp9);

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
