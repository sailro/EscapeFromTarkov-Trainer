using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Win32;

#nullable enable

namespace Installer
{
	internal class Installation
	{
		public Version Version { get; }
		public Version? SptAkiVersion { get; private set; }
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

		public static IEnumerable<Installation> DiscoverInstallations()
		{
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

			if (TryDiscoverInstallation(path!, out var installation))
				yield return installation;

			var subFolders = Directory.EnumerateDirectories(Path.Combine(path!, ".."));
			foreach (var folder in subFolders)
			{
				if (TryDiscoverInstallation(folder, out installation))
					yield return installation;
			}
		}

		public static bool TryDiscoverInstallation(string path, [NotNullWhen(true)] out Installation? installation)
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

				var patchFolder = Path.Combine(path, "Aki_Data", "Launcher", "Patches");
				if (Directory.Exists(patchFolder))
				{
					installation.SptAkiVersion = Directory.EnumerateFiles(patchFolder, "*.bpf")
						.Select(n => Version.TryParse(Path.GetFileNameWithoutExtension(n), out var version) ? version : default)
						.FirstOrDefault();
				}

				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}
	}
}
