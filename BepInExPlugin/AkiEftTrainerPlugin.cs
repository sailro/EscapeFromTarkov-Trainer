using BepInEx;
using EFT.Trainer;
using JetBrains.Annotations;

[BepInPlugin("com.spt-aki.efttrainer", "AKI.EftTrainer", "1.0.0")]
[UsedImplicitly]
public class AkiDebuggingPlugin : BaseUnityPlugin
{
	public static bool Loaded = false;

	[UsedImplicitly]
	public void Awake()
	{
		if (Loaded) 
			return;

		Loader.Load();
		Loaded = true;
	}

}
