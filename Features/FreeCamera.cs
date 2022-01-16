using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class FreeCamera : ToggleFeature
	{
		private const string MouseXAxis = "Mouse X";
		private const string MouseYAxis = "Mouse Y";

		public override string Name => "camera";

		[ConfigurationProperty(Skip = true)] // we do not want to offer save/load support for this
		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty(Order = 20)]
		public KeyCode Forward { get; set; } = KeyCode.UpArrow;

		[ConfigurationProperty(Order = 21)]
		public KeyCode Backward { get; set; } = KeyCode.DownArrow;

		[ConfigurationProperty(Order = 22)]
		public KeyCode Left { get; set; } = KeyCode.LeftArrow;

		[ConfigurationProperty(Order = 23)]
		public KeyCode Right { get; set; } = KeyCode.RightArrow;

		[ConfigurationProperty(Order = 24)]
		public KeyCode FastMode { get; set; } = KeyCode.RightShift;

		[ConfigurationProperty(Order = 25)]
		public KeyCode Teleport { get; set; } = KeyCode.T;

		[ConfigurationProperty(Order = 30)]
		public float FreeLookSensitivity { get; set; } = 3f;

		[ConfigurationProperty(Order = 31)]
		public float MovementSpeed { get; set; } = 10f;

		[ConfigurationProperty(Order = 32)]
		public float FastMovementSpeed { get; set; } = 100f;


		private void TogglePlayerActiveStatusIfNeeded()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var playerGameObject = player.gameObject;
			if (playerGameObject.activeSelf == Enabled)
				playerGameObject.SetActive(!Enabled);
		}

		protected override void Update()
		{
			base.Update();

			TogglePlayerActiveStatusIfNeeded();
		}

		protected override void UpdateWhenEnabled()
		{
			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

            var fastMode = Input.GetKey(FastMode);
            var movementSpeed = fastMode ? FastMovementSpeed : MovementSpeed;

            var heading = Vector3.zero;
            var cameraTransform = camera.transform;

            if (Input.GetKey(Left))
	            heading = -cameraTransform.right;

            if (Input.GetKey(Right))
	            heading = cameraTransform.right;

            if (Input.GetKey(Forward))
	            heading = cameraTransform.forward;

            if (Input.GetKey(Backward))
	            heading = -cameraTransform.forward;

			if (heading != Vector3.zero)
				cameraTransform.position += movementSpeed * Time.deltaTime * heading;

			var localEulerAngles = cameraTransform.localEulerAngles;
			var newRotationX = localEulerAngles.y + Input.GetAxis(MouseXAxis) * FreeLookSensitivity;
            var newRotationY = localEulerAngles.x - Input.GetAxis(MouseYAxis) * FreeLookSensitivity;
            cameraTransform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);

            if (Input.GetKey(Teleport))
            {
	            var player = GameState.Current?.LocalPlayer;
	            if (!player.IsValid())
		            return;

	            var position = new Vector3(cameraTransform.position.x, cameraTransform.position.y - 2f, cameraTransform.position.z);
	            var playerGameObject = player.gameObject;

	            playerGameObject.transform.SetPositionAndRotation(position, cameraTransform.rotation);
	            playerGameObject.SetActive(true);
	            Enabled = false;
            }
		}
	}
}
