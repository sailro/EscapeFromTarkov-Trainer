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

				result = new GameObject("Trainer");
				Object.DontDestroyOnLoad(result);
				return result;
			}
		}

		public static void Load()
		{
			HookObject.AddComponent<TrainerBehaviour>();
		}
	}
}
