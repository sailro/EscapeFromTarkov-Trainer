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
	public class ExfiltrationPoints : PointOfInterests
	{
		[ConfigurationProperty]
		public Color EligibleColor { get; set; } = Color.green;
		
		[ConfigurationProperty]
		public Color NotEligibleColor { get; set; } = Color.yellow;

		public override float CacheTimeInSec { get; set; } = 7f;

		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		public override PointOfInterest[] RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			if (world.ExfiltrationController == null)
				return Empty;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return Empty;

			var profile = player!.Profile;
			var info = profile?.Info;
			if (info == null)
				return Empty;

			var side = info.Side;
			var points = GetExfiltrationPoints(side, world);
			if (points == null)
				return Empty;

			var camera = Camera.main;
			if (camera == null)
				return Empty;

			var eligiblePoints = GetEligibleExfiltrationPoints(side, world, profile);
			var records = new List<PointOfInterest>();
			foreach (var point in points)
			{
				if (!point.IsValid()) 
					continue;

				var position = point.transform.position;
				records.Add(new PointOfInterest
				{
					Name = point.Settings.Name.Localized(),
					Position = position,
					ScreenPosition = camera.WorldPointToScreenPoint(position),
					Color = eligiblePoints.Contains(point) ? EligibleColor : NotEligibleColor
				});
			}

			return records.ToArray();
		}

		private static ExfiltrationPoint[] GetExfiltrationPoints(EPlayerSide side, GameWorld world)
		{
			var ect = world.ExfiltrationController;
			// ReSharper disable once CoVariantArrayConversion
			return side == EPlayerSide.Savage ? ect.ScavExfiltrationPoints : ect.ExfiltrationPoints;
		}

		private static ExfiltrationPoint[] GetEligibleExfiltrationPoints(EPlayerSide side, GameWorld world, Profile profile)
		{
			var ect = world.ExfiltrationController;
			if (side != EPlayerSide.Savage)
				return ect.EligiblePoints(profile);

			var mask = ect.GetScavExfiltrationMask(profile.Id);
			var result = new List<ExfiltrationPoint>();
			var points = ect.ScavExfiltrationPoints;

			for (int i = 0; i < 31; i++)
			{
				if ((mask & (1 << i)) != 0)
					result.Add(points[i]);
			}

			return result.ToArray();
		}
	}
}
