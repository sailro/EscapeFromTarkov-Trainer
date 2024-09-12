using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

namespace Installer;

internal class VersionChecker
{

	private static readonly Dictionary<Version, bool> _versions = [];

	public static async Task<bool> IsVersionSupportedAsync(Version version)
	{
		if (_versions.TryGetValue(version, out var supported))
			return supported;

		try
		{
			var branch = $"dev-{version}";
			using var client = new HttpClient();
			var result = await client.GetAsync(new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/archive/refs/heads/{branch}.zip"));
			_versions[version] = result.IsSuccessStatusCode;
			return _versions[version];
		}
		catch (Exception e)
		{
#if DEBUG
			AnsiConsole.WriteException(e);
#endif

			return false;
		}
	}

	public static bool IsVersionSupported(Version version)
	{
#pragma warning disable VSTHRD002
		return IsVersionSupportedAsync(version)
			.GetAwaiter()
			.GetResult();
#pragma warning restore VSTHRD002
	}
}
