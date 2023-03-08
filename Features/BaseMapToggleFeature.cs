using UnityEngine;

namespace EFT.Trainer.Features
{
	internal abstract class BaseMapToggleFeature : ToggleFeature
	{
		private GameObject? _mapCameraObject = null;
		private Camera? _mapCamera = null;

		protected void ToggleMapCameraIfNeeded(bool state)
		{
			if (_mapCamera == null)
				return;

			if (_mapCamera.enabled == state)
				return;

			_mapCamera.enabled = state;
		}

		protected void SetupMapCameraOnce(Camera camera, float x, float y, float sizex, float sizey)
		{
			if (_mapCameraObject != null)
			{
				ToggleMapCameraIfNeeded(true);
				return;
			}

			// We need to setup weather for proper rendering
			Weather.ToClearWeather();

			_mapCameraObject = new GameObject(GetType().FullName + nameof(_mapCameraObject), typeof(Camera), typeof(PrismEffects));
			_mapCameraObject.GetComponent<PrismEffects>().CopyComponentValues(camera.GetComponent<PrismEffects>());
			_mapCamera = _mapCameraObject.GetComponent<Camera>();
			_mapCamera.name = GetType().FullName + nameof(_mapCamera);
			_mapCamera.pixelRect = new Rect(x, y, sizex, sizey);
			_mapCamera.allowHDR = false;
			_mapCamera.depth = -1;


			// Prevent NullReferenceException in PrismEffects 
			GameWorld.OnDispose -= UpdateWhenDisabled;
			GameWorld.OnDispose += UpdateWhenDisabled;
		}

		protected void UpdateMapCamera(Transform cameraTransform, float range)
		{
			if (_mapCameraObject == null) 
				return;

			var mapTransform = _mapCameraObject.transform;
			mapTransform.eulerAngles = new Vector3(90, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
			mapTransform.localPosition = new Vector3(cameraTransform.localPosition.x, range * Mathf.Tan(45), cameraTransform.localPosition.z);
		}
	}
}
