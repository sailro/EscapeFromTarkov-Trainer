using UnityEngine;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class CameraExtensions
	{
		public static Vector2 WorldPointToScreenPoint(this Camera camera, Vector3 worldPoint)
		{
			var screenPoint = camera.WorldToScreenPoint(worldPoint);
			var scale = Screen.height / (float)camera.scaledPixelHeight;
			screenPoint.y = Screen.height - screenPoint.y * scale;
			screenPoint.x *= scale;
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

		public static Vector2 WorldPointToVisibleScreenPoint(this Camera camera, Vector3 worldPoint)
		{
			var screenPoint = camera.WorldToScreenPoint(worldPoint);
			var scale = Screen.height / (float)camera.scaledPixelHeight;
			screenPoint.y = Screen.height - screenPoint.y * scale;
			screenPoint.x *= scale;
			if (screenPoint is { z: > 0.01f, x: > -5f, y: > -5f } && screenPoint.x < Screen.width && screenPoint.y < Screen.height)
				return screenPoint;
			return Vector2.zero;
		}
	}
}
