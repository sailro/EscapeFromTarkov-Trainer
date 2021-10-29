using EFT.Trainer.Configuration;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class TrackedItem
	{
		public TrackedItem(string name, Color? color = null)
		{
			Name = name;
			Color = color;
		}

		public string Name { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? Color { get; set; }
	}
}
