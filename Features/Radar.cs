using EFT.Trainer.Configuration;
using EFT.Trainer.UI;
using EFT.UI;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Radar : BaseMapToggleFeature
	{
		public override string Name => "radar";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty(Order = 10)]
		public float RadarPercentage { get; set; } = 10f;

		[ConfigurationProperty(Order = 20)]
		public float RadarRange { get; set; } = 100f;

		[ConfigurationProperty(Order = 30)]
		public Color RadarBackground { get; set; } = new(0f, 0f, 0f, 0.5f);

		[ConfigurationProperty(Order = 30)]
		public Color RadarCrosshair { get; set; } = new(1f, 1f, 1f, 0.5f);

		[ConfigurationProperty(Order = 100)]
		public bool ShowMap { get; set; } = false;

		[ConfigurationProperty(Order = 101)]
		public bool ShowCompass { get; set; } = false;

		private Camera? _radarMapCamera = null;

		protected override void OnGUIWhenEnabled()
		{
			if (RadarRange <= 0)
				return;

			var screenPercentage = RadarPercentage / 100f;
			if (screenPercentage > 1)
				screenPercentage = 1;

			var snapshot = GameState.Current;
			if (snapshot == null)
				return;

			if (snapshot.MapMode)
				return;

			var camera = snapshot.Camera;
			if (camera == null)
				return;

			var hostiles = snapshot.Hostiles;

			var radarSize = Mathf.Sqrt(Screen.height * Screen.width * screenPercentage) / 2;
			var radarX = Screen.width - radarSize;
			var radarY = Screen.height - radarSize;

			if (ShowMap)
			{
				SetupMapCamera(camera, radarX, Screen.currentResolution.height - radarY - radarSize, radarSize, radarSize);
				UpdateMapCamera(camera, RadarRange);

				if (_radarMapCamera == null)
				{
					foreach (var cameras in Camera.allCameras)
					{
						if (cameras.name == "EFT.Trainer.Features.Radar_mapCamera")
						{
							_radarMapCamera = cameras;
						}
					}
				}

				if (_radarMapCamera == null)
					return;

				DrawHostiles(_radarMapCamera, hostiles, RadarRange);
			}
			else
			{
				ToggleMapCameraIfNeeded(false);
				DrawHostiles(camera, hostiles, RadarRange, radarX, radarY, radarSize);
			}


			var forward = camera.transform.forward;
			var right = camera.transform.right;
			forward.y = 0;
			right.y = 0;

			var forwardHeading = GetHeadingAngle(forward);
			var rearHeading = GetHeadingAngle(-forward);
			var rightHeading = GetHeadingAngle(right);
			var leftHeading = GetHeadingAngle(-right);

			if (ShowCompass)
			{
				var radarTop = new Vector2(radarX + radarSize / 2, radarY + 12f);
				var radarLeft = new Vector2(radarX + 12f, radarY + radarSize / 2);
				var radarRight = new Vector2(radarX + radarSize - 12f, radarY + radarSize / 2);
				var radarBottom = new Vector2(radarX + radarSize / 2, radarY + radarSize - 12f);
				Render.DrawString(radarTop, forwardHeading);
				Render.DrawString(radarLeft, leftHeading);
				Render.DrawString(radarRight, rightHeading);
				Render.DrawString(radarBottom, rearHeading);
				Render.DrawCrosshair(new Vector2(radarX + radarSize / 2, radarY + radarSize / 2), 25f, RadarCrosshair, 2f);
			}
			else
				Render.DrawCrosshair(new Vector2(radarX + radarSize / 2, radarY + radarSize / 2), radarSize / 2, RadarCrosshair, 2f);

			Render.DrawBox(radarX, radarY, radarSize, radarSize, 2f, RadarBackground);
		}

		protected override void UpdateWhenDisabled()
		{
			ToggleMapCameraIfNeeded(false);
		}
	}
}
