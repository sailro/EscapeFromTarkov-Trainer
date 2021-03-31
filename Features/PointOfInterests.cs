using System;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class PointOfInterests : CachableMonoBehaviour<PointOfInterest[]>
	{
		public override void ProcessDataOnGUI(PointOfInterest[] data)
		{
			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			foreach (var quest in data)
			{
				var position = quest.Position;

				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Math.Round(Vector3.Distance(camera.transform.position, position));
				var caption = $"{quest.Name} [{distance}m]";
				Render.DrawString(new Vector2(screenPosition.x - 50f, screenPosition.y), caption, quest.Color);
			}
		}
	}
}
