using System.Collections.Generic;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	internal class Corpse : PointOfInterests
	{
		public override string Name => "corpses";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.white;

		public override bool Enabled { get; set; } = false;
		public override float CacheTimeInSec { get; set; } = 3f;
		public override Color GroupingColor => Color;


		public override PointOfInterest[] RefreshData()
		{

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

			// Find the corpses
			FindCorpses(world, records, camera);

			return records.ToArray();
		}

		private void FindCorpses(GameWorld world, List<PointOfInterest> records, Camera camera)
		{
			var lootItems = world.LootItems;
			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				var position = lootItem.transform.position;
				if (lootItem is Interactive.Corpse)
				{
					AddCorpse(lootItem.Item, records, camera, position);
					continue;
				}
			}
		}

		private void AddCorpse(Item item, List<PointOfInterest> records, Camera camera, Vector3 position, string? owner = null)
		{
			records.Add(new PointOfInterest
			{
				Name = "Corpse",
				Position = position,
				ScreenPosition = camera.WorldPointToScreenPoint(position),
				Color = Color
			});
		}
	}
}
