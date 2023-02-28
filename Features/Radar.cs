using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Radar : ToggleFeature
	{
		public override string Name => "radar";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty(Order = 10)]
		public float RadarPercentage { get; set; } = 10f;

		[ConfigurationProperty(Order = 20)]
		public float RadarRange { get; set; } = 100f;

		[ConfigurationProperty(Order = 30)]
		public Color RadarBackground { get; set; } = new Color(0f, 0f, 0f, 0.5f);

		[ConfigurationProperty(Order = 30)]
		public Color RadarCrosshair { get; set; } = new Color(1f, 1f, 1f, 0.5f);

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
		public Color BearColors { get; set; } = Color.blue;

		[ConfigurationProperty(Order = 100)]
		public Color UsecColors { get; set; } = Color.green;

		[ConfigurationProperty(Order = 110)]
		public Color ScavColors { get; set; } = Color.yellow;

		[ConfigurationProperty(Order = 120)]
		public Color BossColors { get; set; } = Color.red;

		[ConfigurationProperty(Order = 130)]
		public Color CultistColors { get; set; } = Color.yellow;

		[ConfigurationProperty(Order = 140)]
		public Color ScavRaiderColors { get; set; } = Color.yellow;



		[UsedImplicitly]
		protected void OnGUI()
		{
			if (!Enabled)
				return;

			var screenPercentage = RadarPercentage / 100f;
			if (RadarRange <= 0 || screenPercentage > 1)
				return;

			var hostiles = GameState.Current?.Hostiles;
			if (hostiles == null)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var radarSize = Mathf.Sqrt(Screen.height * Screen.width * screenPercentage) / 2;
			var radarX = Screen.width - radarSize;
			var radarY = Screen.height - radarSize;

			foreach (var enemy in hostiles)
			{
				if (!enemy.IsValid())
					continue;

				var position = enemy.Transform.position;

				var distance = Mathf.Round(Vector3.Distance(camera.transform.position, position));
				if (RadarRange > 0 && distance > RadarRange)
					continue;

				var hostileType = GetHostileType(enemy);

				if (hostileType == 0 && !ShowScavs)
					continue;

				if (hostileType == 1 && !ShowScavRaiders)
					continue;

				if (hostileType == 2 && !ShowCultists)
					continue;

				if (hostileType == 3 && !ShowBosses)
					continue;

				if ((hostileType == 4 || hostileType == 5) && !ShowPlayers)
					continue;

				var playerColor = hostileType switch
				{
					0 => ScavColors,
					1 => ScavRaiderColors,
					2 => CultistColors,
					3 => BossColors,
					4 => BearColors,
					5 => UsecColors,
					_ => ScavColors,
				};
				
				DrawRadarEnemy(player, enemy, radarSize, distance, playerColor);

				}

			//To Render Our background
			Render.DrawCrosshair(new Vector2(radarX + (radarSize / 2), radarY + (radarSize / 2)), radarSize / 2, RadarCrosshair, 2f);
			Render.DrawBox(radarX, radarY, radarSize, radarSize, 2f, RadarBackground);
		}

		private int GetHostileType(Player player)
		{
			var info = player.Profile?.Info;
			if (info == null)
				return 0;

			var settings = info.Settings;
			if (settings != null)
			{
				switch (settings.Role)
				{
					case WildSpawnType.pmcBot:
						return 1;
					case WildSpawnType.sectantWarrior:
						return 2;
				}

				if (settings.IsBoss())
					return 3;
			}

			// it can still be a bot in sptarkov but let's use the pmc color
			return info.Side switch
			{
				EPlayerSide.Bear => 4,
				EPlayerSide.Usec => 5,
				_ => 0
			};
		}

		private void DrawRadarEnemy(Player player, Player enemy, float radarSize, float distance, Color playerColor)
		{
			var radarX = Screen.width - radarSize;
			var radarY = Screen.height - radarSize;

			//Draw Box
			float enemyY = player.Transform.position.x - enemy.Transform.position.x;
			float enemyX = player.Transform.position.z - enemy.Transform.position.z;
			float enemyAtan = Mathf.Atan2(enemyY, enemyX) * Mathf.Rad2Deg - 270 - player.Transform.eulerAngles.y;

			float enemyRadarX = distance * Mathf.Cos(enemyAtan * Mathf.Deg2Rad);
			float enemyRadarY = distance * Mathf.Sin(enemyAtan * Mathf.Deg2Rad);

			enemyRadarX = enemyRadarX * (radarSize / RadarRange) / 2f;
			enemyRadarY = enemyRadarY * (radarSize / RadarRange) / 2f;

			Render.DrawCircle(new Vector2(radarX + radarSize / 2f + enemyRadarX, radarY + radarSize / 2f + enemyRadarY), 10f, playerColor, 2f, 8);
		}
	}
}
