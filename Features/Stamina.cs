using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Stamina : ToggleFeature
{
	public override string Name => "stamina";
	public override string Description => "Unlimited stamina.";

	public override bool Enabled { get; set; } = false;

	private float _aimDrainRate;
	private float _aimRangeFinderDrainRate;
	private float _sprintDrainRate;
	private float _jumpConsumption;
	private float _proneConsumption;

	private Vector3 _aimConsumptionByPose;
	private Vector3 _overweightConsumptionByPose;
	private Vector2 _crouchConsumption;
	private Vector2 _standupConsumption;
	private Vector2 _walkConsumption;

	private float _oxygenRestoration;
	private float _exhaustedMeleeSpeed;

	private float _baseRestorationRate;

	private bool _staminaExhaustionCausesJiggle;
	private bool _staminaExhaustionRocksCamera;
	private bool _staminaExhaustionStartsBreathSound;

	private bool _isConfigured = false;
	private bool _wasReset = true;

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

			HarmonyPrefix(harmony, playerPhysicalStamina.GetType(), nameof(playerPhysicalStamina.Consume), nameof(ConsumePrefix));
		});

		var parameters = playerPhysical.StaminaParameters;
		if (parameters == null)
			return;

		if (!_isConfigured)
		{
			_aimDrainRate = parameters.AimDrainRate;
			_aimRangeFinderDrainRate = parameters.AimRangeFinderDrainRate;
			_sprintDrainRate = parameters.SprintDrainRate;
			_jumpConsumption = parameters.JumpConsumption;
			_proneConsumption = parameters.ProneConsumption;

			_aimConsumptionByPose = parameters.AimConsumptionByPose;
			_overweightConsumptionByPose = parameters.OverweightConsumptionByPose;
				
			_crouchConsumption = parameters.CrouchConsumption;
			_standupConsumption = parameters.StandupConsumption;
			_walkConsumption = parameters.WalkConsumption;

			_oxygenRestoration = parameters.OxygenRestoration;
			_exhaustedMeleeSpeed = parameters.ExhaustedMeleeSpeed;

			_baseRestorationRate = parameters.BaseRestorationRate;

			_staminaExhaustionCausesJiggle = parameters.StaminaExhaustionCausesJiggle;
			_staminaExhaustionRocksCamera = parameters.StaminaExhaustionRocksCamera;
			_staminaExhaustionStartsBreathSound = parameters.StaminaExhaustionStartsBreathSound;

			_isConfigured = true;
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

		_wasReset = false; // Maintain variables in modified state
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

		if (_wasReset)
			return;

		parameters.AimDrainRate = _aimDrainRate;
		parameters.AimRangeFinderDrainRate = _aimRangeFinderDrainRate;
		parameters.SprintDrainRate = _sprintDrainRate;
		parameters.JumpConsumption = _jumpConsumption;
		parameters.ProneConsumption = _proneConsumption;

		parameters.AimConsumptionByPose = _aimConsumptionByPose;
		parameters.OverweightConsumptionByPose = _overweightConsumptionByPose;

		parameters.CrouchConsumption = _crouchConsumption;
		parameters.StandupConsumption = _standupConsumption;
		parameters.WalkConsumption = _walkConsumption;

		parameters.OxygenRestoration = _oxygenRestoration;
		parameters.ExhaustedMeleeSpeed = _exhaustedMeleeSpeed;

		parameters.BaseRestorationRate = _baseRestorationRate;

		parameters.StaminaExhaustionCausesJiggle = _staminaExhaustionCausesJiggle;
		parameters.StaminaExhaustionRocksCamera = _staminaExhaustionRocksCamera;
		parameters.StaminaExhaustionStartsBreathSound = _staminaExhaustionStartsBreathSound;

		_wasReset = true;
	}
}
