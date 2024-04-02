using EFT.InventoryLogic;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Examine : ToggleFeature
{
	public override string Name => "examine";
	public override string Description => "All items already examined.";

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

	protected override void UpdateWhenEnabled()
	{
		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(string)]);
			HarmonyPrefix(harmony, typeof(Profile), nameof(Profile.Examined), nameof(ExaminedPrefix), [typeof(Item)]);
		});
	}
}
