using EFT.Trainer.Features;
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
			HookObject.AddComponent<GameState>();
			HookObject.AddComponent<ExfiltrationPoints>();
			HookObject.AddComponent<Hud>();
			HookObject.AddComponent<Players>();
			//HookObject.AddComponent<Doors>(); disabling for now, we are impacting badly load performance
		}
	}
}
