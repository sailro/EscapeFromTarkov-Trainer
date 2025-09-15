﻿using System.Diagnostics.CodeAnalysis;
using Comfort.Common;
using EFT.Ballistics;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using EFT.Trainer.UI;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Aimbot : HoldFeature
{
	public override string Name => Strings.FeatureAimbotName;
	public override string Description => Strings.FeatureAimbotDescription;

	public override KeyCode Key { get; set; } = KeyCode.Slash;

	[ConfigurationProperty(Order = 10)]
	public float MaximumDistance { get; set; } = 200f;

	[ConfigurationProperty(Order = 11)]
	public float Smoothness { get; set; } = 0.085f;

	[ConfigurationProperty(Order = 20)]
	public float FovRadius { get; set; } = 0f;

	[ConfigurationProperty(Order = 21)]
	public bool ShowFovCircle { get; set; } = false;

	[ConfigurationProperty(Order = 22)]
	public Color FovCircleColor { get; set; } = Color.white;

	[ConfigurationProperty(Order = 23)]
	public float FovCircleThickness { get; set; } = 1f;

	[ConfigurationProperty(Order = 30)]
	public bool SilentAim { get; set; } = false;

	[ConfigurationProperty(Order = 31)]
	public float SilentAimNextShotDelay { get; set; } = 0.25f;

	[ConfigurationProperty(Order = 32)]
	public float SilentAimSpeedFactor { get; set; } = 100f;

#pragma warning disable IDE0060
	[UsedImplicitly]
	protected static bool CreateShotPrefix(object ammo, Vector3 origin, ref Vector3 direction, int fireIndex, string player, Item weapon, ref float speedFactor, int fragmentIndex)
	{
		var feature = FeatureFactory.GetFeature<Aimbot>();
		if (feature == null || !feature.SilentAim || feature._silentAimTarget == null)
			return true; // keep using original code, we are not enabled

		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return true;

		var localPlayer = world.GetEverExistedBridgeByProfileID(player)?.iPlayer;
		if (localPlayer == null)
			return true;

		if (!localPlayer.IsYourPlayer)
			return true; // keep using original code, not our player

		direction = (feature._silentAimTarget.position - origin).normalized;
		speedFactor = feature.SilentAimSpeedFactor;

		return true; // call the original code with updated direction and speedFactor
	}

	[UsedImplicitly]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	protected static bool ApplyShotPrefix(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, object shotId, Player? __instance)
	{
		var feature = FeatureFactory.GetFeature<Aimbot>();
		if (feature == null || !feature.SilentAim || feature._silentAimTarget == null)
			return true; // keep using original code, we are not enabled

		if (damageInfo.Player?.iPlayer is { IsYourPlayer: true } && __instance is { IsYourPlayer: true })
			return false; // prevent self damage !

		return true; // call the original code
	}

	[UsedImplicitly]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	protected static bool CreateOpticCalibrationDataPrefix(Weapon? __instance, ref AmmoTemplate ammoTemplate)
	{
		// Taken from SPT-BetterZeroing, credits to ehaugw
		// Use the loaded ammo for elevation adjustment calculations rather than a weapon's default ammo
		if (__instance?.GetCurrentMagazine() is { } mag && mag.FirstRealAmmo() is AmmoItemClass { AmmoTemplate: { } magazineTemplate })
		{
			ammoTemplate = magazineTemplate;
		}
		else if (__instance?.Chambers is { Length: > 0 } slots && slots[0]?.ContainedItem is AmmoItemClass { AmmoTemplate: { } chamberedTemplate })
		{
			ammoTemplate = chamberedTemplate;
		}

		return true; // call the original code with updated template
	}

#pragma warning restore IDE0060

	private Transform? _silentAimTarget = null;
	private float _silentAimNextShotTime = 0f;
	protected override void Update()
	{
		base.Update();

		if (!SilentAim)
			return;

		HarmonyPatchOnce(harmony =>
		{
			HarmonyPrefix(harmony, typeof(BallisticsCalculator), nameof(BallisticsCalculator.CreateShot), nameof(CreateShotPrefix));
			HarmonyPrefix(harmony, typeof(Player), nameof(Player.ApplyShot), nameof(ApplyShotPrefix));
			HarmonyPrefix(harmony, typeof(Weapon), nameof(Weapon.CreateOpticCalibrationData), nameof(CreateOpticCalibrationDataPrefix));
		});

		_silentAimTarget = null;

		if (!TryGetNearestTarget(out var player, out var camera, out var nearestTarget))
			return;

		if (player.IsInventoryOpened)
			return;

		if (player.HandsController is not Player.FirearmController controller)
			return;

		if (!camera.IsTransformVisible(nearestTarget))
			return;

		_silentAimTarget = nearestTarget;

		if (_silentAimNextShotTime > Time.time)
			return;

		controller.SetTriggerPressed(true);
		_silentAimNextShotTime = Time.time + SilentAimNextShotDelay;
		controller.SetTriggerPressed(false);
	}

	protected override void UpdateWhenHold()
	{
		if (!TryGetNearestTarget(out var player, out _, out var nearestTarget))
			return;

		AimAtPosition(player, nearestTarget.position, Smoothness);
	}

	private bool TryGetNearestTarget([NotNullWhen(true)] out Player? localPlayer, [NotNullWhen(true)] out Camera? camera, [NotNullWhen(true)] out Transform? nearestTarget)
	{
		localPlayer = null;
		camera = null;
		nearestTarget = null;
		var nearestTargetDistance = float.MaxValue;

		var state = GameState.Current;
		if (state == null)
			return false;

		camera = state.Camera;
		if (camera == null)
			return false;

		localPlayer = state.LocalPlayer;
		if (localPlayer == null)
			return false;

		if (localPlayer.HandsController == null || localPlayer.HandsController.Item is not Weapon weapon)
			return false;

		var template = weapon.CurrentAmmoTemplate;
		if (template == null)
			return false;

		foreach (var hostile in state.Hostiles)
		{
			if (hostile == null)
				continue;

			if (!hostile.IsAlive())
				continue;

			if (!TryGetHeadTransform(hostile, out var hostileTransform))
				continue;

			var destination = hostileTransform.position;
			var screenPosition = camera.WorldPointToVisibleScreenPoint(destination);
			if (screenPosition == Vector2.zero)
				continue;

			if (!IsInFieldOfView(screenPosition))
				continue;

			var distance = Vector3.Distance(camera.transform.position, destination);
			if (distance > MaximumDistance)
				continue;

			if (distance >= nearestTargetDistance)
				continue;

			nearestTargetDistance = distance;
			var travelTime = distance / template.InitialSpeed;
			destination.x += hostile.Velocity.x * travelTime;
			destination.y += hostile.Velocity.y * travelTime;

			nearestTarget = hostileTransform;
		}

		return nearestTarget != null;
	}

	[UsedImplicitly]
	protected void OnGUI()
	{
		if (!ShowFovCircle || FovRadius <= 0)
			return;

		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		if (player.HandsController == null || player.HandsController.Item is not Weapon)
			return;

		Render.DrawCircle(Render.ScreenCenter, FovRadius, FovCircleColor, FovCircleThickness, 48);
	}

	private bool IsInFieldOfView(Vector2 screenPosition)
	{
		if (FovRadius <= 0f)
			return true;

		var distance = Vector2.Distance(Render.ScreenCenter, screenPosition);
		return distance <= FovRadius;
	}

	private static void AimAtPosition(Player player, Vector3 targetPosition, float smoothness)
	{
		var firingAngle = player.Fireport.position - player.Fireport.up * 1f;
		var normalized = (targetPosition - firingAngle).normalized;
		var quaternion = Quaternion.LookRotation(normalized);
		var euler = quaternion.eulerAngles;

		//This is necessary due to crossing Y plane with target
		if (euler.x > 180f)
			euler.x -= 360f;

		var playerRotation = player.MovementContext.Rotation;
		var smoothAngle = GetSmoothAngle(playerRotation, new Vector2(euler.y, euler.x), smoothness);
		player.MovementContext.Rotation = smoothAngle;
	}

	private static Vector2 GetSmoothAngle(Vector2 fromAngle, Vector2 toAngle, float smoothness)
	{
		var delta = fromAngle - toAngle;
		NormalizeAngle(ref delta);
		var smoothedDelta = Vector2.Scale(delta, new Vector2(smoothness, smoothness));
		toAngle = fromAngle - smoothedDelta;
		return toAngle;
	}

	private static void NormalizeAngle(ref Vector2 angle)
	{
		var newX = angle.x switch
		{
			<= -180f => angle.x + 360f,
			> 180f => angle.x - 360f,
			_ => angle.x
		};

		var newY = angle.y switch
		{
			> 90f => angle.y - 180f,
			<= -90f => angle.y + 180f,
			_ => angle.y
		};

		angle = new Vector2(newX, newY);
	}

	private static bool TryGetHeadTransform(Player player, [NotNullWhen(true)] out Transform? transform)
	{
		transform = null;

		var bones = player.PlayerBones;
		if (bones == null)
			return false;

		transform = bones.Head.Original;
		return true;
	}
}
