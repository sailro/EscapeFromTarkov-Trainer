using EFT.Trainer.Configuration;
using JetBrains.Annotations;

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class FovChanger : ToggleFeature
	{
		public override string Name => "fovchanger";
		[ConfigurationProperty(Order = 1)]
		public override bool Enabled { get; set; } = true;

		[ConfigurationProperty(Order = 3)]
		public float Fov { get; set; } = 90;

		protected override void LateUpdateWhenEnabled()
		{
			var camera = GameState.Current?.Camera;

			if (camera == null)
				return;

			if (Enabled)
				camera.fieldOfView = Fov;
		}
	}
}
