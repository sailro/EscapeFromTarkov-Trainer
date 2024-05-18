using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Ghost : ToggleFeature
{
	public override string Name => "ghost";
	public override string Description => "Stop bots from seeing you.";

	public override bool Enabled { get; set; } = false;

	[UsedImplicitly]
	private static bool CheckLookEnemy(EnemyInfo __instance)
	{
		var feature = FeatureFactory.GetFeature<Ghost>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		if (__instance.Person is not {IsYourPlayer: true}) 
			return true; // use original code

		var groupInfo = __instance.GroupInfo;
		groupInfo.Clear();
		groupInfo.IsHaveSeen = false;
		groupInfo.EnemyLastPosition = Vector3.zero;
		groupInfo.EnemyLastVisiblePosition = Vector3.zero;
		groupInfo.EnemyWeaponRootLastPos = Vector3.zero;
		groupInfo.EnemyLastSeenTimeSense = 0f;
		groupInfo.EnemyLastSeenTimeReal = 0f;

		var memory = __instance.Owner.Memory;
		memory.GoalTarget.Clear();
		memory.GoalEnemy = null;
		memory.LastEnemy = null;

		__instance.SetCanShoot(false);
		__instance.SetCanShoot(false);
		__instance.SetIgnoreState();

		return false; // skip the original code
	}

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(EnemyInfo), nameof(EnemyInfo.CheckLookEnemy), nameof(CheckLookEnemy));
		});
	}
}
