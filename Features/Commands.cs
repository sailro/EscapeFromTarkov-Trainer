using System.Text.RegularExpressions;
using EFT.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Commands : MonoBehaviour
	{
		public bool Registered { get; set; } = false;
		public const string ValueGroup = "value";

		public void Update()
		{
			if (Registered)
				return;

			if (!PreloaderUI.Instantiated)
				return;

			var commands = ConsoleScreen.Commands;
			if (commands.Count == 0)
				return;

			commands.AddCommand(new GClass1907($"exfil (?<{ValueGroup}>(on)|(off))", OnTriggerFeature<ExfiltrationPoints>));
			commands.AddCommand(new GClass1907($"hud (?<{ValueGroup}>(on)|(off))", OnTriggerFeature<Hud>));
			commands.AddCommand(new GClass1907($"wallhack (?<{ValueGroup}>(on)|(off))", OnTriggerFeature<Players>));
			commands.AddCommand(new GClass1907($"norecoil (?<{ValueGroup}>(on)|(off))", OnTriggerFeature<Recoil>));

			Registered = true;

			Destroy(this);
		}

		public void OnTriggerFeature<T>(Match match) where T : MonoBehaviour, IEnableable
		{
			var feature = Loader.HookObject.GetComponent<T>();
			if (feature == null)
				return;

			switch (match.Groups[ValueGroup].Value)
			{
				case "on":
					feature.Enabled = true;
					break;
				case "off":
					feature.Enabled = false;
					break;
			}
		}
	}
}
