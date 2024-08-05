using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoRecoil : ToggleFeature
{
	public override string Name => Strings.FeatureNoRecoilName;
	public override string Description => Strings.FeatureNoRecoilDescription;

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		if (player.ProceduralWeaponAnimation == null)
			return;

		var effect = player.ProceduralWeaponAnimation.Shootingg?.CurrentRecoilEffect;
		if (effect == null)
			return;

		effect.CameraRotationRecoilEffect.Intensity = 0f;
		effect.HandPositionRecoilEffect.Intensity = 0f;
		effect.HandRotationRecoilEffect.Intensity = 0f;
	}
}
