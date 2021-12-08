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

			var containers = FindObjectsOfType<LootableContainer>();
			var records = new List<PointOfInterest>();

			foreach (var container in containers)
			{
				if (!container.IsValid())
					continue;

				if (container.Template != KnownTemplateIds.BuriedBarrelCache && container.Template != KnownTemplateIds.GroundCache)
					continue;

				var position = container.transform.position;
				records.Add(new PointOfInterest
				{
					Name = container.Template.LocalizedShortName(),
					Position = position,
					ScreenPosition = camera.WorldPointToScreenPoint(position),
					Color = Color
				});
			}

			return records.ToArray();
		}
	}
}
