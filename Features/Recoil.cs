using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;

namespace EFT.Trainer.Features
{
	public class Recoil : EnableableMonoBehaviour
	{
		[ConfigurationProperty]
		public override bool Enabled { get; set; } = false;

		private void Update()
		{
			if (!Enabled)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			player!.ProceduralWeaponAnimation.Shootingg.Intensity = 0f;
		}
	}
}
