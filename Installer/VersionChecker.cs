#pragma warning disable IDE0079

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Installer;

internal class VersionChecker
{

	private static readonly Dictionary<Version, bool> _versions = [];
	private static readonly HttpClient _client = new();
	private static readonly SemaphoreSlim _semaphore = new(1, 1);

	public static async Task<bool> IsVersionSupportedAsync(Version version)
	{
		await _semaphore.WaitAsync();

		try
		{
			if (_versions.TryGetValue(version, out var supported))
				return supported;

			var branch = $"dev-{version}";
			var uri = new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/tree/{branch}");
			var result = await _client.GetAsync(uri);
			_versions[version] = result.IsSuccessStatusCode;
		}
		catch (Exception e)
		{
#if DEBUG
			Spectre.Console.AnsiConsole.WriteException(e);
#endif
			_ = e;
			_versions[version] = false;
		}
		finally
		{
			_semaphore.Release();
		}

		return _versions[version];
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
