using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal struct PointOfInterest
	{
		public string Name { get; set; }
		public string? Owner { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 ScreenPosition { get; set; }
		public Color Color { get; set; }
	}
}
