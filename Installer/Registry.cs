using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Installer
{
	[SupportedOSPlatform("windows")]
	internal class Registry
	{
		public static bool TryGetEscapeFromTarkovInstallationPath([NotNullWhen(true)] out string? installationPath)
		{
			installationPath = null;

			try
			{
				using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
				using var eft = hive.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov", false);

				if (eft == null)
					return false;

				var exe = eft.GetValue("DisplayIcon") as string;
				if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
					return false;

				var path = Path.GetDirectoryName(exe);
				if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
					return false;

				installationPath = path;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static IEnumerable<string?> GetSptInstallationsFromMuiCache()
		{
			try
			{
				using var hive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
				using var mui = hive.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache", false);

				if (mui == null)
					return [];

				const string attribute = ".FriendlyAppName";
				string[] candidates = ["SPT.Launcher.exe", "SPT.Server.exe"];

				return mui
					.GetValueNames()
					.Where(v => candidates.Any(c => v.Contains($"{c}{attribute}", StringComparison.OrdinalIgnoreCase)))
					.Select(v => Path.GetDirectoryName(v.Replace(attribute, string.Empty)))
					.Distinct();
			}
			catch
			{
				return [];
			}
		}
	}
}
