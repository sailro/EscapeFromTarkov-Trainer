using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class Players : ToggleMonoBehaviour
	{
		[ConfigurationProperty]
		public Color BearColor { get; set; } = Color.blue;

		[ConfigurationProperty]
		public Color BearBorderColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color UsecColor { get; set; } = Color.green;

		[ConfigurationProperty]
		public Color UsecBorderColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color ScavColor { get; set; } = Color.yellow;

		[ConfigurationProperty]
		public Color ScavBorderColor { get; set; } = Color.red;
		
		[ConfigurationProperty]
		public Color BossColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color BossBorderColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color CultistColor { get; set; } = Color.yellow;

		[ConfigurationProperty]
		public Color CultistBorderColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color ScavRaiderColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public Color ScavRaiderBorderColor { get; set; } = Color.red;

		protected override void UpdateWhenEnabled()
		{
			var hostiles = GameState.Current?.Hostiles;
			if (hostiles == null)
				return;

			foreach (var ennemy in hostiles)
			{
				if (!ennemy.IsValid())
					continue;

				GetPlayerColors(ennemy, out var color, out var borderColor);
				SetShaders(ennemy, GameState.OutlineShader, color, borderColor);
			}
		}

		private void GetPlayerColors(Player player, out Color color, out Color borderColor)
		{
			var info = player.Profile?.Info;
			if (info == null)
			{
				color = ScavColor;
				borderColor = ScavBorderColor;
				return;
			}

			var settings = info.Settings;
			if (settings != null)
			{
				switch(settings.Role)
				{
					case WildSpawnType.pmcBot:
						color = ScavRaiderColor;
						borderColor = ScavRaiderBorderColor;
						return;
					case WildSpawnType.sectantWarrior:
						color = CultistColor;
						borderColor = CultistBorderColor;
						return;
				}

				if (settings.IsBoss())
				{
					color = BossColor;
					borderColor = BossBorderColor;
					return;
				}
			}

			// it can still be a bot in sptarkov but let's use the pmc color
			switch (info.Side)
			{
				case EPlayerSide.Bear:
					color = BearColor;
					borderColor = BearBorderColor;
					break;
				case EPlayerSide.Usec:
					color = UsecColor;
					borderColor = UsecBorderColor;
					break;
				default:
					color = ScavColor;
					borderColor = ScavBorderColor;
					break;
			}
		}

		private static void SetShaders(Player player, Shader shader, Color color, Color borderColor)
		{
			var skins = player.PlayerBody.BodySkins;
			foreach (var skin in skins.Values)
			{
				if (skin == null)
					continue;

				foreach (var renderer in skin.GetRenderers())
				{
					if (renderer == null)
						continue;

					var material = renderer.material;
					if (material == null)
						continue;

					if (material.shader != null && material.shader == shader)
						continue;

					material.shader = shader;

					material.SetColor("_FirstOutlineColor", borderColor);
					material.SetFloat("_FirstOutlineWidth", 0.02f);
					material.SetColor("_SecondOutlineColor", color);
					material.SetFloat("_SecondOutlineWidth", 0.0025f);
				}
			}
		}
	}
}
