﻿using EFT.InventoryLogic;
using JsonType;

#nullable enable

namespace EFT.Trainer.Extensions;

public static class ItemTemplateExtensions
{
	public static ELootRarity GetEstimatedRarity(this ItemTemplate template)
	{
		return template.LootExperience switch
		{
			<= 0 => ELootRarity.Not_exist,
			<= 20 => ELootRarity.Common,
			<= 40 => ELootRarity.Rare,
			> 40 => ELootRarity.Superrare,
		};
	}
}
