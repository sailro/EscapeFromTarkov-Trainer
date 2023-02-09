using Comfort.Common;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using System;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class UnlimitedAmmo : ToggleFeature
	{
		public override string Name => "unlammo";

		public override bool Enabled { get; set; } = false;


		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player.HandsController.Item is not Weapon weapon)
				return;

			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return;

			var sbc = world.SharedBallisticsCalculator;
			if (sbc == null)
				return;

			for (int shotIndex = 0; shotIndex < sbc.ActiveShotsCount; shotIndex++)
			{
				var shot = sbc.GetActiveShot(shotIndex);
				if (shot == null)
					continue;

				if (shot.IsShotFinished)
					continue;

				// Make sur we are not enhancing ennemy shots
				if (shot.Player.IsValid() && !shot.Player.IsYourPlayer)
					continue;

				MagazineClass currentMagazine = weapon.GetCurrentMagazine();

				if (currentMagazine != null)
				{
					var instantiated = Singleton<ItemFactory>.Instantiated;
					if (instantiated)
					{
						ItemFactory instance = Singleton<ItemFactory>.Instance;
						var itemId = Guid.NewGuid().ToString("N").Substring(0, 24);
						StackSlot cartridges = currentMagazine.Cartridges;
						cartridges?.Add(instance.CreateItem(itemId, shot.Ammo.TemplateId, null) ?? shot.Ammo, false);
					}
				}
			}
		}
	}
}
