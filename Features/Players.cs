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

namespace EFT.Trainer.Features;

public class PlayerColor(Color color, Color borderColor, Color infoColor) : IFeature
{
	[ConfigurationProperty(Order = 1)]
	public Color Color { get; set; } = color;

	[ConfigurationProperty(Order = 2)]
	public Color BorderColor { get; set; } = borderColor;

	[ConfigurationProperty(Order = 3)]
	public Color InfoColor { get; set; } = infoColor;

	public string Name => nameof(PlayerColor);
}

public class ShootableColor(Color color, Color borderColor) : IFeature
{
	[ConfigurationProperty(Order = 1)]
	public Color Color { get; set; } = color;

	[ConfigurationProperty(Order = 2)]
	public Color BorderColor { get; set; } = borderColor;

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
	public bool ShowNotShootable { get; set; } = false;

	[ConfigurationProperty(Order = 63)]
	public ShootableColor NotShootableColors { get; set; } = new(Color.red, Color.blue);

	[ConfigurationProperty(Order = 19)]
	public float MaximumDistance { get; set; } = 0f;

	private static bool _lastXRayVision = true;
	private static bool _lastShowCharms = true;

	private static Camera? _opticCamera;
	private static (Vector2 center, float radius) _scopeParameters;

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

		var isAiming = AimingCheck(camera, player);
			
