#nullable enable

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
		}
	}
}
