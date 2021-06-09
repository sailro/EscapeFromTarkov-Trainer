using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class Aimbot : HoldMonoBehaviour
	{
		public override KeyCode Key { get; set; } = KeyCode.Slash;

		[ConfigurationProperty]
		public float MaximumDistance { get; set; } = 200f;

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

				var distance = Vector3.Distance(camera.transform.position, player.Transform.position);
				if (distance > MaximumDistance)
					continue;

				if (distance >= nearestTargetDistance)
					continue;

				var template = localPlayer.Weapon?.CurrentAmmoTemplate;
				if (template == null)
					continue;

				nearestTargetDistance = distance;
				var travelTime = distance / template.InitialSpeed;
				destination.x += (player.Velocity.x * travelTime);
				destination.y += (player.Velocity.y * travelTime);

				nearestTarget = destination;
			}

			if (nearestTarget != Vector3.zero)
				AimAtPosition(localPlayer, nearestTarget);
		}

		private static void AimAtPosition(Player player, Vector3 position)
		{
			var delta = player.Fireport.position - player.Fireport.up * 1f;
			var eulerAngles = Quaternion.LookRotation((position - delta).normalized).eulerAngles;

			if (eulerAngles.x > 180f)
				eulerAngles.x -= 360f;

			player.MovementContext.Rotation = new Vector2(eulerAngles.y, eulerAngles.x);
		}

		public static Vector3 GetHeadPosition(Player player)
		{
			var bones = player.PlayerBones;
			if (bones == null)
				return Vector3.zero;

			var head = bones.Head;
			return head?.position ?? Vector3.zero;
		}
	}
}
