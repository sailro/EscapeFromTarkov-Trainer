using EFT.Trainer.Configuration;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal class TrackedItem(string name, Color? color = null, ELootRarity? rarity = null)
{
	public string Name { get; set; } = name;

	[JsonConverter(typeof(ColorConverter))]
	public Color? Color { get; set; } = color;

	public ELootRarity? Rarity { get; set; } = rarity;
}
