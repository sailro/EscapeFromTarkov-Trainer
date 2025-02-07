using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using EFT.Trainer;
using JetBrains.Annotations;

[BepInPlugin(PluginId, "AKI.EftTrainer", "1.0.0")]
[UsedImplicitly]
public class AkiDebuggingPlugin : BaseUnityPlugin
{
	private const string PluginId = "com.spt-aki.efttrainer";
	public static bool Loaded = false;

	[UsedImplicitly]
	public void Awake()
	{
		if (Loaded)
			return;

		Loader.Load();
		Loaded = true;

		HandleSptAkiBetaReleases();
	}

	[UsedImplicitly]
	public void OnGUI()
	{
		if (_commitHash == null)
			return;

		var hash = _commitHash.GetValue(null) as string;
		if (hash == string.Empty)
		{
			// Stop monitoring
			_commitHash = null;
			return;
		}

		// Suppress this dumb watermark
		_commitHash.SetValue(null, string.Empty);
	}

	private static FieldInfo _commitHash;
	private static void HandleSptAkiBetaReleases()
	{
		// Whitelist this plugin for spt-aki beta releases
		var menuNotificationManager = Type.GetType("SPT.Custom.Utils.MenuNotificationManager, spt-custom", throwOnError: false);
		if (menuNotificationManager == null)
			return;

		var hashField = menuNotificationManager.GetField("whitelistedPlugins", BindingFlags.NonPublic | BindingFlags.Static);
		if (hashField == null)
			return;

		var hashset = hashField.GetValue(null) as HashSet<string>;
		hashset?.Add(PluginId);

		_commitHash = menuNotificationManager.GetField("commitHash", BindingFlags.Public | BindingFlags.Static);

	}
}
