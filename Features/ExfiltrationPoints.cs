using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class ExfiltrationPoints : PointOfInterests
{
	public override string Name => Strings.FeatureExfiltrationPointsName;
	public override string Description => Strings.FeatureExfiltrationPointsDescription;

	[ConfigurationProperty(Order = 10)]
	public Color EligibleColor { get; set; } = Color.green;
		
	[ConfigurationProperty(Order = 10)]
	public Color NotEligibleColor { get; set; } = Color.yellow;

	[ConfigurationProperty(Order = 20)]
	public bool ShowEligible { get; set; } = true;

	[ConfigurationProperty(Order = 20)]
	public bool ShowNotEligible { get; set; } = true;

	[ConfigurationProperty(Order = 20)]
	public string StatusFilter { get; set; } = string.Empty;

	public override float CacheTimeInSec { get; set; } = 7f;
	public override Color GroupingColor => EligibleColor;

	public override void RefreshData(List<PointOfInterest> data)
	{
		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		if (world.ExfiltrationController == null)
			return;

		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var profile = player.Profile;
		var info = profile?.Info;
		if (info == null)
			return;

		var side = info.Side;
		var points = GetExfiltrationPoints(side, world);
		if (points == null)
			return;

		var camera = GameState.Current?.Camera;
		if (camera == null)
			return;

		var eligiblePoints = GetEligibleExfiltrationPoints(side, world, profile!);
		if (eligiblePoints == null)
			return;

		foreach (var point in points)
		{
			if (!point.IsValid()) 
				continue;

			var position = point.transform.position;
			var isEligible = eligiblePoints.Contains(point);

			if (!ShowEligible && isEligible)
				continue;

			if (!ShowNotEligible && !isEligible)
				continue;

			if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter.IndexOf(GetStatus(point.Status), StringComparison.OrdinalIgnoreCase) >= 0)
				continue;

			var poi = Pool.Get();
			poi.Name = GetName(point, isEligible);
			poi.Position = position;
			poi.Color = isEligible ? EligibleColor : NotEligibleColor;
			poi.Owner = null;

			data.Add(poi);
		}
	}

	private static string GetName(ExfiltrationPoint point, bool isEligible)
	{
		var localizedName = point.Settings.Name.Localized();
		return !isEligible ? localizedName : $"{localizedName} ({GetStatus(point.Status)})";
	}

	public static string GetStatus(EExfiltrationStatus status)
	{
		return status switch
		{
			EExfiltrationStatus.AwaitsManualActivation => Strings.FeatureExfiltrationPointsStatusActivate,
			EExfiltrationStatus.Countdown => Strings.FeatureExfiltrationPointsStatusTimer,
			EExfiltrationStatus.NotPresent => Strings.FeatureExfiltrationPointsStatusClosed,
			EExfiltrationStatus.Pending => Strings.FeatureExfiltrationPointsStatusPending,
			EExfiltrationStatus.RegularMode => Strings.FeatureExfiltrationPointsStatusOpen,
			EExfiltrationStatus.UncompleteRequirements => Strings.FeatureExfiltrationPointsStatusRequirement,
			_ => string.Empty
		};
	}

	private static ExfiltrationPoint[]? GetExfiltrationPoints(EPlayerSide side, GameWorld world)
	{
		var ect = world.ExfiltrationController;
		// ReSharper disable once CoVariantArrayConversion
		return side == EPlayerSide.Savage ? ect.ScavExfiltrationPoints : ect.ExfiltrationPoints;
	}

	private static ExfiltrationPoint[]? GetEligibleExfiltrationPoints(EPlayerSide side, GameWorld world, Profile profile)
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

		return [.. result];
	}
}
