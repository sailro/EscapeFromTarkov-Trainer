using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;

namespace EFT.Trainer.Features
{
	public class AutomaticGun : FeatureMonoBehaviour
	{
		[ConfigurationProperty]
		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public int Rate { get; set; } = 500;

		protected override void UpdateFeature()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player!.HandsController.Item is not Weapon weapon)
				return;

			var fireModeComponent = weapon.GetItemComponent<FireModeComponent>();
			if (fireModeComponent == null)
				return;

			fireModeComponent.FireMode = Weapon.EFireMode.fullauto;

			var firearmController = player.GetComponent<Player.FirearmController>();
			if (firearmController == null)
				return;

			var template = firearmController.Item?.Template;
			if (template == null)
				return;

			template.BoltAction = false;
			template.bFirerate = Rate;
		}
	}
}
