using System;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Health : ToggleFeature
	{
		public override string Name => "health";

		public override bool Enabled { get; set; } = false;

		private static readonly Array _bodyParts = Enum.GetValues(typeof(EBodyPart));

#if HARMONY
		[UsedImplicitly]
		protected static bool ApplyDamagePrefix(object __instance, ref float __result)
		{
			var feature = FeatureFactory.GetFeature<Health>();
			if (feature == null || !feature.Enabled)
				return true; // keep using original code, we are not enabled

			if (__instance is IHealthController {Player: {IsYourPlayer: false}})
				return true; // keep using original code, apply damage to others

			__result = 0f;
			return false;  // skip the original code and all other prefix methods 
		}
#endif

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var healthController = player.ActiveHealthController;
			if (healthController == null)
				return;

#if HARMONY
			HarmonyPatchOnce(harmony =>
			{
				var original = HarmonyLib.AccessTools.Method(healthController.GetType(), nameof(healthController.ApplyDamage));
				if (original == null)
					return;

				var prefix = HarmonyLib.AccessTools.Method(GetType(), nameof(ApplyDamagePrefix));
				if (prefix == null)
					return;

				harmony.Patch(original, new HarmonyLib.HarmonyMethod(prefix));
			});
#endif

			foreach (EBodyPart bodyPart in _bodyParts)
			{
				if (bodyPart == EBodyPart.Common)
					continue;

				if (healthController.IsBodyPartBroken(bodyPart) || healthController.IsBodyPartDestroyed(bodyPart))
					healthController.RestoreBodyPart(bodyPart, 0);

				var bodyPartHealth = healthController.GetBodyPartHealth(bodyPart);
				if (bodyPartHealth.AtMaximum)
					continue;

				healthController.RestoreFullHealth();
				healthController.RemoveNegativeEffects(EBodyPart.Common);
				break;
			}

			if (!healthController.Energy.AtMaximum)
				healthController.ChangeEnergy(healthController.Energy.Maximum);

			if (!healthController.Hydration.AtMaximum)
				healthController.ChangeHydration(healthController.Hydration.Maximum);
		}
	}
}
