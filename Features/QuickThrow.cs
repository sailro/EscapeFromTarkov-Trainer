using System.Linq;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class QuickTrow : TriggerFeature
	{
		public override string Name => "quickthrow";

		public override KeyCode Key { get; set; } = KeyCode.None;

		protected override void UpdateOnceWhenTriggered()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var inventory = player
				.Profile
				.Inventory;

			var grenade = inventory
				.GetAllEquipmentItems()
				.OfType<GrenadeClass>()
				.FirstOrDefault();

			if (grenade == null)
				return;

			player.SetInHandsForQuickUse(grenade, null);
		}
	}
}
