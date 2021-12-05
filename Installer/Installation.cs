using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Spectre.Console;

#nullable enable

namespace Installer
{
	internal class Installation
	{
		public Version Version { get; }
		public bool UsingSptAki { get; private set; }
		public string Location { get; }
		public string Data => Path.Combine(Location, "EscapeFromTarkov_Data");
		public string Managed => Path.Combine(Data, "Managed");

		public Installation(string location, Version version)
		{
			if (string.IsNullOrEmpty(location))
				throw new ArgumentException(nameof(location));

			Location = location;
			Version = version;
		}

		public override bool Equals(object obj)
		{
			if (obj is not Installation other)
				return false;

			return other.Location == Location;
		}

		public override int GetHashCode()
		{
			return Location.GetHashCode();
		}

		public static Installation? GetTargetInstallation(string? path, string promptTitle)
		{
			var installations = new List<Installation>();
			
			AnsiConsole
				.Status()
				.Start("Discovering [green]Escape From Tarkov[/] installations...", _ =>
				{
					installations = DiscoverInstallations()
						.Distinct()
						.ToList();
				});

			if (path is not null && TryDiscoverInstallation(path, out var installation))
				installations.Add(installation);

			installations = installations
				.Distinct()
				.OrderBy(i => i.Location)
				.ToList();

			switch (installations.Count)
			{
				case 0:
					AnsiConsole.MarkupLine("[yellow]No [green]EscapeFromTarkov[/] installation found, please re-run this installer, passing the installation path as argument.[/]");
					return null;
				case 1:
					var first = installations.First();
					return AnsiConsole.Confirm($"Continue with [green]EscapeFromTarkov ({first.Version})[/] in [blue]{first.Location.EscapeMarkup()}[/] ?") ? first : null;
				default:
					var prompt = new SelectionPrompt<Installation>
					{
						Converter = i => i.Location.EscapeMarkup(),
						Title = promptTitle
					};
					prompt.AddChoices(installations);
					return AnsiConsole.Prompt(prompt);
			}
		}

		private static IEnumerable<Installation> DiscoverInstallations()
		{
			if (TryDiscoverInstallation(Environment.CurrentDirectory, out var installation))
				yield return installation;

			if (TryDiscoverInstallation(Path.GetDirectoryName(typeof(Installation).Assembly.Location)!, out installation))
				yield return installation;

			using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
			using var eft = hive.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov", false);

			if (eft == null)
				yield break;

			var exe = eft.GetValue("DisplayIcon") as string;
			if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
				yield break;

			var path = Path.GetDirectoryName(exe);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				yield break;

			if (TryDiscoverInstallation(path, out installation))
				yield return installation;

			var subFolders = Directory.EnumerateDirectories(Path.Combine(path, ".."));
			foreach (var folder in subFolders)
			{
				if (TryDiscoverInstallation(folder, out installation))
					yield return installation;
			}
		}

		private static bool TryDiscoverInstallation(string path, [NotNullWhen(true)] out Installation? installation)
		{
			installation = null;

			try
			{
				path = Path.GetFullPath(path);
				var exe = Path.Combine(path, "EscapeFromTarkov.exe");
				if (!File.Exists(exe))
					return false;

				var vi = FileVersionInfo.GetVersionInfo(exe);
				installation = new Installation(path, new Version(vi.FileVersion));

				if (!Directory.Exists(installation.Managed))
					return false;

				var akiFolder = Path.Combine(path, "Aki_Data");
				installation.UsingSptAki = Directory.Exists(akiFolder);

				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}
	}
}
