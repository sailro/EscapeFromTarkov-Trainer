using System.Collections.Generic;
using Comfort.Common;
using EFT.Interactive;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class LootableContainers : PointOfInterests
{
	public override string Name => "stash";

	//Static Hashset for List of Container IDS
	private static readonly HashSet<string> _targetedContainer =
	[
		KnownTemplateIds.BuriedBarrelCache,
		KnownTemplateIds.GroundCache,
		KnownTemplateIds.AirDropCommon,
		KnownTemplateIds.AirDropMedical,
		KnownTemplateIds.AirDropSupply,
		KnownTemplateIds.AirDropWeapon
	];
	
	[ConfigurationProperty]
	public Color Color { get; set; } = Color.white;

	public override float CacheTimeInSec { get; set; } = 11f;
	public override bool Enabled { get; set; } = false;
	public override Color GroupingColor => Color;

	[ConfigurationProperty]
	public bool ShowContainers {  get; set; } = true;

	[ConfigurationProperty]
	public bool ShowCorpses {  get; set; } = true;

	public override void RefreshData(List<PointOfInterest> data)
	{
		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var camera = GameState.Current?.Camera;
		if (camera == null)
			return;

		var owners = world.ItemOwners;

		foreach (var owner in owners)
		{
			var itemOwner = owner.Key;
			var rootItem = itemOwner.RootItem;

			if (!rootItem.IsValid())
				continue;

			if (rootItem is not { IsContainer: true })
				continue;

			if (ShowContainers && _targetedContainer.Contains(rootItem.TemplateId))
				AddRecord(rootItem.TemplateId.LocalizedShortName(), owner.Value.Transform.position, data);

			if (ShowCorpses && rootItem.TemplateId == KnownTemplateIds.DefaultInventory 
			                && itemOwner is TraderControllerClass { Name: nameof(Corpse) }) // only display dead bodies
				AddRecord(nameof(Corpse), owner.Value.Transform.position, data);
		}
	}

	private void AddRecord(string itemName, Vector3 position, List<PointOfInterest> records)
	{
		var poi = Pool.Get();
		poi.Name = itemName;
		poi.Position = position;
		poi.Color = Color;
		poi.Owner = null;

		records.Add(poi);
	}
}
