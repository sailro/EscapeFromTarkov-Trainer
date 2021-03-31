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
		}
	}
}
