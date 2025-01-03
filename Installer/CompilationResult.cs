using System.IO.Compression;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Installer;

internal class CompilationResult(CSharpCompilation? compilation, ZipArchive? archive, Diagnostic[] errors, ResourceDescription[] resources)
{
	public CSharpCompilation? Compilation { get; } = compilation;
	public ZipArchive? Archive { get; } = archive;
	public Diagnostic[] Errors { get; } = errors;
	public ResourceDescription[] Resources { get; } = resources;

	public string[] ErrorFiles
	{
		get
		{
			return Errors
				.Select(d => d.Location.SourceTree?.FilePath)
				.Where(s => s is not null)
				.OfType<string>()
				.Distinct()
				.ToArray();
		}
	}
}
