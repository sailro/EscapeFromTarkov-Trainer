using System;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public abstract class PointOfInterests : CachableMonoBehaviour<PointOfInterest[]>
	{
		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		[ConfigurationProperty]
		public float MaximumDistance { get; set; } = 0f;

		public override void ProcessDataOnGUI(PointOfInterest[] data)
		{
			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			foreach (var poi in data)
			{
				var position = poi.Position;

				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Math.Round(Vector3.Distance(camera.transform.position, position));

				if (MaximumDistance > 0 && distance > MaximumDistance)
					continue;

				Render.DrawString(new Vector2(screenPosition.x - 50f, screenPosition.y), GetCaption(poi, distance), poi.Color);
			}
		}

		public virtual string GetCaption(PointOfInterest poi, double distance)
		{
			return $"{poi.Name} [{distance}m]";
		}
	}
}
