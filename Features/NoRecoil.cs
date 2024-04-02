using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoRecoil : ToggleFeature
{
	public override string Name => "norecoil";
	public override string Description => "No recoil.";

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
