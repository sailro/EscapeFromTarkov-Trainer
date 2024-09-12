using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

namespace Installer;

internal class VersionChecker
{

	private static readonly Dictionary<Version, bool> _versions = [];
	private static readonly HttpClient _client = new();

	public static async Task<bool> IsVersionSupportedAsync(Version version)
	{
		if (_versions.TryGetValue(version, out var supported))
			return supported;

		try
		{
			var branch = $"dev-{version}";
			var uri = new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/tree/{branch}");
			var result = await _client.GetAsync(uri);
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
