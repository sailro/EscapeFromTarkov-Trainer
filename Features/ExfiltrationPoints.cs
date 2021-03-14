using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class ExfiltrationPoints : CachableMonoBehaviour<IEnumerable<ExfiltrationPointRecord>>
	{
		public static readonly Color EligibleExfiltrationPointColor = Color.green;
		public static readonly Color ExfiltrationPointColor = Color.yellow;

		public override float CacheTimeInSec => 10f;
		public override bool Enabled { get; set; } = true;

		public override IEnumerable<ExfiltrationPointRecord> RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				yield break;

			if (world.ExfiltrationController == null)
				yield break;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				yield break;

			var profile = player!.Profile;
			var info = profile?.Info;
			if (info == null)
				yield break;

			var side = info.Side;
			var points = GetExfiltrationPoints(side, world);
			if (points == null)
				yield break;

			var eligiblePoints = GetEligibleExfiltrationPoints(side, world, profile);
			var camera = Camera.main;
			foreach (var point in points)
			{
				if (!point.IsValid()) 
					continue;

				var position = point.transform.position;
				yield return new ExfiltrationPointRecord
				{
					Name = point.Settings.Name.Localized(),
					Position = position,
					ScreenPosition = camera.WorldPointToScreenPoint(position),
					Color = eligiblePoints.Contains(point) ? EligibleExfiltrationPointColor : ExfiltrationPointColor
				};
			}
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

		public override void ProcessDataOnGUI(IEnumerable<ExfiltrationPointRecord> data)
		{
			var camera = Camera.main;

			foreach (var point in data)
			{
				var position = point.Position;

				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Math.Round(Vector3.Distance(camera.transform.position, position));
				var caption = $"{point.Name} [{distance}m]";
				Render.DrawString(new Vector2(screenPosition.x - 50f, screenPosition.y), caption, point.Color);
			}
		}
	}

	public struct ExfiltrationPointRecord
	{
		public string Name { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 ScreenPosition { get; set; }
		public Color Color { get; set; }
	}
}
