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

		private Camera? _mapCamera = null;

		protected override void OnGUIWhenEnabled()
		{
			if (Range <= 0)
				return;

			var snapshot = GameState.Current;
			if (snapshot == null)
				return;

			snapshot.MapMode = true;

			var camera = snapshot.Camera;
			if (camera == null)
				return;

			var hostiles = snapshot.Hostiles;
			var width = Screen.currentResolution.width;
			var height = Screen.currentResolution.height;

			SetupMapCamera(camera, 0, 0, width, height);
			UpdateMapCamera(camera, Range);

			if (_mapCamera == null)
			{
				foreach (var cameras in Camera.allCameras)
				{
					if (cameras.name == "EFT.Trainer.Features.Map_mapCamera")
					{
						_mapCamera = cameras;
					}
				}
			}

			if (_mapCamera == null)
				return;

			DrawHostiles(_mapCamera, hostiles, Range);
		}

		protected override void UpdateWhenDisabled()
		{
			ToggleMapCameraIfNeeded(false);

			var snapshot = GameState.Current;
			if (snapshot == null)
				return;

			snapshot.MapMode = false;
		}
	}
}
