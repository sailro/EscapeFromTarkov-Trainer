using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class NoRecoil : ToggleFeature
	{
		public override string Name => "norecoil";

		public override bool Enabled { get; set; } = false;

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player.ProceduralWeaponAnimation == null)
				return;

			player.ProceduralWeaponAnimation.Shootingg.Intensity = 0f;
		}
	}
}
