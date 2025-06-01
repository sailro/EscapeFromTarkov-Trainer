using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using EFT.Trainer;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

[BepInPlugin(PluginId, "SPT.EftTrainer", "1.0.0")]
[UsedImplicitly]
public class SptEftTrainerPlugin : BaseUnityPlugin
{
	private const string PluginId = "com.SPT.efttrainer";
	public static bool Loaded = false;

	[UsedImplicitly]
	public void Awake()
	{
		SetupSptBetaReleases();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		LoadTrainer();

		if (Loaded)
			SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	[UsedImplicitly]
	public void OnGUI()
	{
		RemoveSptBetaReleaseWatermark();
	}

	private void LoadTrainer()
	{
		if (Loaded)
			return;

		var scene = SceneManager.GetActiveScene();
		if (string.IsNullOrEmpty(scene.name))
			return;

		Logger.LogInfo($"Found {scene.name}, loading...");

		Loader.Load();
		Loaded = true;
	}

	private void RemoveSptBetaReleaseWatermark()
	{
		if (_commitHash == null)
			return;

		var hash = _commitHash.GetValue(null) as string;
		if (hash == string.Empty)
		{
			Logger.LogInfo("Disabling SPT hash monitoring");

			// Stop monitoring
			_commitHash = null;
			return;
		}

		// Suppress this dumb watermark
		Logger.LogInfo("Removing SPT watermark");
		_commitHash.SetValue(null, string.Empty);
	}

	private static FieldInfo _commitHash;
	private void SetupSptBetaReleases()
	{
		// Whitelist this plugin for spt beta releases
		var menuNotificationManager = Type.GetType("SPT.Custom.Utils.MenuNotificationManager, spt-custom", throwOnError: false);
		if (menuNotificationManager == null)
		{
			Logger.LogInfo("MenuNotificationManager not found");
			return;
		}

		var hashField = GetStaticField(menuNotificationManager, "whitelistedPlugins");
		if (hashField == null)
		{
			Logger.LogInfo("whitelistedPlugins not found");
			return;
		}

		Logger.LogInfo($"White-listing {PluginId}");
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
