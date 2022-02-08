using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Grenades : CachableFeature<Throwable[]>
	{
		public override string Name => "grenade";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.red;

		public override bool Enabled { get; set; } = false;
		public override float CacheTimeInSec { get; set; } = 0.25f;

		public static Throwable[] Empty => Array.Empty<Throwable>();

		public override Throwable[] RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			var grenades = world.Grenades;
			if (grenades == null)
				return Empty;

			var result = new List<Throwable>();
			for (var i = 0; i < grenades.Count; i++)
			{
				var grenade = grenades.GetByIndex(i);
				if (!grenade.IsValid())
					continue;

				result.Add(grenade);
			}

			return result.ToArray();
		}

		public override void ProcessData(Throwable[] data)
		{
			foreach (var throwable in data)
			{
				if (!throwable.IsValid())
					return;

				SetShaders(throwable, GameState.OutlineShader, Color);
			}
		}

		private static void SetShaders(Throwable throwable, Shader? shader, Color color)
		{
			var transform = throwable.transform;
			foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
			{
				if (renderer == null)
					continue;

				var material = renderer.material;
				if (material == null)
					continue;

				if (material.shader != null && material.shader == shader)
					continue;

				material.shader = shader;

				material.SetColor("_FirstOutlineColor", color);
				material.SetFloat("_FirstOutlineWidth", 0.02f);
				material.SetColor("_SecondOutlineColor", color);
				material.SetFloat("_SecondOutlineWidth", 0.0025f);
				material.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
			}
		}
	}
}
