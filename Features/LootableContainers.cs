using System.Collections.Generic;
using Comfort.Common;
using EFT.Interactive;
using EFT.InventoryLogic;
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

		var owners = world.ItemOwners;
		var records = new List<PointOfInterest>();

		foreach (var owner in owners)
		{
			var itemOwner = owner.Key;
			var rootItem = itemOwner.RootItem;

			if (!rootItem.IsValid())
				continue;

			if (rootItem is not { IsContainer: true })
				continue;

			if (ShowContainers && _targetedContainer.Contains(rootItem.TemplateId))
				AddRecord(rootItem.TemplateId.LocalizedShortName(), owner.Value.Transform.position, records);

			if (ShowCorpses && rootItem.TemplateId == KnownTemplateIds.DefaultInventory 
			                && itemOwner is TraderControllerClass { Name: nameof(Corpse) }) // only display dead bodies
				AddRecord(nameof(Corpse), owner.Value.Transform.position, records);
		}

		return [.. records];
	}

	private void AddRecord(string itemName, Vector3 position, List<PointOfInterest> records)
	{
		records.Add(new PointOfInterest
		{
			Name = itemName,
			Position = position,
			Color = Color
		});
	}
}
