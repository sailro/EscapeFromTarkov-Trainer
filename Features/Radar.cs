using EFT.Trainer.Configuration;
using EFT.Trainer.UI;
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

		private float ScreenPercentage => Mathf.Min(RadarPercentage / 100f, 1);
		private float RadarSize => Mathf.Sqrt(Screen.height * Screen.width * ScreenPercentage) / 2;
		private float RadarX => Screen.width - RadarSize;
		private float RadarY => Screen.height - RadarSize;

		protected override void OnGUIWhenEnabled()
		{
			if (RadarRange <= 0)
				return;

			var snapshot = GameState.Current;
			if (snapshot == null)
				return;

			if (snapshot.MapMode)
				return;

			var camera = snapshot.Camera;
			if (camera == null)
				return;

			var hostiles = snapshot.Hostiles;

			var radarX = RadarX;
			var radarY = RadarY;
			var radarSize = RadarSize;

			if (ShowMap)
			{
				SetupMapCamera(camera, radarX, Screen.currentResolution.height - radarY - radarSize, radarSize, radarSize);
				UpdateMapCamera(camera, RadarRange);

				if (MapCamera != null)
					DrawHostiles(MapCamera, hostiles, RadarRange);
			}
			else
			{
				ToggleMapCameraIfNeeded(false);
				DrawHostiles(camera, hostiles, RadarRange);
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
				Render.DrawString(radarTop, forwardHeading, RadarCrosshair);
				Render.DrawString(radarLeft, leftHeading, RadarCrosshair);
				Render.DrawString(radarRight, rightHeading, RadarCrosshair);
				Render.DrawString(radarBottom, rearHeading, RadarCrosshair);
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

		protected override Vector2 GetTargetPosition(Vector3 playerPosition, Vector3 targetPosition, float playerEulerY)
		{
			if (MapCamera != null && MapCamera.enabled)
				return MapCamera.WorldToScreenPoint(targetPosition);

			float enemyY = playerPosition.x - targetPosition.x;
			float enemyX = playerPosition.z - targetPosition.z;
			float enemyAtan = Mathf.Atan2(enemyY, enemyX) * Mathf.Rad2Deg - 270 - playerEulerY;

			var enemyDistance = Mathf.Round(Vector3.Distance(playerPosition, targetPosition));

			float enemyMapX = enemyDistance * Mathf.Cos(enemyAtan * Mathf.Deg2Rad);
			float enemyMapY = enemyDistance * Mathf.Sin(enemyAtan * Mathf.Deg2Rad);

			var radarSize = RadarSize;
			var range = RadarRange;

			enemyMapX = enemyMapX * (radarSize / range) / 2f;
			enemyMapY = enemyMapY * (radarSize / range) / 2f;

			return new Vector2(RadarX + radarSize / 2f + enemyMapX, RadarY + radarSize / 2f + enemyMapY);
		}

		protected override void AdjustTargetPositionForRender(ref Vector2 position)
		{
			if (MapCamera != null && MapCamera.enabled)
				position.y = Screen.height - position.y;
		}
	}
}
