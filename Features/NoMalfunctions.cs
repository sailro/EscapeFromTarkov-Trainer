using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoMalfunctions : ToggleFeature
{
	public override string Name => Strings.FeatureNoMalfunctionsName;
	public override string Description => Strings.FeatureNoMalfunctionsDescription;

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		if (player.HandsController.Item is not Weapon)
			return;

		if (player.HandsController is not Player.FirearmController controller) 
			return;

		var template = controller.Item?.Template;
		if (template == null)
			return;

		template.AllowFeed = false;
		template.AllowJam = false;
		template.AllowMisfire = false;
		template.AllowOverheat = false;
		template.AllowSlide = false;
	}
}
