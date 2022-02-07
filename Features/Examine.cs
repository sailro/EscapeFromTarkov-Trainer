using EFT.InventoryLogic;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Examine : ToggleFeature
	{
		public override string Name => "examine";

		public override bool Enabled { get; set; } = false;

#if HARMONY
		[UsedImplicitly]
		protected static bool ExaminedPrefix(ref bool __result)
		{
			var feature = FeatureFactory.GetFeature<Examine>();
			if (feature == null || !feature.Enabled)
				return true; // keep using original code, we are not enabled

			__result = true;
			return false;  // skip the original code and all other prefix methods 
		}
#endif

		protected override void UpdateWhenEnabled()
		{
#if HARMONY
			HarmonyPatchOnce(harmony =>
			{
				var originalString = HarmonyLib.AccessTools.Method(typeof(Profile), nameof(Profile.Examined), new[] {typeof(string)});
				if (originalString == null)
					return;

				var originalItem = HarmonyLib.AccessTools.Method(typeof(Profile), nameof(Profile.Examined), new[] {typeof(Item)});
				if (originalItem == null)
					return;

				var prefix = HarmonyLib.AccessTools.Method(GetType(), nameof(ExaminedPrefix));
				if (prefix == null)
					return;

				harmony.Patch(originalString, new HarmonyLib.HarmonyMethod(prefix));
				harmony.Patch(originalItem, new HarmonyLib.HarmonyMethod(prefix));
			});
#endif
		}
	}
}
