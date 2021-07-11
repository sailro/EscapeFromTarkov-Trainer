using Spectre.Console.Cli;

#nullable enable

namespace Installer
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			var app = new CommandApp<InstallCommand>();
			return app.Run(args);
		}
	}
}
