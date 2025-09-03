﻿using System.Collections.Generic;
using Comfort.Common;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Grenades : CachableFeature<Throwable>
{
	public override string Name => Strings.FeatureGrenadesName;
	public override string Description => Strings.FeatureGrenadesDescription;

	[ConfigurationProperty]
	public Color Color { get; set; } = Color.red;

	public override bool Enabled { get; set; } = false;
	public override float CacheTimeInSec { get; set; } = 0.25f;

	public override void RefreshData(List<Throwable> data)
	{
		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var grenades = world.Grenades;
		if (grenades == null)
			return;

		for (var i = 0; i < grenades.Count; i++)
		{
			var grenade = grenades.GetByIndex(i);
			if (!grenade.IsValid())
				continue;

			data.Add(grenade);
		}
	}

	public override void ProcessData(IReadOnlyList<Throwable> data)
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

			material.SetColor(ShaderProperties.FirstOutlineColor, color);
			material.SetFloat(ShaderProperties.FirstOutlineWidth, 0.02f);
			material.SetColor(ShaderProperties.SecondOutlineColor, color);
			material.SetFloat(ShaderProperties.SecondOutlineWidth, 0.0025f);
			material.SetFloat(ShaderProperties.ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
		}
	}
}
