using System.Linq;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Durability : ToggleFeature
{
	public override string Name => Strings.FeatureDurabilityName;
	public override string Description => Strings.FeatureDurabilityDescription;

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
