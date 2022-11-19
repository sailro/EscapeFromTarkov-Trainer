using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Stamina : ToggleFeature
	{
		public override string Name => "stamina";

		public override bool Enabled { get; set; } = false;

		[UsedImplicitly]
		protected static bool ConsumePrefix()
		{
			var feature = FeatureFactory.GetFeature<Stamina>();
			if (feature == null || !feature.Enabled)
				return true; // keep using original code, we are not enabled

			return false;  // skip the original code and all other prefix methods 
		}

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var playerPhysical = player.Physical;
			if (playerPhysical == null)
				return;

			HarmonyPatchOnce(harmony =>
			{
				var playerPhysicalStamina = playerPhysical.Stamina;
				if (playerPhysicalStamina == null)
					return;

				var original = HarmonyLib.AccessTools.Method(playerPhysicalStamina.GetType(), nameof(playerPhysicalStamina.Consume));
				if (original == null)
					return;

				var prefix = HarmonyLib.AccessTools.Method(GetType(), nameof(ConsumePrefix));
				if (prefix == null)
					return;

				harmony.Patch(original, new HarmonyLib.HarmonyMethod(prefix));
			});

			var parameters = playerPhysical.StaminaParameters;
			if (parameters == null)
				return;

			parameters.AimDrainRate = 0f;
			parameters.SprintDrainRate = 0f;
			parameters.JumpConsumption = 0f;
			parameters.ProneConsumption = 0f;

			parameters.AimConsumptionByPose = Vector3.zero;
			parameters.OverweightConsumptionByPose = Vector3.zero;

			parameters.CrouchConsumption = Vector2.zero;
			parameters.StandupConsumption = Vector2.zero;
			parameters.WalkConsumption = Vector2.zero;

			parameters.OxygenRestoration = 10000f;
			parameters.ExhaustedMeleeSpeed = 10000f;

			parameters.BaseRestorationRate = parameters.Capacity;

			parameters.StaminaExhaustionCausesJiggle = false;
			parameters.StaminaExhaustionRocksCamera = false;
			parameters.StaminaExhaustionStartsBreathSound = false;
		}
	}
}
