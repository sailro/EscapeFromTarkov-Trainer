using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoSway : ToggleFeature
{
	public override string Name => "nosway";

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var weaponAnimation = player.ProceduralWeaponAnimation;
		if (weaponAnimation == null)
			return;

		var motionReact = weaponAnimation.MotionReact;
		motionReact.Intensity = 0f;
		motionReact.SwayFactors = Vector3.zero;
		motionReact.Velocity = Vector3.zero;

		weaponAnimation.Breath.Intensity = 0;
		weaponAnimation.Walk.Intensity = 0;
		weaponAnimation.Shootingg.Stiffness = 0;
		weaponAnimation.ForceReact.Intensity = 0;
		weaponAnimation.WalkEffectorEnabled = false;

	}
}
