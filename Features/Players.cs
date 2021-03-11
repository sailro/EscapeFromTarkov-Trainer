using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Players : MonoBehaviour
	{
		internal static readonly Color PlayerColor = Color.blue;
		internal static readonly Color BotColor = Color.yellow;
		internal static readonly Color BossColor = Color.red;

		private Shader _outline;

		private void Awake()
		{
			var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "outline"));
			_outline = bundle.LoadAsset<Shader>("assets/outline.shader");
		}

		private void Update()
		{
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

		private static Color GetPlayerColor(Player player)
		{
			var settings = player.Profile?.Info?.Settings;
			if (settings != null && settings.IsBoss())
				return BossColor;

			return player.IsAI ? BotColor : PlayerColor;
		}

		private static void SetShaders(Player player, Shader shader, Color color)
		{
			var transform = player.Transform.Original;

			var renderers = new List<Renderer>();
			var eligibleRenderers = new List<Renderer>();

			foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
			{
				var material = renderer.material;
				var current = material.shader;
				if (current.name == shader.name || !current.name.StartsWith("p0"))
					continue;

				renderers.Add(renderer);
			}

			Dispatch(renderers, out var chestRigs, out var armors, out var tops, out var helmets, out var heads, out var others);

			if (chestRigs.Any())
				eligibleRenderers.AddRange(chestRigs);
			else if (armors.Any())
				eligibleRenderers.AddRange(armors);
			else
				eligibleRenderers.AddRange(tops);

			eligibleRenderers.AddRange(helmets.Any() ? helmets : heads);
			eligibleRenderers.AddRange(others);

			foreach (var renderer in eligibleRenderers)
			{
				var material = renderer.material;
				material.shader = shader;
				material.SetColor("_FirstOutlineColor", Color.red);
				material.SetFloat("_FirstOutlineWidth", 0.02f);
				material.SetColor("_SecondOutlineColor", color);
				material.SetFloat("_SecondOutlineWidth", 0.0025f);
			}
		}

		private static void Dispatch(List<Renderer> renderers, out List<Renderer> chestRigs, out List<Renderer> armors, out List<Renderer> tops, out List<Renderer> helmets, out List<Renderer> heads, out List<Renderer> others)
		{
			chestRigs = new List<Renderer>();
			armors = new List<Renderer>();
			tops = new List<Renderer>();
			helmets = new List<Renderer>();
			heads = new List<Renderer>();
			others = new List<Renderer>();

			const StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;

			foreach (var renderer in renderers)
			{
				var name = renderer.name;
				if (name.StartsWith("CR_", comparisonType))
				{
					chestRigs.Add(renderer);
					continue;
				}
				if (name.StartsWith("AR_", comparisonType))
				{
					armors.Add(renderer);
					continue;
				}
				if (name.StartsWith("Top_", comparisonType))
				{
					tops.Add(renderer);
					continue;
				}
				if (name.StartsWith("Pants_", comparisonType))
				{
					others.Add(renderer);
					continue;
				}
				if (name.IndexOf("_helmet_", comparisonType) >= 0)
				{
					helmets.Add(renderer);
					continue;
				}
				if (name.IndexOf("_head_", comparisonType) >= 0)
				{
					heads.Add(renderer);
					continue;
				}
			}
		}
	}
}
