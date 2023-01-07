using System;
using System.IO.Compression;

#nullable enable

namespace Installer
{
	internal class CompilationContext
	{
		public int Try { get; set; } = 0;
		public Installation Installation { get; set; }
		public string ProjectTitle { get; set; }
		public string Project { get; set; }
		public string Branch { get; set; } = "master";
		public string[] Exclude { get; set; } = Array.Empty<string>();
		public ZipArchive? Archive { get; set; }

		public CompilationContext(Installation installation, string projectTitle, string project)
		{
			Installation = installation;
			ProjectTitle = projectTitle;
			Project = project;
		}
	}
}
