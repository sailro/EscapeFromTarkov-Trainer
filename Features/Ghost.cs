using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Ghost : ToggleFeature
{
	public override string Name => "ghost";
	public override string Description => "Stop bots from seeing you.";

	public override bool Enabled { get; set; } = false;

	[UsedImplicitly]
	protected static bool SetVisiblePrefix(ref bool value, EnemyInfo __instance)
	{
		var feature = FeatureFactory.GetFeature<Ghost>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		if (__instance.Person.IsYourPlayer)
		{
			value = false;
			var groupInfo = __instance.GroupInfo;
			groupInfo.Clear();
			__instance.SetCanShoot(false);
			__instance.SetIgnoreState();
		}

		return true; // use original code with modified parameters
	}

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(EnemyInfo), nameof(EnemyInfo.SetVisible), nameof(SetVisiblePrefix));
		});
	}
}
