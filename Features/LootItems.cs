using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class LootItems : PointOfInterests
	{
		public static readonly Color LootItemColor = Color.cyan;

		public override float CacheTimeInSec => 3f;
		public override bool Enabled { get; set; } = true;

		private static readonly List<string> _names = new();

		public static void Track(string name)
		{
			if (!_names.Contains(name))
				_names.Add(name);

			DumpList();
		}

		public static void UnTrack(string name)
		{
			if (name == "*")
				_names.Clear();
			else
				_names.Remove(name);

			DumpList();
		}

		private static void DumpList()
		{
			AddConsoleLog("Tracking list updated...", "tracker");
			foreach (var item in _names)
				AddConsoleLog($"Tracking: {item}", "tracker");
		}

		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		public override PointOfInterest[] RefreshData()
		{
			if (_names.Count == 0)
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

			var lootItems = world.LootItems;
			var records = new List<PointOfInterest>();
			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				var position = lootItem.transform.position;
				var lootItemName = lootItem.Item.ShortName.Localized();

				if (_names.Any(match => lootItemName.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0))
				{
					records.Add(new PointOfInterest
					{
						Name = lootItemName,
						Position = position,
						ScreenPosition = camera.WorldPointToScreenPoint(position),
						Color = LootItemColor
					});
				}
			}

			return records.ToArray();
		}
	}
}
