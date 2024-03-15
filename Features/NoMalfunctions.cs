using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoMalfunctions : ToggleFeature
{
	public override string Name => "nomal";
	public override string Description => "No weapon malfunctions: no misfires or failures to eject or feed. No jammed bolts or overheating.";

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		if (player.HandsController.Item is not Weapon)
			return;

		if (!player.TryGetComponent<Player.FirearmController>(out var firearmController))
			return;

		var template = firearmController.Item?.Template;
		if (template == null)
			return;

		template.AllowFeed = false;
		template.AllowJam = false;
		template.AllowMisfire = false;
		template.AllowOverheat = false;
		template.AllowSlide = false;
	}
}
