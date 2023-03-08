using UnityEngine;

namespace EFT.Trainer.Features
{
	internal abstract class BaseMapToggleFeature : ToggleFeature
	{
		private GameObject? _radarCameraObject = null;
		private Camera? _radarCamera = null;

		protected void ToggleMapCameraIfNeeded(bool state)
		{
			if (_radarCamera == null)
				return;

			if (_radarCamera.enabled == state)
				return;

			_radarCamera.enabled = state;
		}

		protected void SetupMapCameraOnce(Camera camera, float x, float y, float sizex, float sizey)
		{
			if (_radarCameraObject != null)
			{
				ToggleMapCameraIfNeeded(true);
				return;
			}

			// We need to setup weather for proper rendering
			Weather.ToClearWeather();

			_radarCameraObject = new GameObject(GetType().FullName + nameof(_radarCameraObject), typeof(Camera), typeof(PrismEffects));
			_radarCameraObject.GetComponent<PrismEffects>().CopyComponentValues(camera.GetComponent<PrismEffects>());
			_radarCamera = _radarCameraObject.GetComponent<Camera>();
			_radarCamera.name = GetType().FullName + nameof(_radarCamera);
			_radarCamera.pixelRect = new Rect(x, y, sizex, sizey);
			_radarCamera.allowHDR = false;
			_radarCamera.depth = -1;

			// When using several cameras
			// _radarCamera.transparencySortMode = TransparencySortMode.CustomAxis;

			// Prevent NullReferenceException in PrismEffects 
			GameWorld.OnDispose -= UpdateWhenDisabled;
			GameWorld.OnDispose += UpdateWhenDisabled;
		}

		protected void UpdateMapCamera(Transform cameraTransform, float range)
		{
			if (_radarCameraObject == null) 
				return;

			var radarCameraTransform = _radarCameraObject.transform;
			radarCameraTransform.eulerAngles = new Vector3(90, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
			radarCameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, range * Mathf.Tan(45), cameraTransform.localPosition.z);
		}
	}
}
