using UnityEngine;

namespace EFT.Trainer
{
	public class Loader
	{
		public static GameObject HookObject
		{
			get
			{
				var result = GameObject.Find("Application (Main Client)");
				if (result != null)
					return result;

				result = new GameObject(nameof(Loader));
				Object.DontDestroyOnLoad(result);
				return result;
			}
		}

		public static void Load()
		{
			HookObject.AddComponent<Features.GameState>();
			HookObject.AddComponent<Features.ExfiltrationPoints>();
			HookObject.AddComponent<Features.Hud>();
			HookObject.AddComponent<Features.Players>();
			HookObject.AddComponent<Features.Doors>();
			HookObject.AddComponent<Features.NoRecoil>();
			HookObject.AddComponent<Features.Quests>(); 
			HookObject.AddComponent<Features.LootItems>();
			HookObject.AddComponent<Features.LootableContainers>();
			HookObject.AddComponent<Features.AutomaticGun>();
			HookObject.AddComponent<Features.Stamina>();
			HookObject.AddComponent<Features.NightVision>();
			HookObject.AddComponent<Features.ThermalVision>();
			HookObject.AddComponent<Features.CrossHair>();
			HookObject.AddComponent<Features.Commands>();
		}
	}
}
