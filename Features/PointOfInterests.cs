using System;
using System.Linq;
using System.Text;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal abstract class PointOfInterests : CachableFeature<PointOfInterest[]>
	{
		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		[ConfigurationProperty]
		public float MaximumDistance { get; set; } = 0f;

		public abstract Color GroupingColor { get; }

		public override void ProcessDataOnGUI(PointOfInterest[] data)
		{
			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var poiPerPosition = data.GroupBy(poi => poi.Position);
			foreach (var positionGroup in poiPerPosition)
			{
				var position = positionGroup.Key;
				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Math.Round(Vector3.Distance(camera.transform.position, position));
				if (MaximumDistance > 0 && distance > MaximumDistance)
					continue;

				var drawPosition = screenPosition;

				var poiPerOwner = positionGroup.GroupBy(poi => poi.Owner);
				foreach (var ownerGroup in poiPerOwner)
				{
					var distinctGroup = ownerGroup
						.DistinctBy(poi => poi.Name)
						.ToList();

					var owner = ownerGroup.Key;
					var flags = GetCaptionFlags.All;

					if (owner != null && distinctGroup.Count > 1)
					{
						flags = GetCaptionFlags.Name;
						drawPosition = new Vector2(drawPosition.x, drawPosition.y + Render.DrawString(drawPosition, $">> In {owner} [{distance}m]", GroupingColor, false).y);
					}

					foreach (var poi in distinctGroup)
					{
						drawPosition = new Vector2(drawPosition.x, drawPosition.y + Render.DrawString(drawPosition, GetCaption(poi, distance, flags), poi.Color, flags == GetCaptionFlags.All).y);
					}
				}
			}
		}

		[Flags]
		public enum GetCaptionFlags
		{
			Name = 1,
			Owner = 2,
			Distance = 4,
			All = Name | Owner | Distance
		}

		public virtual string GetCaption(PointOfInterest poi, double distance, GetCaptionFlags flags = GetCaptionFlags.All)
		{
			var result = new StringBuilder();

			if ((flags & GetCaptionFlags.Name) != 0)
			{
				result.Append(poi.Name);
				result.Append(" ");
			}

			if (poi.Owner != null && (flags & GetCaptionFlags.Owner) != 0)
			{
				result.Append($"(in {poi.Owner})");
				result.Append(" ");
			}

			if ((flags & GetCaptionFlags.Distance) != 0)
			{
				result.Append($"[{distance}m]");
			}

			return result.ToString();
		}
	}
}
