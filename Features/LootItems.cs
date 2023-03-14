using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JsonType;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal class LootItems : PointOfInterests
	{
		public override string Name => "loot";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.cyan;

		[ConfigurationProperty(Browsable = false, Comment = @"Example: [""foo"", ""bar""] or with extended properties: [{""Name"":""foo"",""Color"":[1.0,0.0,0.0,1.0]},{""Name"":""bar"",""Color"":[1.0,1.0,1.0,0.8],""Rarity"":""Rare""}]")]
		public List<TrackedItem> TrackedNames { get; set; } = new();

		[ConfigurationProperty] 
		public bool SearchInsideContainers { get; set; } = true;

		[ConfigurationProperty]
		public bool SearchInsideCorpses { get; set; } = true;
		
		[ConfigurationProperty]
		public bool ShowCorpses {  get; set; } = true;

		[ConfigurationProperty]
		public bool ShowPrices { get; set; } = true;

		public override float CacheTimeInSec { get; set; } = 3f;
		public override Color GroupingColor => Color;

		public bool Track(string lootname, Color? color, ELootRarity? rarity)
		{
			lootname = lootname.Trim();

			if (TrackedNames.Any(t => t.Name == lootname && t.Rarity == rarity))
				return false;

			TrackedNames.Add(new TrackedItem(lootname, color, rarity));
			return true;

		}

		public bool UnTrack(string lootname)
		{
			lootname = lootname.Trim();

			if (lootname == "*" && TrackedNames.Count > 0)
			{
				TrackedNames.Clear();
				return true;
			}
			
			return TrackedNames.RemoveAll(t => t.Name == lootname) > 0;
		}

		public override PointOfInterest[] RefreshData()
		{
			if (TrackedNames.Count == 0)
				return Empty;

			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return Empty;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return Empty;

			var records = new List<PointOfInterest>();

			// Step 1 - look outside containers (loot items)
			FindLootItems(world, records);

			// Step 2 - look inside containers (items)
			if (SearchInsideContainers)
				FindItemsInContainers(world, records);

			return records.ToArray();
		}

		private void FindItemsInContainers(GameWorld world, List<PointOfInterest> records)
		{
			var owners = world.ItemOwners; // contains all containers: corpses, LootContainers, ...
			foreach (var owner in owners)
			{
				var rootItem = owner.Key.RootItem;
				if (rootItem is not { IsContainer: true })
					continue;

				if (rootItem.Parent.Item.IsFiltered())
					continue;

				var position = owner.Value.Transform.position;
				FindItemsInRootItem(records, rootItem, position);
			}
		}

		private void FindItemsInRootItem(List<PointOfInterest> records, Item? rootItem, Vector3 position)
		{
			var items = rootItem?
				.GetAllItems()?
				.ToArray();

			if (items == null)
				return;

			foreach (var item in items)
			{
				if (!item.IsValid())
					continue;

				if (item.IsFiltered())
					continue;

				TryAddRecordIfTracked(item, records, position, item.Owner?.RootItem?.TemplateId.LocalizedShortName());
			}
		}

		private void FindLootItems(GameWorld world, List<PointOfInterest> records)
		{
			var lootItems = world.LootItems;
			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				var position = lootItem.transform.position;

				if (lootItem is Corpse corpse)
				{
					if (ShowCorpses)
						AddCorpse(records, position);
						
					if (SearchInsideCorpses)
						FindItemsInRootItem(records, corpse.ItemOwner?.RootItem, position);

					continue;
				}

				TryAddRecordIfTracked(lootItem.Item, records, position);
			}
		}

		private string FormatName(string itemName, Item item)
		{
			var price = item.Template.CreditsPrice;
			if (!ShowPrices || price < 1000)
				return itemName;

			return $"{itemName} {price / 1000}K";
		}

		private void TryAddRecordIfTracked(Item item, List<PointOfInterest> records, Vector3 position, string? owner = null)
		{
			var itemName = item.ShortName.Localized();
			var template = item.Template;
			var trackedItem = TrackedNames.Find(t => t.Name == "*"
			                                         || itemName.IndexOf(t.Name, StringComparison.OrdinalIgnoreCase) >= 0
			                                         || string.Equals(template._id, t.Name, StringComparison.OrdinalIgnoreCase));

			var rarity = template.GetEstimatedRarity();
			if (trackedItem == null || !RarityMatches(rarity, trackedItem.Rarity))
				return;

			var color = trackedItem.Color ?? Color;
			records.Add(new PointOfInterest
			{
				Name = FormatName(itemName, item),
				Owner = string.Equals(itemName, owner, StringComparison.OrdinalIgnoreCase) ? null : owner,
				Position = position,
				Color = color
			});
		}

		private static bool RarityMatches(ELootRarity itemRarity, ELootRarity? trackedRarity)
		{
			if (!trackedRarity.HasValue)
				return true;

			return trackedRarity.Value == itemRarity;
		}

		private void AddCorpse(List<PointOfInterest> records, Vector3 position)
		{
			records.Add(new PointOfInterest
			{
				Name = "Corpse",
				Position = position,
				Color = Color
			});
		}
	}
}
