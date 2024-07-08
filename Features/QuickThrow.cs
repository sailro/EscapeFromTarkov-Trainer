using System.Linq;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class QuickTrow : TriggerFeature
{
	public override string Name => Strings.FeatureQuickTrowName;
	public override string Description => Strings.FeatureQuickTrowDescription;

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
			.GetPlayerItems(EPlayerItems.Equipment)
			.OfType<GrenadeClass>()
			.FirstOrDefault();

		if (grenade == null)
			return;

		player.SetInHandsForQuickUse(grenade, null);
	}
}
