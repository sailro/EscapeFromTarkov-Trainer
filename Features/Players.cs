﻿using System.Collections.Generic;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Players : ToggleFeature
	{
		public override string Name => "wallhack";

		[ConfigurationProperty(Order = 10)]
		public Color BearColor { get; set; } = Color.blue;

		[ConfigurationProperty(Order = 10)]
		public Color BearBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color BearInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color UsecColor { get; set; } = Color.green;

		[ConfigurationProperty(Order = 10)]
		public Color UsecBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color UsecInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color ScavColor { get; set; } = Color.yellow;

		[ConfigurationProperty(Order = 10)]
		public Color ScavBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color ScavInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color BossColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color BossBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color BossInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color CultistColor { get; set; } = Color.yellow;

		[ConfigurationProperty(Order = 10)]
		public Color CultistBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color CultistInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color ScavRaiderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color ScavRaiderBorderColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 10)]
		public Color ScavRaiderInfoColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 20)]
		public bool ShowBoxes { get; set; } = true;

		[ConfigurationProperty(Order = 21)]
		public float BoxThickness { get; set; } = 2f;

		[ConfigurationProperty(Order = 30)]
		public bool ShowCharms { get; set; } = true;

		[ConfigurationProperty(Order = 31)]
		public bool XRayVision { get; set; } = true;
		
		[ConfigurationProperty(Order = 40)]
		public bool ShowInfos { get; set; } = true;

		[ConfigurationProperty(Order = 50)]
		public bool ShowSkeletons { get; set; } = false;

		[ConfigurationProperty(Order = 51)]
		public float SkeletonThickness { get; set; } = 2;

		[ConfigurationProperty(Order = 60)]
		public bool ShowShootable { get; set; } = false;

		[ConfigurationProperty(Order = 61)]
		public Color ShootableColor { get; set; } = Color.green;

		[ConfigurationProperty(Order = 62)]
		public Color ShootableBoxColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 63)]
		public Color NotShootableColor { get; set; } = Color.red;

		[ConfigurationProperty(Order = 64)]
		public Color NotShootableBoxColor { get; set; } = Color.blue;

		[ConfigurationProperty(Order = 19)]
		public float MaximumDistance { get; set; } = 0f;

		private static bool _lastXRayVision = true;
		private static bool _lastShowCharms = true;

		[UsedImplicitly]
		protected void OnGUI()
		{
			var hostiles = GameState.Current?.Hostiles;
			if (hostiles == null)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var cacheComponent = player.GetOrAddComponent<ShaderCache>();
			var cache = cacheComponent.Cache;

			if (!Enabled || XRayVision != _lastXRayVision || ShowCharms != _lastShowCharms)
			{
				_lastXRayVision = XRayVision;
				_lastShowCharms = ShowCharms;

				if (cache.Count > 0)
					ResetShaders(cache);

				return;
			}

			foreach (var ennemy in hostiles)
			{
				if (!ennemy.IsValid())
					continue;

				GetPlayerColors(ennemy, out var color, out var borderColor, out var infoColor);

				if (ShowCharms)
					SetShaders(ennemy, GameState.OutlineShader, color, borderColor, cache);

				var position = ennemy.Transform.position;
				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Mathf.Round(Vector3.Distance(camera.transform.position, position));
				if (MaximumDistance > 0 && distance > MaximumDistance)
					continue;

				var playerBones = ennemy.PlayerBones;
				if (playerBones == null)
					continue;

				if (ShowShootable)
				{
					var shootable = camera.IsTransformVisible(playerBones.Head.Original);

					if (shootable)
					{
						color = ShootableColor;
						borderColor = ShootableBoxColor;
					}	
					else
					{
						color = NotShootableColor;
						borderColor = NotShootableBoxColor;
					}
				}

				var headScreenPosition = camera.WorldPointToScreenPoint(playerBones.Head.position);
				var leftShoulderScreenPosition = camera.WorldPointToScreenPoint(playerBones.LeftShoulder.position);
				var heightOffset = Mathf.Abs(headScreenPosition.y - leftShoulderScreenPosition.y);

				var boxHeight = Mathf.Abs(headScreenPosition.y - screenPosition.y) + heightOffset * 3f;
				var boxWidth = boxHeight * 0.62f;

				var boxPositionX = screenPosition.x - boxWidth / 2f;
				var boxPositionY = headScreenPosition.y - heightOffset * 2f;
				
				if (ShowBoxes)
					Render.DrawBox(boxPositionX, boxPositionY, boxWidth, boxHeight, BoxThickness, borderColor);
				
				var ennemyHealthController = ennemy.HealthController;
				var ennemyHandController = ennemy.HandsController;
				if (ShowInfos && ennemyHealthController is {IsAlive: true})
					{
					var bodyPartHealth = ennemyHealthController.GetBodyPartHealth(EBodyPart.Common);
					var currentPlayerHealth = bodyPartHealth.Current;
					var maximumPlayerHealth = bodyPartHealth.Maximum;

					var weaponText = ennemyHandController != null && ennemyHandController.Item is Weapon weapon ? weapon.ShortName.Localized() : string.Empty;
					var infoText = $"{weaponText} {Mathf.Round(currentPlayerHealth * 100 / maximumPlayerHealth)}% [{distance}m]".Trim();

					Render.DrawString(new Vector2(boxPositionX, boxPositionY - 20f), infoText, infoColor, false);
					}

				if (ShowSkeletons)
					Bones.RenderBones(ennemy, SkeletonThickness, color, camera);
			}
		}

		private void GetPlayerColors(Player player, out Color color, out Color borderColor, out Color infoColor)
		{
			var info = player.Profile?.Info;
			if (info == null)
			{
				color = ScavColor;
				borderColor = ScavBorderColor;
				infoColor = ScavInfoColor;
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
						infoColor = ScavRaiderInfoColor;
						return;
					case WildSpawnType.sectantWarrior:
						color = CultistColor;
						borderColor = CultistBorderColor;
						infoColor = CultistInfoColor;
						return;
				}

				if (settings.IsBoss())
				{
					color = BossColor;
					borderColor = BossBorderColor;
					infoColor = BossInfoColor;
					return;
				}
			}

			// it can still be a bot in sptarkov but let's use the pmc color
			switch (info.Side)
			{
				case EPlayerSide.Bear:
					color = BearColor;
					borderColor = BearBorderColor;
					infoColor = BearInfoColor;
					break;
				case EPlayerSide.Usec:
					color = UsecColor;
					borderColor = UsecBorderColor;
					infoColor = UsecInfoColor;
					break;
				default:
					color = ScavColor;
					borderColor = ScavBorderColor;
					infoColor = ScavInfoColor;
					break;
			}
		}

		private void SetShaders(Player player, Shader? shader, Color color, Color borderColor, Dictionary<Renderer, Shader?> cache)
		{
			var playerBody = player.PlayerBody;
			if (playerBody == null)
				return;

			var skins = playerBody.BodySkins;
			if (skins == null)
				return;

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

					cache[renderer] = material.shader;
					material.shader = shader;

					material.SetColor("_FirstOutlineColor", borderColor);
					material.SetFloat("_FirstOutlineWidth", 0.02f);
					material.SetColor("_SecondOutlineColor", color);
					material.SetFloat("_SecondOutlineWidth", 0.0025f);
					material.SetFloat("_ZTest", (float) (XRayVision ? CompareFunction.Always : CompareFunction.Less));
				}
			}
		}

		private static void ResetShaders(Dictionary<Renderer, Shader?> cache)
		{
			var hits = 0;
			foreach (var key in cache.Keys)
			{
				var shader = cache[key];
				if (key.material == null)
					continue;

				if (key.material.shader == shader) 
					continue;

				key.material.shader = shader;
				hits++;
			}

			if (hits == 0 && cache.Count > 0)
				cache.Clear();
		}
	}
}
