using EFT.InventoryLogic;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Examine : ToggleFeature
{
	public override string Name => Strings.FeatureExamineName;
	public override string Description => Strings.FeatureExamineDescription;

	public override bool Enabled { get; set; } = false;

	[UsedImplicitly]
	protected static bool ExaminedPrefix(ref bool __result)
	{
		var feature = FeatureFactory.GetFeature<Examine>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		__result = true;
		return false;  // skip the original code and all other prefix methods 
	}

	[UsedImplicitly]
	protected static bool GetSearchStatePrefix(SearchableItemClass __instance)
	{
		var feature = FeatureFactory.GetFeature<Examine>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return true;

		//__instance.UncoverAll(player.ProfileId);
		return true;
	}


	protected override void UpdateWhenEnabled()
	{
		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(string)]);
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(Item)]);
			//HarmonyPrefix(harmony, typeof(SearchableItemClass), nameof(SearchableItemClass.GetSearchState), nameof(GetSearchStatePrefix));
		});
	}
}
