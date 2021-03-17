using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using EFT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EFT.Trainer.Features
{
	public class Commands : MonoBehaviour
	{
		public bool Registered { get; set; } = false;
		public const string ValueGroup = "value";
		private readonly Dictionary<string, Type> _features = new()
		{
			{"exfil", typeof(ExfiltrationPoints)},
			{"hud", typeof(Hud)},
			{"wallhack", typeof(Players)},
			{"norecoil", typeof(Recoil)},
			{"quest", typeof(Quests)}
		};

		public void Update()
		{
			if (Registered)
				return;

			if (!PreloaderUI.Instantiated)
				return;

			RegisterCommands();
		}

		private void RegisterCommands()
		{
			var commands = ConsoleScreen.Commands;
			if (commands.Count == 0)
				return;

			foreach (var (featureName, featureType) in _features)
			{
				commands.AddCommand(new GClass1907($"{featureName} (?<{ValueGroup}>(on)|(off))", m => OnTriggerFeature(featureType, m)));
			}

			commands.AddCommand(new GClass1907("dump", _ => Dump()));
			commands.AddCommand(new GClass1907("status", _ => Status()));

			Registered = true;
			Destroy(this);
		}

		private void Status()
		{
			foreach (var (featureName, featureType) in _features)
			{
				if (Loader.HookObject.GetComponent(featureType) is not IEnableable feature)
					return;

				PreloaderUI.Instance.Console.AddLog($"{featureName} is {(feature.Enabled ? "on" : "off")}", "status");
			}
		}

		private static void Dump()
		{
			var dumpfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov", "Dumps");
			var thisDump = Path.Combine(dumpfolder, $"{DateTime.Now:yyyyMMdd-HHmmss}");

			Directory.CreateDirectory(thisDump);

			PreloaderUI.Instance.Console.AddLog("Dumping scenes...", "dump");
			for (int i = 0; i < SceneManager.sceneCount; i++) 
			{
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded)
					continue;

				var json = SceneDumper.DumpScene(scene).ToPrettyJson();
				File.WriteAllText(Path.Combine(thisDump, GetSafeFilename($"@scene - {scene.name}.txt")), json);
			}

			PreloaderUI.Instance.Console.AddLog("Dumping game objects...", "dump");
			foreach (var go in FindObjectsOfType<GameObject>())
			{
				if (go == null || go.transform.parent != null || !go.activeSelf) 
					continue;

				var filename = GetSafeFilename(go.name + "-" + go.GetHashCode() + ".txt");
				var json = SceneDumper.DumpGameObject(go).ToPrettyJson();
				File.WriteAllText(Path.Combine(thisDump, filename), json);
			}

			PreloaderUI.Instance.Console.AddLog($"Dump created in {thisDump}", "dump");
		}

		private static string GetSafeFilename(string filename)
		{
			return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));  	
		}

		public void OnTriggerFeature(Type featureType, Match match)
		{
			var matchGroup = match?.Groups[ValueGroup];
			if (matchGroup == null || !matchGroup.Success)
				return;

			if (Loader.HookObject.GetComponent(featureType) is not IEnableable feature)
				return;

			feature.Enabled = matchGroup.Value switch
			{
				"on" => true,
				"off" => false,
				_ => feature.Enabled
			};
		}
	}
}
