#nullable enable

using UnityEngine;

namespace EFT.Trainer.Features
{
	public class NightVision : ToggleMonoBehaviour
	{
		public override bool Enabled { get; set; } = false;

		protected override void Update()
		{
			base.Update();

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var component = camera.GetComponent<BSG.CameraEffects.NightVision>();
			if (component == null || component.On == Enabled)
				return;

			component.StartSwitch(Enabled);
			if (Enabled)
			{
				component.DiffuseIntensity = 0f;
				component.Intensity = 0f;
				component.NoiseIntensity = 0f;

				component.TextureMask.Color = new Color(0f, 0f, 0f, 0f);
				component.TextureMask.Stretch = false;
				component.TextureMask.Size = 0f;
			}
		}
	}
}
