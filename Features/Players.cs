using System.IO;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Players : MonoBehaviour, IEnableable
	{
		[ConfigurationProperty]
		public Color BearPlayerColor { get; set; } = Color.blue;

		[ConfigurationProperty]
		public Color UsecPlayerColor { get; set; } = Color.green;

		[ConfigurationProperty]
		public Color BotColor { get; set; } = Color.yellow;
		
		[ConfigurationProperty]
		public Color BossColor { get; set; } = Color.red;

		[ConfigurationProperty]
		public bool Enabled { get; set; } = true;

		private Shader _outline;

		private void Awake()
		{
			var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "outline"));
			_outline = bundle.LoadAsset<Shader>("assets/outline.shader");
		}

		private void Update()
		{
			if (!Enabled)
				return;

			var hostiles = GameState.Current?.Hostiles;
			if (hostiles == null)
				return;

			foreach (var ennemy in hostiles)
			{
				if (!ennemy.IsValid())
					continue;

				var color = GetPlayerColor(ennemy);
				SetShaders(ennemy, _outline, color);
			}
		}

		private Color GetPlayerColor(Player player)
		{
			var info = player.Profile?.Info;
			if (info == null)
				return BotColor;

			var settings = info.Settings;
			if (settings != null && settings.IsBoss())
				return BossColor;

			// it can still be a bot in sptarkov but let's use the pmc color
			return info.Side switch
			{
				EPlayerSide.Bear => BearPlayerColor,
				EPlayerSide.Usec => UsecPlayerColor,
				_ => BotColor
			};
		}

		private static void SetShaders(Player player, Shader shader, Color color)
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

					material.shader = shader;

					material.SetColor("_FirstOutlineColor", Color.red);
					material.SetFloat("_FirstOutlineWidth", 0.02f);
					material.SetColor("_SecondOutlineColor", color);
					material.SetFloat("_SecondOutlineWidth", 0.0025f);
				}
			}
		}
	}
}
