using System;
using System.Runtime.Versioning;
using Spectre.Console.Cli;

namespace Installer;

internal class Program
{
	[SupportedOSPlatform("windows")]
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
		var result = app.Run(args);

		if (StartedByExplorer())
		{
			Console.WriteLine();
			Console.WriteLine(@"Press a key to exit...");
			Console.ReadKey();
		}

		return result;
	}

	[SupportedOSPlatform("windows")]
	private static bool StartedByExplorer()
	{
		try
		{
			var parent = ParentProcessHelper.GetParentProcess();
			return parent?.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase) ?? false;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
