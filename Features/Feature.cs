using System;
using EFT.UI;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal interface IFeature
	{
		[JsonIgnore]
		public string Name { get; }
	}

	internal abstract class Feature : MonoBehaviour, IFeature
	{
		public abstract string Name { get; }

		private string? _harmonyId = null;

		public void HarmonyPatchOnce(Action<HarmonyLib.Harmony> action)
		{
			if (_harmonyId != null) // this is faster than calling HarmonyLib.Harmony.HasAnyPatches(_harmonyId) for every Update
				return;

			_harmonyId = GetType().FullName;
			var harmony = new HarmonyLib.Harmony(_harmonyId);
			action(harmony);
		}

		protected void AddConsoleLog(string log)
		{
			if (PreloaderUI.Instantiated)
				ConsoleScreen.Log(log);
		}
	}
}
