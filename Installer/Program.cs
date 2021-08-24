using Spectre.Console.Cli;

#nullable enable

namespace Installer
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			var app = new CommandApp<InstallCommand>();
			app.Configure(config =>
			{
				config
					.AddCommand<InstallCommand>("install")
					.WithDescription("Install the trainer");
				
				config
					.AddCommand<UninstallCommand>("uninstall")
					.WithDescription("Uninstall the trainer");
			});
			return app.Run(args);
		}
	}
}
