using System;
using EFT.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal abstract class Feature : MonoBehaviour
	{
		public abstract string Name { get; }

#if HARMONY
		private string? _harmonyId = null;

		public void HarmonyPatchOnce(Action<HarmonyLib.Harmony> action)
		{
			if (_harmonyId != null) // this is faster than calling HarmonyLib.Harmony.HasAnyPatches(_harmonyId) for every Update
				return;

			_harmonyId = GetType().FullName;
			var harmony = new HarmonyLib.Harmony(_harmonyId);
			action(harmony);
		}
#endif

		protected void AddConsoleLog(string log, string? from = null)
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from ?? Name);
		}
	}
}
