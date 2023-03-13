using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	// We do not want to store camera-relative stuff here, given the player is moving.
	// We'll need to re-compute the screen-position/distance/... when using OnGUI
	internal struct PointOfInterest
	{
		public string Name { get; set; }
		public string? Owner { get; set; }
		public Vector3 Position { get; set; }
		public Color Color { get; set; }
	}
}
