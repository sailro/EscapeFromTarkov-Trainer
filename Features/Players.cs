using System.Collections.Generic;
using System.Linq;
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
	public class PlayerColor : IFeature
	{
		public PlayerColor(Color color, Color borderColor, Color infoColor)
		{
			Color = color;
			BorderColor = borderColor;
			InfoColor = infoColor;
		}

		[ConfigurationProperty(Order = 1)]
		public Color Color { get; set; }

		[ConfigurationProperty(Order = 2)]
		public Color BorderColor { get; set; }

		[ConfigurationProperty(Order = 3)]
		public Color InfoColor { get; set; }

		public string Name => nameof(PlayerColor);
	}

	public class ShootableColor : IFeature
	{
		public ShootableColor(Color color, Color borderColor)
		{
			Color = color;
			BorderColor = borderColor;
		}

		[ConfigurationProperty(Order = 1)]
		public Color Color { get; set; }

		[ConfigurationProperty(Order = 2)]
		public Color BorderColor { get; set; }

		public string Name => nameof(ShootableColor);
	}

	[UsedImplicitly]
	internal class Players : ToggleFeature
	{
		public override string Name => "wallhack";

		[ConfigurationProperty(Order = 10)]
		public PlayerColor BearColors { get; set; } = new(Color.blue, Color.red, Color.red);

		[ConfigurationProperty(Order = 10)]
		public PlayerColor UsecColors { get; set; } = new(Color.green, Color.red, Color.red);

		[ConfigurationProperty(Order = 10)]
		public PlayerColor ScavColors { get; set; } = new(Color.yellow, Color.red, Color.red);

		[ConfigurationProperty(Order = 10)]
		public PlayerColor BossColors { get; set; } = new(Color.red, Color.red, Color.red);

		[ConfigurationProperty(Order = 10)]
		public PlayerColor CultistColors { get; set; } = new(Color.yellow, Color.red, Color.red);

		[ConfigurationProperty(Order = 10)]
		public PlayerColor ScavRaiderColors { get; set; } = new(Color.yellow, Color.red, Color.red);

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
		public ShootableColor ShootableColors { get; set; } = new(Color.green, Color.red);

		[ConfigurationProperty(Order = 62)]
		public ShootableColor NotShootableColors { get; set; } = new(Color.red, Color.blue);

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
			var handsController = player.HandsController;
			if (handsController == null)
				return;

			var weaponAnimation = player.ProceduralWeaponAnimation;
			if (weaponAnimation == null)
				return;

			var aimingMod = weaponAnimation.CurrentAimingMod;
			if (aimingMod == null)
				return;

			var zoom = aimingMod.GetCurrentOpticZoom();
			var isAiming = handsController.IsAiming;

			foreach (var ennemy in hostiles)
			{
				if (!ennemy.IsValid())
					continue;

				var playerColors = GetPlayerColors(ennemy);
				var borderColor = playerColors.BorderColor;

				if (ShowCharms)
					SetShaders(ennemy, GameState.OutlineShader, playerColors.Color, borderColor, cache);

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

				var shootable = false;
				if (ShowShootable)
				{
					var bonesToCheck = GetBonesToCheck(playerBones);
					borderColor = NotShootableColors.BorderColor;
					if (bonesToCheck.Any(bone => camera.IsTransformVisible(bone)))
					{
						borderColor = ShootableColors.BorderColor;
						shootable = true;
					}
				}

				var headScreenPosition = camera.WorldPointToScreenPoint(playerBones.Head.position);
				var leftShoulderScreenPosition = camera.WorldPointToScreenPoint(playerBones.LeftShoulder.position);

				if (isAiming && zoom > 1)
				{
					headScreenPosition = camera.ScopePointToScreenPoint(playerBones.Head.position);
					leftShoulderScreenPosition = camera.ScopePointToScreenPoint(playerBones.LeftShoulder.position);
					screenPosition = camera.ScopePointToScreenPoint(position);
				}

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

					Render.DrawString(new Vector2(boxPositionX, boxPositionY - 20f), infoText, playerColors.InfoColor, false);
				}

				if (!ShowSkeletons) 
					continue;

				if (ShowShootable && shootable)
					Bones.RenderBones(ennemy, SkeletonThickness, ShootableColors.Color, NotShootableColors.Color, camera, isAiming);
				else
					Bones.RenderBones(ennemy, SkeletonThickness, playerColors.Color, camera, isAiming);
			}
		}

		private static Transform[] GetBonesToCheck(PlayerBones playerBones)
		{
			return new []
			{
				playerBones.Head.Original.transform,
				playerBones.Neck.transform,
				playerBones.Shoulders[0].transform,
				playerBones.Shoulders[1].transform,
				playerBones.Spine1.transform,
				playerBones.Upperarms[0].transform,
				playerBones.Upperarms[1].transform,
				playerBones.Forearms[0].transform,
				playerBones.Forearms[1].transform,
				playerBones.LeftThigh1.Original.transform,
				playerBones.RightThigh1.Original.transform,
				playerBones.LeftThigh2.Original.transform,
				playerBones.RightThigh2.Original.transform
			};
		}

		public PlayerColor GetPlayerColors(Player player)
		{
			var info = player.Profile?.Info;
			if (info == null)
				return ScavColors;

			var settings = info.Settings;
			if (settings != null)
			{
				switch(settings.Role)
				{
					case WildSpawnType.pmcBot:
						return ScavRaiderColors;
					case WildSpawnType.sectantWarrior:
						return CultistColors;
				}

				if (settings.IsBoss())
					return BossColors;
			}

			// it can still be a bot in sptarkov but let's use the pmc color
			return info.Side switch
			{
				EPlayerSide.Bear => BearColors,
				EPlayerSide.Usec => UsecColors,
				_ => ScavColors
			};
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
