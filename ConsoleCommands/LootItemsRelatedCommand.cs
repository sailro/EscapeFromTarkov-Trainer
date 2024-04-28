using System;
using EFT.Trainer.Features;
using LootItems = EFT.Trainer.Features.LootItems;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class LootItemsRelatedCommand : ConsoleCommandWithArgument
{
	private readonly Lazy<LootItems> _lootItems = new(() => FeatureFactory.GetFeature<LootItems>()!);
	protected LootItems LootItems => _lootItems.Value;
}
