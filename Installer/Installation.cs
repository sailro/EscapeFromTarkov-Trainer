using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Spectre.Console;

namespace Installer;

internal class Installation
{
	public Version Version { get; }
	public bool UsingSpt { get; private set; }
	public bool UsingSptButNeverRun { get; private set; }
	public bool UsingBepInEx { get; private set; }
	public string Location { get; }
	public string DisplayString { get; private set; } = string.Empty;

	public string Data => Path.Combine(Location, "EscapeFromTarkov_Data");
	public string Managed => Path.Combine(Data, "Managed");
	public string BepInEx => Path.Combine(Location, "BepInEx");
	public string BepInExCore => Path.Combine(BepInEx, "core");
	public string BepInExPlugins => Path.Combine(BepInEx, "plugins");

	private Installation(string location, Version version)
	{
		if (string.IsNullOrEmpty(location))
			throw new ArgumentException("empty location");

		Location = location;
		Version = version;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not Installation other)
			return false;

		return other.Location == Location;
	}

	public override int GetHashCode()
	{
		return Location.GetHashCode();
	}

	[SupportedOSPlatform("windows")]
	public static Installation? GetTargetInstallation(string? path, string promptTitle)
	{
		var installations = new List<Installation>();

		AnsiConsole
			.Status()
			.Start("Discovering [green]Escape From Tarkov[/] installations...", _ =>
			{
				installations = [.. DiscoverInstallations().Distinct()];

				if (path is not null && TryDiscoverInstallation(path, out var installation))
					installations.Add(installation);
			});

		installations = [.. installations.Distinct().OrderBy(i => i.Location)];

		switch (installations.Count)
		{
			case 0:
				AnsiConsole.MarkupLine("[yellow]No [green]EscapeFromTarkov[/] installation found, please re-run this installer, passing the installation path as argument.[/]");
				return null;
			case 1:
				var first = installations.First();
				return AnsiConsole.Confirm($"Continue with [green]EscapeFromTarkov ({first.Version})[/] in [blue]{first.Location.EscapeMarkup()}[/] ?") ? first : null;
			default:
				var prompt = new SelectionPrompt<Installation> { Title = promptTitle };
				prompt.AddChoices(installations);
				return AnsiConsole.Prompt(prompt);
		}
	}

	[SupportedOSPlatform("windows")]
	private static IEnumerable<Installation> DiscoverInstallations()
	{
		if (TryDiscoverInstallation(Environment.CurrentDirectory, out var installation))
			yield return installation;

		if (TryDiscoverInstallation(Path.GetDirectoryName(AppContext.BaseDirectory), out installation))
			yield return installation;

		// SPT default installation path
		if (TryDiscoverInstallation(Path.Combine(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))!, "SPT"), out installation))
			yield return installation;

		// SPT locations from MUI cache
		foreach (var sptpath in Registry.GetSptInstallationsFromMuiCache())
		{
			if (TryDiscoverInstallation(sptpath, out installation))
				yield return installation;
		}

		if (!Registry.TryGetEscapeFromTarkovInstallationPath(out var path))
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

	private static bool TryDiscoverInstallation(string? path, [NotNullWhen(true)] out Installation? installation)
	{
		installation = null;

		try
		{
			if (string.IsNullOrEmpty(path))
				return false;

			path = Path.GetFullPath(path.Trim('\"'));
			var exe = Path.Combine(path, "EscapeFromTarkov.exe");
			if (!File.Exists(exe))
				return false;

			var vi = FileVersionInfo.GetVersionInfo(exe);
			if (vi.FileVersion == null)
				return false;

			installation = new Installation(path, new Version(vi.FileVersion));

			if (!Directory.Exists(installation.Managed))
				return false;

			var sptFolder = Path.Combine(path, "SPT_Data");
			installation.UsingSpt = Directory.Exists(sptFolder);


			var battleye = Path.Combine(path, "BattlEye");
			var user = Path.Combine(path, "user");
			installation.UsingSptButNeverRun = installation.UsingSpt && (Directory.Exists(battleye) || !Directory.Exists(user));

			installation.UsingBepInEx = Directory.Exists(installation.BepInExPlugins);

			installation.DisplayString = installation.ComputeDisplayString();

			return true;
		}
		catch (IOException)
		{
			return false;
		}
	}

	private string ComputeDisplayString()
	{
		var sb = new StringBuilder();
		sb.Append($"{Location.EscapeMarkup()} - [[{Version}]] ");
		sb.Append(UsingSpt ? "[b]SPT[/] " : "Vanilla ");

		if (UsingSpt && VersionChecker.IsVersionSupported(Version))
			sb.Append("[green](Supported)[/]");

		return sb.ToString();
	}

	public override string ToString()
	{
		return DisplayString;
	}
}
