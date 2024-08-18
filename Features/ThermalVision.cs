using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class ThermalVision : ToggleFeature
{
	public override string Name => Strings.FeatureThermalVisionName;
	public override string Description => Strings.FeatureThermalVisionDescription;

	public override bool Enabled { get; set; } = false;

	protected override void Update()
	{
		base.Update();

		// Do not interact while in hideout or if the player is already wearing compatible equipment
		var player = GameState.Current?.LocalPlayer;
		if (player == null || player is HideoutPlayer || player.HasItemComponentInSlot<ThermalVisionComponent>(EquipmentSlot.Headwear))
			return;

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
