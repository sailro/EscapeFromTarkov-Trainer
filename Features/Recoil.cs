using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;

namespace EFT.Trainer.Features
{
	public class Recoil : FeatureMonoBehaviour
	{
		[ConfigurationProperty]
		public override bool Enabled { get; set; } = false;

		protected override void UpdateFeature()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			player!.ProceduralWeaponAnimation.Shootingg.Intensity = 0f;
		}
	}
}
