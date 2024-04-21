using System.Linq;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Durability : ToggleFeature
{
	public override string Name => "durability";
	public override string Description => "Maximum durability of items.";

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var allPlayerItems = player.Profile
			.Inventory
			.GetPlayerItems()
			.ToArray();

		foreach (var item in allPlayerItems)
		{
			var repairable = item?.GetItemComponent<RepairableComponent>();
			if (repairable == null)
				continue;

			repairable.MaxDurability = repairable.TemplateDurability;
			repairable.Durability = repairable.MaxDurability;
		}
	}
}
