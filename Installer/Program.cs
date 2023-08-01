using System;
using System.Diagnostics;
using System.Management;
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
			var result = app.Run(args);

			if (StartedByExplorer())
			{
				Console.WriteLine();
				Console.WriteLine(@"Press a key to exit...");
				Console.ReadKey();
			}

			return result;
		}

		private static bool StartedByExplorer()
		{
			try
			{
				const string ParentProcessId = nameof(ParentProcessId);
				var id = Process.GetCurrentProcess().Id;

				var query = $"SELECT {ParentProcessId} FROM Win32_Process WHERE ProcessId = {id}";
				var search = new ManagementObjectSearcher(@"root\CIMV2", query);
				
				var results = search.Get().GetEnumerator();
				if (!results.MoveNext())
					return true;

				var result = results.Current;
				var parentId = (uint)result[ParentProcessId];
				var parent = Process.GetProcessById((int)parentId);

				return parent.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return true;
			}
		}
	}
}
