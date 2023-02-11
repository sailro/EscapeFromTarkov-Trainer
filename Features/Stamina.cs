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

		public float AimDrainRate;
		public float AimRangeFinderDrainRate;
		public float SprintDrainRate;
		public float JumpConsumption;
		public float ProneConsumption;

		public Vector3 AimConsumptionByPose;
		public Vector3 OverweightConsumptionByPose;
		public Vector2 CrouchConsumption;
		public Vector2 StandupConsumption;
		public Vector2 WalkConsumption;

		public float OxygenRestoration;
		public float ExhaustedMeleeSpeed;

		public float BaseRestorationRate;

		public bool StaminaExhaustionCausesJiggle;
		public bool StaminaExhaustionRocksCamera;
		public bool StaminaExhaustionStartsBreathSound;

		public bool isConfigured = false;
		public bool wasReset = true;

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

			if (!isConfigured)
			{
				AimDrainRate = parameters.AimDrainRate;
				AimRangeFinderDrainRate = parameters.AimRangeFinderDrainRate;
				SprintDrainRate = parameters.SprintDrainRate;
				JumpConsumption = parameters.JumpConsumption;
				ProneConsumption = parameters.ProneConsumption;

				AimConsumptionByPose = parameters.AimConsumptionByPose;
				OverweightConsumptionByPose = parameters.OverweightConsumptionByPose;
				
				CrouchConsumption = parameters.CrouchConsumption;
				StandupConsumption = parameters.StandupConsumption;
				WalkConsumption = parameters.WalkConsumption;

				OxygenRestoration = parameters.OxygenRestoration;
				ExhaustedMeleeSpeed = parameters.ExhaustedMeleeSpeed;

				BaseRestorationRate = parameters.BaseRestorationRate;

				StaminaExhaustionCausesJiggle = parameters.StaminaExhaustionCausesJiggle;
				StaminaExhaustionRocksCamera = parameters.StaminaExhaustionRocksCamera;
				StaminaExhaustionStartsBreathSound = parameters.StaminaExhaustionStartsBreathSound;

				isConfigured = true;
			}
				
			parameters.AimDrainRate = 0f;
			parameters.AimRangeFinderDrainRate = 0f;
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

			wasReset = false; // Maintain variables in modified state
		}

		protected override void UpdateWhenDisabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var playerPhysical = player.Physical;
			if (playerPhysical == null)
				return;

			var parameters = playerPhysical.StaminaParameters;
			if (parameters == null)
				return;

			if (wasReset)
				return;

			parameters.AimDrainRate = AimDrainRate;
			parameters.AimRangeFinderDrainRate = AimRangeFinderDrainRate;
			parameters.SprintDrainRate = SprintDrainRate;
			parameters.JumpConsumption = JumpConsumption;
			parameters.ProneConsumption = ProneConsumption;

			parameters.AimConsumptionByPose = AimConsumptionByPose;
			parameters.OverweightConsumptionByPose = OverweightConsumptionByPose;

			parameters.CrouchConsumption = CrouchConsumption;
			parameters.StandupConsumption = StandupConsumption;
			parameters.WalkConsumption = WalkConsumption;

			parameters.OxygenRestoration = OxygenRestoration;
			parameters.ExhaustedMeleeSpeed = ExhaustedMeleeSpeed;

			parameters.BaseRestorationRate = BaseRestorationRate;

			parameters.StaminaExhaustionCausesJiggle = StaminaExhaustionCausesJiggle;
			parameters.StaminaExhaustionRocksCamera = StaminaExhaustionRocksCamera;
			parameters.StaminaExhaustionStartsBreathSound = StaminaExhaustionStartsBreathSound;

			wasReset = true;
		}
	}
}