		foreach (var ennemy in hostiles)
		{
			if (!ennemy.IsValid())
				continue;

			var playerColors = GetPlayerColors(ennemy);
			var borderColor = playerColors.BorderColor;

			if (ShowCharms)
				SetShaders(ennemy, GameState.OutlineShader, playerColors.Color, borderColor, cache);

			var position = ennemy.Transform.position;
			var screenPosition = isAiming ? ScopePointToScreenPoint(camera, position) : camera.WorldPointToVisibleScreenPoint(position);
			if (screenPosition == Vector2.zero)
				continue;

			var distance = Mathf.Round(Vector3.Distance(camera.transform.position, position));
			if (MaximumDistance > 0 && distance > MaximumDistance)
				continue;

			var playerBones = ennemy.PlayerBones;
			if (playerBones == null)
				continue;

			var headScreenPosition = isAiming
				? ScopePointToScreenPoint(camera, playerBones.Head.position)
				: camera.WorldPointToVisibleScreenPoint(playerBones.Head.position);
			var leftShoulderScreenPosition = isAiming
				? ScopePointToScreenPoint(camera, playerBones.LeftShoulder.position)
				: camera.WorldPointToVisibleScreenPoint(playerBones.LeftShoulder.position);

			if (headScreenPosition == Vector2.zero || leftShoulderScreenPosition == Vector2.zero)
				continue;

			if (ShowShootable)
			{
				var bonesToCheck = GetBonesToCheck(playerBones);
				borderColor = bonesToCheck.Any(bone => IsTransformVisibleCached(bone.transform, camera.IsTransformVisible))
					? ShootableColors.BorderColor
					: ShowNotShootable ? NotShootableColors.BorderColor : playerColors.BorderColor;

				if (ShowSkeletons)
				{
					foreach (var bone in bonesToCheck)
					{
						var bonesColor = IsTransformVisibleCached(bone.transform, camera.IsTransformVisible) ? ShootableColors.Color : ShowNotShootable ? NotShootableColors.Color : playerColors.Color;
						Bones.RenderBones(ennemy, bone.bones, SkeletonThickness, bonesColor, camera, isAiming);
					}

					var color = IsTransformVisibleCached(bonesToCheck[0].transform, camera.IsTransformVisible) ? ShootableColors.Color : ShowNotShootable ? NotShootableColors.Color : playerColors.Color;
					Bones.RenderHead(ennemy, SkeletonThickness, color, camera, isAiming);
					if (distance < 75f)
						Bones.RenderFingers(ennemy, SkeletonThickness, color, camera, isAiming);
				}

				ClearTransformCache();
			}
			else if (ShowSkeletons)
				Bones.RenderBones(ennemy, SkeletonThickness, playerColors.Color, camera, isAiming, distance);

			var heightOffset = Mathf.Abs(headScreenPosition.y - leftShoulderScreenPosition.y);

			var boxHeight = Mathf.Abs(headScreenPosition.y - screenPosition.y) + heightOffset * 3f;
			var boxWidth = boxHeight * 0.62f;

			var boxPositionX = screenPosition.x - boxWidth / 2f;
			var boxPositionY = headScreenPosition.y - heightOffset * 2f;

			if (ShowBoxes)
				Render.DrawBox(boxPositionX, boxPositionY, boxWidth, boxHeight, BoxThickness, borderColor);
				
			var ennemyHealthController = ennemy.HealthController;
			var ennemyHandController = ennemy.HandsController;

			if (!ShowInfos || ennemyHealthController is not { IsAlive: true })
				continue;

			var bodyPartHealth = ennemyHealthController.GetBodyPartHealth(EBodyPart.Common);
			var currentPlayerHealth = bodyPartHealth.Current;
			var maximumPlayerHealth = bodyPartHealth.Maximum;

			var weaponText = ennemyHandController != null && ennemyHandController.Item is Weapon weapon ? weapon.ShortName.Localized() : string.Empty;
			var infoText = $"{weaponText} {Mathf.Round(currentPlayerHealth * 100 / maximumPlayerHealth)}% [{distance}m]".Trim();

			Render.DrawString(new Vector2(boxPositionX, boxPositionY - 20f), infoText, playerColors.InfoColor, false);
		}
	}

	private static (Transform transform, string[] bones)[] GetBonesToCheck(PlayerBones playerBones)
	{
		return
		[
			(playerBones.Head.Original.transform, [Bones.Neck, Bones.Head]),
			(playerBones.Neck.transform, [Bones.RCollarbone, Bones.Spine3, Bones.LCollarbone, Bones.Spine3, Bones.Spine3, Bones.Neck]),
			(playerBones.Spine1.transform, [Bones.Pelvis, Bones.Spine1, Bones.Spine1, Bones.Spine2, Bones.Spine2, Bones.Spine3]),
			(playerBones.Upperarms[0].transform, [Bones.LCollarbone, Bones.LForearm1, Bones.LForearm1, Bones.LForearm2]),
			(playerBones.Upperarms[1].transform, [Bones.RCollarbone, Bones.RForearm1, Bones.RForearm1, Bones.RForearm2]),
			(playerBones.Forearms[0].transform, [Bones.LForearm2, Bones.LForearm3, Bones.LForearm3, Bones.LPalm]),
			(playerBones.Forearms[1].transform, [Bones.RForearm2, Bones.RForearm3, Bones.RForearm3, Bones.RPalm]),
			(playerBones.LeftThigh1.Original.transform, [Bones.Pelvis, Bones.LThigh1, Bones.LThigh1, Bones.LThigh2]),
			(playerBones.RightThigh1.Original.transform, [Bones.Pelvis, Bones.RThigh1, Bones.RThigh1, Bones.RThigh2]),
			(playerBones.LeftThigh2.Original.transform, [Bones.LThigh2, Bones.LCalf, Bones.LCalf, Bones.LFoot, Bones.LFoot, Bones.LToe]),
			(playerBones.RightThigh2.Original.transform, [Bones.RThigh2, Bones.RCalf, Bones.RCalf, Bones.RFoot, Bones.RFoot, Bones.RToe])
		];
	}

	private readonly Dictionary<Transform, bool> _cache = [];

	private bool IsTransformVisibleCached(Transform value, Func<Transform, bool> isVisibleFunc)
	{
		if (_cache.TryGetValue(value, out bool isVisible))
		{
			return isVisible;
		}

		isVisible = isVisibleFunc(value);
		_cache[value] = isVisible;
		return isVisible;
	}

	private void ClearTransformCache()
	{
		_cache.Clear();
	}

	private static bool AimingCheck(Camera camera, Player player)
	{
		var handsController = player.HandsController;
		if (handsController == null)
			return false;

		var weaponAnimation = player.ProceduralWeaponAnimation;
		if (weaponAnimation == null)
			return false;

		var aimingMod = weaponAnimation.CurrentAimingMod;
		if (aimingMod == null)
			return false;

		if (aimingMod.ScopesCount <= 0)
			return false;

		var zoom = aimingMod.GetCurrentOpticZoom();
		var isAiming = handsController.IsAiming;

		if (isAiming && zoom <= 1)
			isAiming = false;

		var currentOptic = weaponAnimation.HandsContainer.Weapon.GetComponentInChildren<OpticSight>();
		if (isAiming && currentOptic != null)
			GetScopeParameters(camera, currentOptic);

		if (_opticCamera != null)
			return isAiming;

		_opticCamera = Camera.allCameras.FirstOrDefault(c => c.name == "BaseOpticCamera(Clone)");

		return isAiming;
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

				// disable custom occlusion/culling system, making the chams flickering or not rendering at all
				renderer.allowOcclusionWhenDynamic = false;
				renderer.forceRenderingOff = false;
				renderer.enabled = true;

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
		foreach (var renderer in cache.Keys)
		{
			if (renderer == null)
				continue;

			if (renderer.material == null)
				continue;

			var shader = cache[renderer];
			if (renderer.material.shader == shader) 
				continue;

			renderer.material.shader = shader;
			hits++;
		}

		if (hits == 0 && cache.Count > 0)
			cache.Clear();
	}

	public static Vector2 ScopePointToScreenPoint(Camera camera, Vector3 worldPoint, bool clamp = false)
	{
		if (_opticCamera == null || !GetCameraOffset(camera, out var scale, out var cameraOffset))
			return camera.WorldPointToScreenPoint(worldPoint);

		var scopePoint = (Vector2)_opticCamera.WorldToScreenPoint(worldPoint) + cameraOffset;
		scopePoint.y = Screen.height - scopePoint.y * scale;
		scopePoint.x *= scale;

		if (clamp)
			return ClampPointToScope(scopePoint);

		var distance = Vector2.Distance(_scopeParameters.center, scopePoint);
		if (distance <= _scopeParameters.radius)
			return scopePoint;
			
		return Vector2.zero;
	}

	private static bool GetCameraOffset(Camera camera, out float scale, out Vector2 cameraOffset)
	{
		scale = 0f;
		cameraOffset = Vector2.zero;

		if (_opticCamera == null)
			return false;

		scale = Screen.height / (float)camera.scaledPixelHeight;
		cameraOffset = new Vector2(
			camera.pixelWidth / 2 - _opticCamera.pixelWidth / 2,
			camera.pixelHeight / 2 - _opticCamera.pixelHeight / 2);

		return true;
	}
	private static Vector2 ClampPointToScope(Vector2 scopePoint)
	{
		var distance = Vector2.Distance(_scopeParameters.center, scopePoint);

		var clampedPoint = scopePoint;

		if (distance > _scopeParameters.radius)
		{
			var clampedVector = (scopePoint - _scopeParameters.center).normalized * _scopeParameters.radius;
			clampedPoint = _scopeParameters.center + clampedVector;
		}

		return clampedPoint;
	}

	private static void GetScopeParameters(Camera camera, OpticSight currentOptic)
	{
		var opticTransform = currentOptic.LensRenderer.transform;
		var lensMesh = currentOptic.LensRenderer.GetComponent<MeshFilter>().mesh;
		var lensUpperRight = opticTransform.TransformPoint(lensMesh.bounds.max);
		var lensUpperLeft = opticTransform.TransformPoint(new Vector3(lensMesh.bounds.min.x, 0, lensMesh.bounds.max.z));

		var lensUpperRight3D = camera.WorldPointToScreenPoint(lensUpperRight);
		var lensUpperLeft3D = camera.WorldPointToScreenPoint(lensUpperLeft);
		_scopeParameters.radius = Vector2.Distance(lensUpperRight3D, lensUpperLeft3D) / 2;
		_scopeParameters.center = camera.WorldPointToScreenPoint(opticTransform.position);
	}
}
