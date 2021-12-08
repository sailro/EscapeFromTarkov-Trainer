using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class CrossHair : ToggleFeature
	{
		public override string Name => "crosshair";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.red;

		[ConfigurationProperty]
		public bool HideWhenAiming { get; set; } = true;

		[ConfigurationProperty]
		public float Size { get; set; } = 10f;

		[ConfigurationProperty]
		public float Thickness { get; set; } = 2f;

		protected override void OnGUIWhenEnabled()
		{
			// do not show when the console is enabled or in the hideout
			if (Cursor.visible)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player.HandsController == null)
				return;

			if (player.HandsController.IsAiming && HideWhenAiming)
				return;

			var centerx = Screen.width / 2;
			var centery = Screen.height / 2;

			Render.DrawCrosshair(new Vector2(centerx, centery), Size, Color, Thickness);
		}
	}
}
