using System;
using System.Collections.Generic;
using System.Linq;
using EFT.CameraControl;
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

		private static Camera? _opticCamera;

		[UsedImplicitly]
		protected void OnGUI()
		{
			var snapshot = GameState.Current;
			if (snapshot == null)
				return;

			if (snapshot.MapMode)
				return;

			var hostiles = snapshot.Hostiles;

			var player = snapshot.LocalPlayer;
			if (player == null)
				return;

			var camera = snapshot.Camera;
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

			if (isAiming && zoom <= 1)
				isAiming = false;

			if (_opticCamera == null)
			{
				foreach (var opticCamera in Camera.allCameras)
				{
					if (opticCamera.name == "BaseOpticCamera(Clone)")
					{
						_opticCamera = opticCamera;
					}
				}
			}
			
			foreach (var ennemy in hostiles)
			{
				if (!ennemy.IsValid())
					continue;

				var playerColors = GetPlayerColors(ennemy);
				var borderColor = playerColors.BorderColor;

				if (ShowCharms)
					SetShaders(ennemy, GameState.OutlineShader, playerColors.Color, borderColor, cache);

				var position = ennemy.Transform.position;
				var screenPosition = isAiming ? ScopePointToScreenPoint(camera, position) : camera.WorldPointToScreenPoint(position);
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
					var bonesToCheck = GetBonesToCheck(playerBones);
					borderColor = bonesToCheck.Any(bone => IsTransformVisibleCached(bone.transform, camera.IsTransformVisible))
						? ShootableColors.BorderColor
						: NotShootableColors.BorderColor;

					if (ShowSkeletons)
					{
						foreach (var bone in bonesToCheck)
						{
							var bonesColor = IsTransformVisibleCached(bone.transform, camera.IsTransformVisible) ? ShootableColors.Color : NotShootableColors.Color;
							Bones.RenderBones(ennemy, bone.bones, SkeletonThickness, bonesColor, camera, isAiming);
						}

						var color = IsTransformVisibleCached(bonesToCheck[0].transform, camera.IsTransformVisible) ? ShootableColors.Color : NotShootableColors.Color;
						Bones.RenderHead(ennemy, SkeletonThickness, color, camera, isAiming);
						Bones.RenderFingers(ennemy, SkeletonThickness, color, camera, isAiming);
					}

					ClearTransformCache();
				}
				else if (ShowSkeletons)
					Bones.RenderBones(ennemy, SkeletonThickness, playerColors.Color, camera, isAiming);

				var headScreenPosition = isAiming
					? Players.ScopePointToScreenPoint(camera, playerBones.Head.position)
					: camera.WorldPointToScreenPoint(playerBones.Head.position);
				var leftShoulderScreenPosition = isAiming
					? Players.ScopePointToScreenPoint(camera, playerBones.LeftShoulder.position)
					: camera.WorldPointToScreenPoint(playerBones.LeftShoulder.position);

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
			}
		}

		private static (Transform transform, string[] bones)[] GetBonesToCheck(PlayerBones playerBones)
		{
			return new []
			{
				(playerBones.Head.Original.transform, new [] {Bones.Neck, Bones.Head}),
				(playerBones.Neck.transform, new [] {Bones.RCollarbone, Bones.Spine3, Bones.LCollarbone, Bones.Spine3, Bones.Spine3, Bones.Neck}),
				(playerBones.Spine1.transform, new [] {Bones.Pelvis, Bones.Spine1, Bones.Spine1, Bones.Spine2, Bones.Spine2, Bones.Spine3}),
				(playerBones.Upperarms[0].transform, new [] {Bones.LCollarbone, Bones.LForearm1, Bones.LForearm1, Bones.LForearm2}),
				(playerBones.Upperarms[1].transform, new [] {Bones.RCollarbone, Bones.RForearm1, Bones.RForearm1, Bones.RForearm2}),
				(playerBones.Forearms[0].transform, new [] {Bones.LForearm2, Bones.LForearm3, Bones.LForearm3, Bones.LPalm}),
				(playerBones.Forearms[1].transform, new [] {Bones.RForearm2, Bones.RForearm3, Bones.RForearm3, Bones.RPalm}),
				(playerBones.LeftThigh1.Original.transform, new [] {Bones.Pelvis, Bones.LThigh1, Bones.LThigh1, Bones.LThigh2}),
				(playerBones.RightThigh1.Original.transform, new [] {Bones.Pelvis, Bones.RThigh1, Bones.RThigh1, Bones.RThigh2}),
				(playerBones.LeftThigh2.Original.transform, new [] {Bones.LThigh2, Bones.LCalf, Bones.LCalf, Bones.LFoot, Bones.LFoot, Bones.LToe}),
				(playerBones.RightThigh2.Original.transform, new [] {Bones.RThigh2, Bones.RCalf, Bones.RCalf, Bones.RFoot, Bones.RFoot, Bones.RToe})
			};
		}

		private readonly Dictionary<Transform, bool> _cache = new Dictionary<Transform, bool>();

		private bool IsTransformVisibleCached(Transform transform, Func<Transform, bool> isVisibleFunc)
		{
			if (_cache.TryGetValue(transform, out bool isVisible))
			{
				return isVisible;
			}

			isVisible = isVisibleFunc(transform);
			_cache[transform] = isVisible;
			return isVisible;
		}

		private void ClearTransformCache()
		{
			_cache.Clear();
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

		public static Vector3 ScopePointToScreenPoint(Camera camera, Vector3 worldPoint, bool OnlyShowInScope = false)
		{
			var screenPoint = camera.WorldPointToScreenPoint(worldPoint);

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return screenPoint;

			var currentOptic = player.ProceduralWeaponAnimation.HandsContainer.Weapon.GetComponentInChildren<OpticSight>();
			if (currentOptic == null)
				return screenPoint;

			if (_opticCamera == null)
				return screenPoint;

			var scale = Screen.height / (float)camera.scaledPixelHeight;
			var point = _opticCamera.WorldToViewportPoint(worldPoint);
			var scopePoint = _opticCamera.ViewportToScreenPoint(point);

			scopePoint.x += camera.pixelWidth / 2 - _opticCamera.pixelWidth / 2;
			scopePoint.y += camera.pixelHeight / 2 - _opticCamera.pixelHeight / 2;

			scopePoint.y = Screen.height - scopePoint.y * scale;
			scopePoint.x *= scale;

			if (!CheckScopeProjection(camera, scopePoint, currentOptic))
				return Vector3.zero;

			return scopePoint;
		}

		public static (Vector3, Vector3) ScopePointToScreenPoint(Camera camera, Vector3 worldPoint1, Vector3 worldPoint2, bool OnlyShowInScope = false)
		{
			var screenPoint1 = camera.WorldPointToScreenPoint(worldPoint1);
			var screenPoint2 = camera.WorldPointToScreenPoint(worldPoint2);

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return (screenPoint1, screenPoint2);

			var currentOptic = player.ProceduralWeaponAnimation.HandsContainer.Weapon.GetComponentInChildren<OpticSight>();
			if (currentOptic == null)
				return (screenPoint1, screenPoint2);

			if (_opticCamera == null)
				return (screenPoint1, screenPoint2);

			var lensMesh = currentOptic.LensRenderer.GetComponent<MeshFilter>().mesh;
			if (lensMesh == null)
				return (screenPoint1, screenPoint2);

			var lensUpperRight = currentOptic.LensRenderer.transform.TransformPoint(lensMesh.bounds.max);
			var lensUpperLeft = currentOptic.LensRenderer.transform.TransformPoint(new Vector3(lensMesh.bounds.min.x, 0, lensMesh.bounds.max.z));

			var lensUpperRight_3D = camera.WorldPointToScreenPoint(lensUpperRight);
			var lensUpperLeft_3D = camera.WorldPointToScreenPoint(lensUpperLeft);
			var scopeRadius = Vector3.Distance(lensUpperRight_3D, lensUpperLeft_3D) / 2;
			var scopePos = camera.WorldPointToScreenPoint(currentOptic.LensRenderer.transform.position);
			var scopeCenter = new Vector2(scopePos.x, scopePos.y);

			var scale = Screen.height / (float)camera.scaledPixelHeight;
			var cameraOffset = new Vector2(
				camera.pixelWidth / 2 - _opticCamera.pixelWidth / 2,
				camera.pixelHeight / 2 - _opticCamera.pixelHeight / 2);

			var point1 = _opticCamera.WorldToViewportPoint(worldPoint1);
			var scopePoint1 = (Vector2)_opticCamera.ViewportToScreenPoint(point1) + cameraOffset;
			scopePoint1.y = Screen.height - scopePoint1.y * scale;
			scopePoint1.x *= scale;

			var point2 = _opticCamera.WorldToViewportPoint(worldPoint2);
			var scopePoint2 = (Vector2)_opticCamera.ViewportToScreenPoint(point2) + cameraOffset;
			scopePoint2.y = Screen.height - scopePoint2.y * scale;
			scopePoint2.x *= scale;

			var distance1 = Vector2.Distance(scopeCenter, scopePoint1);
			var distance2 = Vector2.Distance(scopeCenter, scopePoint2);

			var clampedTarget1 = scopePoint1;
			var clampedTarget2 = scopePoint2;

			if (distance1 > scopeRadius && distance2 > scopeRadius)
				return (Vector2.zero, Vector2.zero);

			if (distance1 > scopeRadius)
			{
				var clampedVector = (scopePoint1 - scopeCenter).normalized * scopeRadius;
				clampedTarget1 = scopeCenter + new Vector2(clampedVector.x, clampedVector.y);
			}

			if (distance2 > scopeRadius)
			{
				var clampedVector = (scopePoint2 - scopeCenter).normalized * scopeRadius;
				clampedTarget2 = scopeCenter + new Vector2(clampedVector.x, clampedVector.y);
			}

			return (clampedTarget1, clampedTarget2);
		}

		private static bool CheckScopeProjection(Camera camera, Vector2 target, OpticSight currentOptic)
		{
			var lensMesh = currentOptic.LensRenderer.GetComponent<MeshFilter>().mesh;
			if (lensMesh == null)
				return false;
		
			var lensUpperRight = currentOptic.LensRenderer.transform.TransformPoint(lensMesh.bounds.max);
			var lensUpperLeft = currentOptic.LensRenderer.transform.TransformPoint(new Vector3(lensMesh.bounds.min.x, 0, lensMesh.bounds.max.z));
		
			var lensUpperRight_3D = camera.WorldPointToScreenPoint(lensUpperRight);
			var lensUpperLeft_3D = camera.WorldPointToScreenPoint(lensUpperLeft);
			var scopeRadius = Vector3.Distance(lensUpperRight_3D, lensUpperLeft_3D) / 2;
			var scopePos = camera.WorldPointToScreenPoint(currentOptic.LensRenderer.transform.position);
		
			var distance = Vector2.Distance(new Vector2(scopePos.x, scopePos.y), target);
			return distance <= scopeRadius;
		}
	}
}
