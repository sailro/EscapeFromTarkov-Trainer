using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal abstract class PointOfInterests : CachableFeature<PointOfInterest>
{

	[ConfigurationProperty]
	public float MaximumDistance { get; set; } = 0f;

	public abstract Color GroupingColor { get; }

	public override void ProcessDataOnGUI(IReadOnlyList<PointOfInterest> data)
	{
		var snapshot = GameState.Current;
		if (snapshot == null)
			return;

		var camera = snapshot.MapMode ? snapshot.MapCamera : snapshot.Camera;
		if (camera == null)
			return;

		var cameraPosition = camera.transform.position;
		var poiPerPosition = data.ToLookup(poi => poi.Position);
		foreach (var positionGroup in poiPerPosition)
		{
			var position = positionGroup.Key;
			var screenPosition = camera.WorldPointToVisibleScreenPoint(position);
			if (screenPosition == Vector2.zero)
				continue;

			if (snapshot.MapMode)
				cameraPosition.y = position.y;

			var distance = Mathf.Round(Vector3.Distance(cameraPosition, position));
			if (MaximumDistance > 0 && distance > MaximumDistance)
				continue;

			var drawPosition = screenPosition;

			var poiPerOwner = positionGroup.ToLookup(poi => poi.Owner);
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
