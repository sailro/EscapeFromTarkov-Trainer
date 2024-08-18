using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Resources.NetStandard;
using System.Text;
using System.Text.RegularExpressions;
using Installer.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Installer;

internal partial class Compiler
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
		var matches = CompileFileRegex().Matches(ProjectContent);

		foreach (var match in matches.Cast<Match>())
		{
			if (!match.Success)
				continue;

			var file = match.Groups["file"].Value;
			if (!Exclude.Contains(file, StringComparer.OrdinalIgnoreCase))
				yield return file;
			else
			{
#if DEBUG
				Spectre.Console.AnsiConsole.MarkupLine($"[grey]>> Excluding {Spectre.Console.StringExtensions.EscapeMarkup(file)}.[/]");
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
			Spectre.Console.AnsiConsole.MarkupLine($"[grey]>> Resolved {assemblyName} to {Spectre.Console.StringExtensions.EscapeMarkup(path)}.[/]");
#endif
		}

		if (reference == null && TryGetAssemblyBytes(assemblyName, out var stream))
		{
#if DEBUG
			Spectre.Console.AnsiConsole.MarkupLine($"[grey]>> Using memory image for {assemblyName}.[/]");
#endif
			reference = MetadataReference.CreateFromImage(stream);
		}

		if (reference == null)
		{
#if DEBUG
			Spectre.Console.AnsiConsole.MarkupLine($"[grey]>> Unable to resolve {assemblyName}.[/]");
#endif
		}

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
		}
		catch
		{
			buffer = null;
		}
		return buffer != null;
	}

	private IEnumerable<MetadataReference> GetReferences()
	{
		yield return MetadataReference.CreateFromFile(Path.Combine(Installation.Managed, "mscorlib.dll"));

		var matches = ProjectReferenceRegex().Matches(ProjectContent);

		foreach (var match in matches.Cast<Match>())
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

	public bool IsLocalizationSupported()
	{
		return IsLanguageSupported(null);
	}

	public bool IsLanguageSupported(CompilationContext? context)
	{
		return GetSourceFiles().Any(f => f.EndsWith(string.Concat("Strings.", context?.Language ?? string.Empty, ".Designer.cs").Replace("..", "."), StringComparison.OrdinalIgnoreCase));
	}

	public IEnumerable<ResourceDescription> GetResources(CompilationContext context)
	{
		var matches = ResourceFileRegex().Matches(ProjectContent);

		foreach (var match in matches.Cast<Match>())
		{
			if (!match.Success)
				continue;

			// For now we only select one resource file, and use it as "neutral"
			var file = match.Groups["file"].Value;
			if (!file.EndsWith(string.Concat("Strings.", context.Language, ".resx").Replace("..", "."), StringComparison.OrdinalIgnoreCase))
				continue;

			var entry = ProjectArchive.Entries.FirstOrDefault(e => e.FullName.EndsWith(file.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
			if (entry == null)
				continue;

			using var stream = entry.Open();
			using var reader = new ResXResourceReader(stream);

			using var memory = new MemoryStream();
			using var writer = new ResourceWriter(memory);

			foreach (DictionaryEntry resourcEntry in reader)
				writer.AddResource(resourcEntry.Key.ToString()!, resourcEntry.Value);

			var resource = new MemoryStream();

			writer.Generate();
			memory.Position = 0;
			memory.CopyTo(resource);
			resource.Position = 0;

			var resourceName = file
				.Replace(@"Properties\Strings", "EFT.Trainer.Properties.Strings")
				.Replace($".{context.Language}.", ".", StringComparison.OrdinalIgnoreCase)
				.Replace(".resx", ".resources");

			yield return new ResourceDescription(resourceName, () => resource, isPublic: true);
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

	[GeneratedRegex("<Compile\\s+Include=\"(?<file>.*)\"\\s*/?>")]
	private static partial Regex CompileFileRegex();

	[GeneratedRegex("<(Project)?Reference\\s+Include=\"(?<assemblyName>.*)\"\\s*/?>")]
	private static partial Regex ProjectReferenceRegex();

	[GeneratedRegex("<EmbeddedResource\\s+Include=\"(?<file>.*)\"\\s*/?>")]
	private static partial Regex ResourceFileRegex();
}
