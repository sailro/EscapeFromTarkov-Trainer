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

		WhitelistThisPlugin();
	}

	private static void WhitelistThisPlugin()
	{
		// Whitelist this plugin for spt-aki beta releases
		var type = Type.GetType("SPT.Custom.Utils.MenuNotificationManager, spt-custom", throwOnError: false);
		if (type == null)
			return;

		var field = type.GetField("whitelistedPlugins", BindingFlags.NonPublic | BindingFlags.Static);
		if (field == null)
			return;

		var hashset = field.GetValue(null) as HashSet<string>;
		hashset?.Add(PluginId);
	}
}
