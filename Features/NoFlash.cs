using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoFlash : ToggleFeature
{
	public override string Name => Strings.FeatureNoFlashName;
	public override string Description => Strings.FeatureNoFlashDescription;

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var camera = GameState.Current?.Camera;
		if (camera == null)
			return;

		if (camera.GetComponent<GrenadeFlashScreenEffect>() is { enabled: true } flash)
		{
			flash.enabled = false;
			flash.EffectStrength = 0;
		}

		if (camera.GetComponent<EyeBurn>() is { enabled: true } eyeburn)
		{
			eyeburn.enabled = false;
			eyeburn.EyesBurn = false;
		}
	}
}
