using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class LootableContainers : PointOfInterests
	{
		public static readonly Color LootableContainerColor = Color.white;

		public override float CacheTimeInSec => 11f;
		public override bool Enabled { get; set; } = false;

		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		public override PointOfInterest[] RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return Empty;

			var camera = Camera.main;
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
					Color = LootableContainerColor
				});
			}

			return records.ToArray();
		}
	}
}
