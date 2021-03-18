using UnityEngine;

namespace EFT.Trainer.Features
{
	public struct PointOfInterest
	{
		public string Name { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 ScreenPosition { get; set; }
		public Color Color { get; set; }
	}
}
