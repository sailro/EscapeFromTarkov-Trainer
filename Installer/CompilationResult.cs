using System.IO.Compression;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Installer;

internal class CompilationResult(CSharpCompilation? compilation, ZipArchive? archive, Diagnostic[] errors, ResourceDescription[] resources)
{
	public CSharpCompilation? Compilation { get; } = compilation;
	public ZipArchive? Archive { get; } = archive;
	public Diagnostic[] Errors { get; } = errors;
	public ResourceDescription[] Resources { get; } = resources;
}
