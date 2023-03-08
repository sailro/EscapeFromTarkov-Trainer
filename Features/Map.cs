using EFT.Trainer.Configuration;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Map : BaseMapToggleFeature
	{
		public override string Name => "map";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty(Order = 20)]
		public float Range { get; set; } = 400f;

		protected override void OnGUIWhenEnabled()
		{
			if (Range <= 0)
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var cameraTransform = camera.transform;

			SetupMapCameraOnce(camera, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
			UpdateMapCamera(cameraTransform, Range);
		}

		protected override void UpdateWhenDisabled()
		{
			ToggleMapCameraIfNeeded(false);
		}
	}
}
