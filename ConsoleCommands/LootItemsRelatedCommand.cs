using System;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class LootItemsRelatedCommand : ConsoleCommandWithArgument
{
	private readonly Lazy<Features.LootItems> _lootItems = new(() => Features.FeatureFactory.GetFeature<Features.LootItems>()!);
	protected Features.LootItems LootItems => _lootItems.Value;
}
