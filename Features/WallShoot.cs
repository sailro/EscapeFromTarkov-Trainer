using EFT.Ballistics;
using EFT.Trainer.Extensions;
using EFT.Trainer.Model;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class WallShoot : ToggleFeature
{
	public override string Name => Strings.FeatureWallShootName;
	public override string Description => Strings.FeatureFeatureWallShootDescription;

#pragma warning disable IDE0060 
	[UsedImplicitly]
	protected static bool IsPenetratedPrefix(object shot, Vector3 hitPoint, BallisticCollider __instance, ref bool __result)
	{
		var feature = FeatureFactory.GetFeature<WallShoot>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		var shotWrapper = new ShotWrapper(shot);
		var player = shotWrapper.Player;
		if (player is not { IsYourPlayer: true })
			return true; // keep using original code for other players

		__result = true;
		__instance.PenetrationChance = 1.0f;
		__instance.PenetrationLevel = 0.0f;
		__instance.RicochetChance = 0.0f;
		__instance.FragmentationChance = 0.0f;
		__instance.TrajectoryDeviationChance = 0.0f;
		__instance.TrajectoryDeviation = 0.0f;

		return false; // don't call the original code
	}
#pragma warning restore IDE0060

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(BallisticCollider), nameof(BallisticCollider.IsPenetrated), nameof(IsPenetratedPrefix));
		});
	}
}
