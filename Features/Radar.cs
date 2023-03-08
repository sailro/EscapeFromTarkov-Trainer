using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
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

		[ConfigurationProperty(Order = 40)] 
		public bool ShowPlayers { get; set; } = true;

		[ConfigurationProperty(Order = 50)]
		public bool ShowScavs { get; set; } = true;

		[ConfigurationProperty(Order = 60)]
		public bool ShowScavRaiders { get; set; } = true;

		[ConfigurationProperty(Order = 70)]
		public bool ShowBosses { get; set; } = true;

		[ConfigurationProperty(Order = 80)]
		public bool ShowCultists { get; set; } = true;

		[ConfigurationProperty(Order = 90)]
		public bool ShowMap { get; set; } = false;

		private enum HostileType
		{
			Scav,
			ScavRaider,
			Boss,
			Cultist,
			Bear,
			Usec,
		}

		protected override void OnGUIWhenEnabled()
		{
			if (RadarRange <= 0)
				return;

			var screenPercentage = RadarPercentage / 100f;
			if (screenPercentage > 1)
				screenPercentage = 1;

			var hostiles = GameState.Current?.Hostiles;
			if (hostiles == null)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var feature = FeatureFactory.GetFeature<Players>();
			if (feature == null)
				return;

			var radarSize = Mathf.Sqrt(Screen.height * Screen.width * screenPercentage) / 2;
			var radarX = Screen.width - radarSize;
			var radarY = Screen.height - radarSize;

			var cameraTransform = camera.transform;

			if (ShowMap)
			{
				SetupMapCameraOnce(camera, radarX, Screen.currentResolution.height - radarY - radarSize, radarSize, radarSize);
				UpdateMapCamera(cameraTransform, RadarRange);
			}
			else
			{
				ToggleMapCameraIfNeeded(false);
			}

			foreach (var enemy in hostiles)
			{
				if (!enemy.IsValid())
					continue;

				var position = enemy.Transform.position;

				var distance = Mathf.Round(Vector3.Distance(cameraTransform.position, position));
				if (RadarRange > 0 && distance > RadarRange)
					continue;

				var hostileType = GetHostileType(enemy);

				switch (hostileType)
				{
					case HostileType.Scav when !ShowScavs:
					case HostileType.ScavRaider when !ShowScavRaiders:
					case HostileType.Cultist when !ShowCultists:
					case HostileType.Boss when !ShowBosses:
					case HostileType.Bear or HostileType.Usec when !ShowPlayers:
						continue;

					default:
					{
						var playerColor = feature.GetPlayerColors(enemy);
						DrawRadarEnemy(camera, enemy, radarSize, playerColor.Color);
						break;
					}
				}
			}

			Render.DrawCrosshair(new Vector2(radarX + radarSize / 2, radarY + radarSize / 2), radarSize / 2, RadarCrosshair, 2f);
			Render.DrawBox(radarX, radarY, radarSize, radarSize, 2f, RadarBackground);
		}

		protected override void UpdateWhenDisabled()
		{
			ToggleMapCameraIfNeeded(false);
		}

		private static HostileType GetHostileType(Player player)
		{
			var info = player.Profile?.Info;
			if (info == null)
				return HostileType.Scav;

			var settings = info.Settings;
			if (settings != null)
			{
				switch (settings.Role)
				{
					case WildSpawnType.pmcBot:
						return HostileType.ScavRaider;
					case WildSpawnType.sectantWarrior:
						return HostileType.Cultist;
				}

				if (settings.IsBoss())
					return HostileType.Boss;
			}

			return info.Side switch
			{
				EPlayerSide.Bear => HostileType.Bear,
				EPlayerSide.Usec => HostileType.Usec,
				_ => HostileType.Scav
			};
		}

		private void DrawRadarEnemy(Camera camera, Player enemy, float radarSize, Color playerColor)
		{
			var radarX = Screen.width - radarSize;
			var radarY = Screen.height - radarSize;

			var cameraTransform = camera.transform;
			var cameraPosition = cameraTransform.position;

			var enemyPosition = enemy.Transform.position;
			var cameraEulerY = cameraTransform.eulerAngles.y;

			var enemyRadar = FindRadarPoint(cameraPosition, enemyPosition, cameraEulerY, radarX, radarY, radarSize);

			var enemyLookDirection = enemy.LookDirection;

			var enemyOffset = enemyPosition + enemyLookDirection * 8f;
			var playerRealRight = enemy.MovementContext.PlayerRealRight;

			var enemyOffset2 = enemyPosition + enemyLookDirection * 4f + playerRealRight * 2f;
			var enemyOffset3 = enemyPosition + enemyLookDirection * 4f - playerRealRight * 2f;

			var enemyForward = FindRadarPoint(cameraPosition, enemyOffset, cameraEulerY, radarX, radarY, radarSize);
			var enemyArrow = FindRadarPoint(cameraPosition, enemyOffset2, cameraEulerY, radarX, radarY, radarSize);
			var enemyArrow2 = FindRadarPoint(cameraPosition, enemyOffset3, cameraEulerY, radarX, radarY, radarSize);

			Render.DrawLine(enemyRadar, enemyForward, 2f, Color.white);
			Render.DrawLine(enemyArrow, enemyForward, 2f, Color.white);
			Render.DrawLine(enemyArrow2, enemyForward, 2f, Color.white);
			Render.DrawCircle(enemyRadar, 10f, playerColor, 2f, 8);
		}

		private Vector2 FindRadarPoint(Vector3 playerPosition, Vector3 enemyPosition, float playerEulerY, float radarX, float radarY, float radarSize)
		{
			float enemyY = playerPosition.x - enemyPosition.x;
			float enemyX = playerPosition.z - enemyPosition.z;
			float enemyAtan = Mathf.Atan2(enemyY, enemyX) * Mathf.Rad2Deg - 270 - playerEulerY;

			var enemyDistance = Mathf.Round(Vector3.Distance(playerPosition, enemyPosition));

			float enemyRadarX = enemyDistance * Mathf.Cos(enemyAtan * Mathf.Deg2Rad);
			float enemyRadarY = enemyDistance * Mathf.Sin(enemyAtan * Mathf.Deg2Rad);

			enemyRadarX = enemyRadarX * (radarSize / RadarRange) / 2f;
			enemyRadarY = enemyRadarY * (radarSize / RadarRange) / 2f;

			return new Vector2(radarX + radarSize / 2f + enemyRadarX, radarY + radarSize / 2f + enemyRadarY);
		}
	}
}
