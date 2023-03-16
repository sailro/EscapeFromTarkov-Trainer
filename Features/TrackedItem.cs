using EFT.Trainer.Configuration;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal class TrackedItem
	{
		public TrackedItem(string name, Color? color = null, ELootRarity? rarity = null, bool wishlist = false)
		{
			Name = name;
			Color = color;
			Rarity = rarity;
			Wishlist = wishlist;
		}

		public string Name { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? Color { get; set; }

		public ELootRarity? Rarity { get; set; }

		public bool Wishlist { get; set; }
	}
}
