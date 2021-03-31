using EFT.Trainer.Extensions;

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

			var parameters = player!.Physical?.StaminaParameters;
			if (parameters == null)
				return;

			parameters.AimDrainRate = 0f;
			parameters.SprintDrainRate = 0f;
			parameters.JumpConsumption = 0f;
			parameters.ExhaustedMeleeSpeed = 10000f;
		}
	}
}
