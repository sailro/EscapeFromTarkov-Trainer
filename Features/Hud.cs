using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Hud : EnableableMonoBehaviour
	{
		[ConfigurationProperty]
		public Color Color { get; set; } = Color.white;

		[ConfigurationProperty]
		public override bool Enabled { get; set; } = true;
		
		private void OnGUI()
		{
			if (!Enabled)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player!.HandsController == null || player.HandsController.Item is not Weapon weapon)
				return;

			var mag = weapon.GetCurrentMagazine();
			if (mag == null)
				return;

			var hud = $"{mag.Count}+{weapon.ChamberAmmoCount}/{mag.MaxCount} [{weapon.SelectedFireMode}]";
			Render.DrawString(new Vector2(512, Screen.height - 16f), hud, Color);
		}
	}
}
