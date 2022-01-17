using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Aimbot : HoldFeature
	{
		public override string Name => "aimbot";

		public override KeyCode Key { get; set; } = KeyCode.Slash;

		[ConfigurationProperty(Order = 20)]
		public float FovRadius { get; set; } = 0f;

		[ConfigurationProperty(Order = 21)]
		public bool ShowFovCircle { get; set; } = false;

		[ConfigurationProperty(Order = 22)]
		public Color FovCircleColor { get; set; } = Color.white;

		[ConfigurationProperty(Order = 23)]
		public float FovCircleThickness { get; set; } = 1f;

		[ConfigurationProperty]
		public float MaximumDistance { get; set; } = 200f;

		[ConfigurationProperty]
		public float Smoothness { get; set; } = 0.085f;

		protected override void UpdateWhenHold()
		{
			var state = GameState.Current;
			if (state == null)
				return;

			var camera = state.Camera;
			if (camera == null)
				return;

			var localPlayer = state.LocalPlayer;
			if (localPlayer == null)
				return;

			var nearestTarget = Vector3.zero;
			var nearestTargetDistance = float.MaxValue;

			foreach (var player in state.Hostiles)
			{
				if (player == null)
					continue;

				var destination = GetHeadPosition(player);
				if (destination == Vector3.zero)
					continue;

				var screenPosition = camera.WorldPointToScreenPoint(destination);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				if (!IsInFieldOfView(screenPosition))
					continue;

				var distance = Vector3.Distance(camera.transform.position, player.Transform.position);
				if (distance > MaximumDistance)
					continue;

				if (distance >= nearestTargetDistance)
					continue;

				if (localPlayer.HandsController == null || localPlayer.HandsController.Item is not Weapon weapon)
					continue;

				var template = weapon.CurrentAmmoTemplate;
				if (template == null)
					continue;

				nearestTargetDistance = distance;
				var travelTime = distance / template.InitialSpeed;
				destination.x += player.Velocity.x * travelTime;
				destination.y += player.Velocity.y * travelTime;

				nearestTarget = destination;
			}

			if (nearestTarget != Vector3.zero)
				AimAtPosition(localPlayer, nearestTarget, Smoothness);
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

		private bool IsInFieldOfView(Vector3 screenPosition)
		{
			if (FovRadius <= 0f)
				return true;

			var distance = Vector2.Distance(Render.ScreenCenter, new Vector2(screenPosition.x, screenPosition.y));
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

		private static Vector3 GetHeadPosition(Player player)
		{
			var bones = player.PlayerBones;
			if (bones == null)
				return Vector3.zero;

			var head = bones.Head;
			return head?.position ?? Vector3.zero;
		}
	}
}
