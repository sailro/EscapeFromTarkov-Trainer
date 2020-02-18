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
				if (result == null)
				{
					result = new GameObject("Trainer");
					Object.DontDestroyOnLoad(result);
				}
				return result;
			}
		}

		public static TrainerBehaviour Trainer
		{
			get
			{
				return HookObject.GetComponent<TrainerBehaviour>();
			}
		}

		public static void Load()
		{
			HookObject.AddComponent<TrainerBehaviour>();
		}

		public static void Unload()
		{
			Object.DestroyImmediate(Trainer);
			Object.DestroyImmediate(HookObject);
		}
	}
}
