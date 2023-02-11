using System;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Model;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Health : ToggleFeature
	{
		public override string Name => "health";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public bool VitalsOnly { get; set; } = false;

		[ConfigurationProperty]
		public bool RemoveNegativeEffects { get; set; } = true;

		[ConfigurationProperty]
		public bool FoodWater { get; set; } = true;

		private static readonly Array _bodyParts = Enum.GetValues(typeof(EBodyPart));
		
		[UsedImplicitly]
		protected static bool ApplyDamagePrefix(EBodyPart bodyPart, object? __instance, ref float __result)
		{
			var feature = FeatureFactory.GetFeature<Health>();
			if (feature == null || !feature.Enabled || __instance == null)
				return true; // keep using original code, we are not enabled

			var healthControllerWrapper = new HealthControllerWrapper(__instance);
			var player = healthControllerWrapper.Player;
			
			if (player == null || !player.IsYourPlayer)
				return true; // keep using original code, apply damage to others
			
			if (feature.VitalsOnly)
			{
				if (bodyPart != EBodyPart.Chest && bodyPart != EBodyPart.Head)
					return true; // keep using original code, apply damage to extremities 
			}

			__result = 0f;
			return false;  // skip the original code and all other prefix methods 
		}

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var healthController = player.ActiveHealthController;
			if (healthController == null)
				return;

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

			foreach (EBodyPart bodyPart in _bodyParts)
			{
				if (bodyPart == EBodyPart.Common)
					continue;

				if (VitalsOnly && bodyPart is not (EBodyPart.Chest or EBodyPart.Head))
					continue;

				if (healthController.IsBodyPartBroken(bodyPart) || healthController.IsBodyPartDestroyed(bodyPart))
					healthController.RestoreBodyPart(bodyPart, 1);

				if (RemoveNegativeEffects)
				{
					healthController.RemoveNegativeEffects(bodyPart);
					healthController.RemoveNegativeEffects(EBodyPart.Common);
				}

				var bodyPartHealth = healthController.GetBodyPartHealth(bodyPart);
				if (bodyPartHealth.AtMaximum)
					continue;

				if (!VitalsOnly)
					healthController.RestoreFullHealth();

				healthController.ChangeHealth(bodyPart, bodyPartHealth.Maximum, default);

				break;
			}

			if (!healthController.Energy.AtMaximum && FoodWater)
				healthController.ChangeEnergy(healthController.Energy.Maximum);

			if (!healthController.Hydration.AtMaximum && FoodWater)
				healthController.ChangeHydration(healthController.Hydration.Maximum);
		}
	}
}
