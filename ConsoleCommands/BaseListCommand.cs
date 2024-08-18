using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Comfort.Common;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class BaseListCommand : ConsoleCommandWithArgument
{
	public override string Pattern => OptionalArgumentPattern;
	protected virtual ELootRarity? Rarity => null;

	public override void Execute(Match match)
	{
		ListLootItems(match, Rarity);
	}

	private void ListLootItems(Match match, ELootRarity? rarityFilter = null)
	{
		var search = string.Empty;
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is { Success: true })
		{
			search = matchGroup.Value.Trim();
			if (search == TrackedItem.MatchAll)
				search = string.Empty;
		}

		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var itemsPerName = new Dictionary<string, List<Item>>();

		// Step 1 - look outside containers and inside corpses (loot items)
		FindLootItems(world, itemsPerName);

		// Step 2 - look inside containers (items)
		if (LootItemsFeature.SearchInsideContainers)
			FindItemsInContainers(world, itemsPerName);

		var names = itemsPerName.Keys.ToList();
		names.Sort();
		names.Reverse();

		var count = 0;
		foreach (var itemName in names)
		{
			if (itemName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
				continue;

			var list = itemsPerName[itemName];
			var rarity = list.First().Template.GetEstimatedRarity();
			if (rarityFilter.HasValue && rarityFilter.Value != rarity)
				continue;

			var extra = rarity != ELootRarity.Not_exist ? string.Format(Strings.CommandListRarityFormat, rarity.Color()) : string.Empty;
			AddConsoleLog(string.Format(Strings.CommandListEnumerateFormat, itemName, list.Count.ToString().Cyan(), extra));

			count += list.Count;
		}

		AddConsoleLog(Strings.TextSeparator);
		AddConsoleLog(string.Format(Strings.CommandListSuccessFormat, count.ToString().Cyan()));
	}

	private static void FindItemsInRootItem(Dictionary<string, List<Item>> itemsPerName, Item? rootItem)
	{
		var items = rootItem?
			.GetAllItems()?
			.ToArray();

		if (items == null)
			return;

		IndexItems(items, itemsPerName);
	}

	private void FindLootItems(GameWorld world, Dictionary<string, List<Item>> itemsPerName)
	{
		var lootItems = world.LootItems;
		var filteredItems = new List<Item>();
		for (var i = 0; i < lootItems.Count; i++)
		{
			var lootItem = lootItems.GetByIndex(i);
			if (!lootItem.IsValid())
				continue;

			if (lootItem is Corpse corpse)
			{
				if (LootItemsFeature.SearchInsideCorpses)
					FindItemsInRootItem(itemsPerName, corpse.ItemOwner?.RootItem);

				continue;
			}

			filteredItems.Add(lootItem.Item);
		}

		IndexItems(filteredItems, itemsPerName);
	}

	private static void IndexItems(IEnumerable<Item> items, Dictionary<string, List<Item>> itemsPerName)
	{
		foreach (var item in items)
		{
			if (!item.IsValid() || item.IsFiltered())
				continue;

			var itemName = item.ShortName.Localized();
			if (!itemsPerName.TryGetValue(itemName, out var pnList))
			{
				pnList = [];
				itemsPerName[itemName] = pnList;
			}

			pnList.Add(item);
		}
	}

	private static void FindItemsInContainers(GameWorld world, Dictionary<string, List<Item>> itemsPerName)
	{
		var owners = world.ItemOwners; // contains all containers: corpses, LootContainers, ...
		foreach (var owner in owners)
		{
			var rootItem = owner.Key.RootItem;
			if (rootItem is not { IsContainer: true })
				continue;

			if (!rootItem.IsValid() || rootItem.IsFiltered()) // filter default inventory container here, given we special case the corpse container
				continue;

			FindItemsInRootItem(itemsPerName, rootItem);
		}
	}
}
