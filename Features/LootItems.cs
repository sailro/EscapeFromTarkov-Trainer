using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class LootItems : PointOfInterests
	{
		[ConfigurationProperty]
		public Color Color { get; set; } = Color.cyan;

		public override float CacheTimeInSec { get; set; } = 3f;

		[ConfigurationProperty]
		public List<string> TrackedNames { get; set; } = new();

		[ConfigurationProperty] 
		public bool SearchInsideContainers { get; set; } = true;

		public void Track(string lootname)
		{
			if (!TrackedNames.Contains(lootname))
				TrackedNames.Add(lootname);

			DumpList();
		}

		public void UnTrack(string lootname)
		{
			if (lootname == "*")
				TrackedNames.Clear();
			else
				TrackedNames.Remove(lootname);

			DumpList();
		}

		private void DumpList()
		{
			AddConsoleLog("Tracking list updated...", "tracker");
			foreach (var item in TrackedNames)
				AddConsoleLog($"Tracking: {item}", "tracker");
		}

		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

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

			var camera = Camera.main;
			if (camera == null)
				return Empty;

			var records = new List<PointOfInterest>();

			// Step 1 - look outside containers (loot items)
			FindLootItems(world, records, camera);

			// Step 2 - look inside containers (items)
			if (SearchInsideContainers)
				FindItemsInContainers(records, camera);

			return records.ToArray();
		}

		private void FindItemsInContainers(List<PointOfInterest> records, Camera camera)
		{
			var containers = FindObjectsOfType<LootableContainer>();
			foreach (var container in containers)
			{
				if (!container.IsValid())
					continue;

				var items = container
					.ItemOwner?
					.RootItem?
					.GetAllItems()?
					.ToArray();

				if (items == null)
					continue;

				var position = container.transform.position;
				var containerName = container.Template.LocalizedShortName();
				foreach (var item in items)
				{
					if (!item.IsValid())
						continue;

					var itemName = item.ShortName.Localized();
					if (TrackedNames.Any(match => itemName.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0))
					{
						records.Add(new PointOfInterest
						{
							Name = $"{itemName} (in {containerName})",
							Position = position,
							ScreenPosition = camera.WorldPointToScreenPoint(position),
							Color = Color
						});
					}
				}
			}
		}

		private void FindLootItems(GameWorld world, List<PointOfInterest> records, Camera camera)
		{
			var lootItems = world.LootItems;
			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				var position = lootItem.transform.position;
				var lootItemName = lootItem.Item.ShortName.Localized();

				if (TrackedNames.Any(match => lootItemName.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0))
				{
					records.Add(new PointOfInterest
					{
						Name = lootItemName,
						Position = position,
						ScreenPosition = camera.WorldPointToScreenPoint(position),
						Color = Color
					});
				}
			}
		}
	}
}
