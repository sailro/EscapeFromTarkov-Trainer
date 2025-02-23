using EFT.InventoryLogic;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using static EFT.Player;

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
		return false; // skip the original code and all other prefix methods 
	}

#pragma warning disable IDE0060
	[UsedImplicitly]
	protected static bool SinglePlayerInventoryControllerConstructorPrefix(Player player, Profile profile, bool isBot, ref bool examined)
	{
		var feature = FeatureFactory.GetFeature<Examine>();
		if (feature == null || !feature.Enabled)
			return true; // keep using original code, we are not enabled

		// this will make the game use the passthrough type implementing IPlayerSearchController, ISearchController with all items known and searched
		examined = true;
		return true;
	}
#pragma warning restore IDE0060

	protected override void UpdateWhenEnabled()
	{
		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(MongoID)]);
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(Item)]);
			HarmonyConstructorPrefix(harmony, typeof(SinglePlayerInventoryController), nameof(SinglePlayerInventoryControllerConstructorPrefix), [typeof(Player), typeof(Profile), typeof(bool), typeof(bool)]);
		});
	}
}
