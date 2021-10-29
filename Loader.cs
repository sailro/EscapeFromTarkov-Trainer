using UnityEngine;

#nullable enable

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
			var hookObject = HookObject;

			hookObject.AddComponent<Features.Aimbot>();
			hookObject.AddComponent<Features.GameState>();
			hookObject.AddComponent<Features.ExfiltrationPoints>();
			hookObject.AddComponent<Features.Hud>();
			hookObject.AddComponent<Features.Players>();
			hookObject.AddComponent<Features.WorldInteractiveObjects>();
			hookObject.AddComponent<Features.NoRecoil>();
			hookObject.AddComponent<Features.NoCollision>();
			hookObject.AddComponent<Features.Quests>(); 
			hookObject.AddComponent<Features.LootItems>();
			hookObject.AddComponent<Features.LootableContainers>();
			hookObject.AddComponent<Features.AutomaticGun>();
			hookObject.AddComponent<Features.Stamina>();
			hookObject.AddComponent<Features.NightVision>();
			hookObject.AddComponent<Features.ThermalVision>();
			hookObject.AddComponent<Features.NoVisor>();
			hookObject.AddComponent<Features.CrossHair>();
			hookObject.AddComponent<Features.Grenades>();
			hookObject.AddComponent<Features.WallShoot>();
			hookObject.AddComponent<Features.Speed>();
			hookObject.AddComponent<Features.Commands>();
		}
	}
}
