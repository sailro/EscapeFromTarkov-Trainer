using System.IO.Compression;

namespace Installer;

internal class CompilationContext(Installation installation, string projectTitle, string project)
{
	internal const string DefaultBranch = "master";

	public int Try { get; set; } = 0;
	public Installation Installation { get; set; } = installation;
	public string ProjectTitle { get; set; } = projectTitle;
	public string Project { get; set; } = project;
	public string Branch { get; set; } = DefaultBranch;
	public string[] Exclude { get; set; } = [];
	public ZipArchive? Archive { get; set; }
	public bool IsFatalFailure { get; set; } = false;
}
