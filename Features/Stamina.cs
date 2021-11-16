using EFT.Trainer.Extensions;

#nullable enable

namespace EFT.Trainer.Features
{
	public class Stamina : ToggleMonoBehaviour
	{
		public override bool Enabled { get; set; } = false;

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var parameters = player.Physical?.StaminaParameters;
			if (parameters == null)
				return;

			parameters.AimDrainRate = 0f;
			parameters.SprintDrainRate = 0f;
			parameters.JumpConsumption = 0f;
			parameters.ProneConsumption = 0f;
			parameters.SitToStandConsumption = 0f;

			parameters.OxygenRestoration = 10000f;
			parameters.ExhaustedMeleeSpeed = 10000f;

			parameters.BaseRestorationRate = parameters.Capacity;
		}
	}
}
