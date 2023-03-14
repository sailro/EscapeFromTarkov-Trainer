using System.Collections.Generic;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class LootableContainers : PointOfInterests
	{
		public override string Name => "stash";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.white;

		public override float CacheTimeInSec { get; set; } = 11f;
		public override bool Enabled { get; set; } = false;
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

			var owners = world.ItemOwners; 
			var records = new List<PointOfInterest>();

			foreach (var owner in owners)
			{
				var itemOwner = owner.Key;
				var rootItem = itemOwner.RootItem;
				if (rootItem is not { IsContainer: true })
					continue;

				if (rootItem.TemplateId != KnownTemplateIds.BuriedBarrelCache && rootItem.TemplateId != KnownTemplateIds.GroundCache)
					continue;

				var position = owner.Value.Transform.position;
				records.Add(new PointOfInterest
				{
					Name = rootItem.TemplateId.LocalizedShortName(), // nicer than ItemOwner.ContainerName which is full caps
					Position = position,
					Color = Color
				});
			}

			return records.ToArray();
		}
	}
}
