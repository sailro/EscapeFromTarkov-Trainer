using System;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class TrackList : ConsoleCommandWithoutArgument
{
	public override string Name => "tracklist";

	// Unfortunately we cannot use BaseTrackCommand or LootItemsRelatedCommand here
	private readonly Lazy<LootItems> _lootItems = new(() => FeatureFactory.GetFeature<LootItems>()!);
	private LootItems LootItems => _lootItems.Value;


	public override void Execute()
	{
		ShowTrackList(this, LootItems);
	}

	internal static void ShowTrackList(ConsoleCommand command, LootItems lootItems, bool changed = false)
	{
		if (changed)
			command.AddConsoleLog("Tracking list updated...");

		foreach (var templateId in lootItems.Wishlist)
			command.AddConsoleLog($"Tracking: {templateId.LocalizedShortName()} (Wishlist)");

		foreach (var item in lootItems.TrackedNames)
		{
			var extra = item.Rarity.HasValue ? $" ({item.Rarity.Value.Color()})" : string.Empty;
			command.AddConsoleLog(item.Color.HasValue ? $"Tracking: {item.Name.Color(item.Color.Value)}{extra}" : $"Tracking: {item.Name}{extra}");
		}
	}
}
