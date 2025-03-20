using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using EFT.Trainer;
using JetBrains.Annotations;

[BepInPlugin(PluginId, "SPT.EftTrainer", "1.0.0")]
[UsedImplicitly]
public class SptEftTrainerPlugin : BaseUnityPlugin
{
	private const string PluginId = "com.SPT.efttrainer";
	public static bool Loaded = false;

	[UsedImplicitly]
	public void Awake()
	{
		if (Loaded)
			return;

		Loader.Load();
		Loaded = true;

		HandleSptBetaReleases();
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
	private static void HandleSptBetaReleases()
	{
		// Whitelist this plugin for spt beta releases
		var menuNotificationManager = Type.GetType("SPT.Custom.Utils.MenuNotificationManager, spt-custom", throwOnError: false);
		if (menuNotificationManager == null)
			return;

		var hashField = GetStaticField(menuNotificationManager, "whitelistedPlugins");
		if (hashField == null)
			return;

		var hashset = hashField.GetValue(null) as HashSet<string>;
		hashset?.Add(PluginId);

		_commitHash = GetStaticField(menuNotificationManager, "commitHash");
	}

	private static FieldInfo GetStaticField(Type type, string name)
	{
		return type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
			   ?? type.GetField(char.ToUpper(name[0]) + name.Substring(1), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
	}
}
