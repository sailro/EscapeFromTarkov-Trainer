using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class ThermalVision : ToggleMonoBehaviour
	{
		public override bool Enabled { get; set; } = false;

		protected override void Update()
		{
			base.Update();

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var component = camera.GetComponent<global::ThermalVision>();
			if (component == null || component.On == Enabled)
				return;

			component.StartSwitch(Enabled);
			
			if (!Enabled) 
				return;

			component.IsFpsStuck = false;
			component.IsGlitch = false;
			component.IsMotionBlurred = false;
			component.IsNoisy = false;
			component.IsPixelated = false;

			component.TextureMask.Color = new Color(0f, 0f, 0f, 0f);
			component.TextureMask.Stretch = false;
			component.TextureMask.Size = 0f;
		}
	}
}
