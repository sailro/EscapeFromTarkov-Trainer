using EFT.Trainer.Configuration;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class FovChanger : ToggleFeature
	{
		public override string Name => "fovchanger";
		[ConfigurationProperty(Order = 1)]
		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty(Order = 2)]
		public float Fov { get; set; } = 75f;
		[ConfigurationProperty(Order = 3)]
		public float CameraOffset { get; set; } = 0.05f;

		[UsedImplicitly]
		private void LateUpdate()
		{
			if (!Enabled)
				return;
			var player = GameState.Current?.LocalPlayer;
			var camera = GameState.Current?.Camera;
			if (camera == null || player == null)
				return;

			player.ProceduralWeaponAnimation.HandsContainer.CameraOffset =
				new Vector3(0.04f, 0.04f, CameraOffset!);
			camera.fieldOfView = Fov;
		}
	}
}
