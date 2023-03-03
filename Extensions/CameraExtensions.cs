using UnityEngine;
using EFT.CameraControl;
using EFT.Trainer.Features;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class CameraExtensions
	{
		public static Vector3 WorldPointToScreenPoint(this Camera camera, Vector3 worldPoint)
		{
			var screenPoint = camera.WorldToScreenPoint(worldPoint);
			var scale = Screen.height / (float)camera.scaledPixelHeight;
			screenPoint.y = Screen.height - screenPoint.y * scale;
			screenPoint.x *= scale;
			return screenPoint;
		}

		public static Vector3 ScopePointToScreenPoint(this Camera camera, Vector3 worldPoint)
		{
			var screenPoint = camera.WorldToScreenPoint(worldPoint);
			var scale = Screen.height / (float)camera.scaledPixelHeight;
			screenPoint.y = Screen.height - screenPoint.y * scale;
			screenPoint.x *= scale;

			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return screenPoint;

			var currentOptic = player.ProceduralWeaponAnimation.HandsContainer.Weapon.GetComponentInChildren<OpticSight>();
			if (currentOptic == null)
				return screenPoint;

			foreach (var opticCamera in Camera.allCameras)
			{
				if (opticCamera.name == "BaseOpticCamera(Clone)")
				{
					var scopePoint = opticCamera.WorldPointToScreenPoint(worldPoint);

					scopePoint.x += camera.pixelWidth / 2 - opticCamera.pixelWidth / 2;
					scopePoint.y += camera.pixelHeight / 2 - opticCamera.pixelHeight / 2;

					//if (!camera.CheckScopeProjection(scopePoint, currentOptic))
					//{
					//	return Vector3.zero;
					//}
					return scopePoint;
				}
			}
			return screenPoint;
		}

		private static readonly LayerMask _layerMask = 1 << 12 | 1 << 16 | 1 << 18 | 1 << 31 | 1 << 22;
		public static bool IsTransformVisible(this Camera camera, Transform transform)
		{
			var origin = camera.transform.position;
			var destination = transform.position;

			if (!Physics.Linecast(origin, destination, out var hitinfo, _layerMask))
				return false;

			return hitinfo.transform == transform;
		}

#pragma warning disable IDE0060 // Remove unused parameter
		public static bool IsScreenPointVisible(this Camera camera, Vector3 screenPoint)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			return screenPoint is { z: > 0.01f, x: > -5f, y: > -5f } && screenPoint.x < Screen.width && screenPoint.y < Screen.height;
		}

		public static bool CheckScopeProjection(this Camera camera, Vector2 target, OpticSight currentOptic)
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
