using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class AutomaticGun : ToggleFeature
	{
		public override string Name => "autogun";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public int Rate { get; set; } = 500;

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player.HandsController.Item is not Weapon weapon)
				return;

			var fireModeComponent = weapon.GetItemComponent<FireModeComponent>();
			if (fireModeComponent == null)
				return;

			fireModeComponent.FireMode = Weapon.EFireMode.fullauto;

			if (!player.TryGetComponent<Player.FirearmController>(out var firearmController))
				return;

			var template = firearmController.Item?.Template;
			if (template == null)
				return;

			template.BoltAction = false;
			template.bFirerate = Rate;
		}
	}
}
