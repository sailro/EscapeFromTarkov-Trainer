using EFT.Trainer.Extensions;

namespace EFT.Trainer.Features
{
	public class NoRecoil : ToggleMonoBehaviour
	{
		public override bool Enabled { get; set; } = false;

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player!.ProceduralWeaponAnimation == null)
				return;

			player.ProceduralWeaponAnimation.Shootingg.Intensity = 0f;
		}
	}
}
