using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Durability : ToggleFeature
{
	public override string Name => "durability";
	public override string Description => "Maintains maximum durability of the player's weapon.";

	public override bool Enabled { get; set; } = false;

	protected override void UpdateWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		if (player.HandsController is not Player.FirearmController controller) 
			return;

		if (controller.Item?.Repairable is not {} repairable)
			return;

		repairable.MaxDurability = repairable.TemplateDurability;
		repairable.Durability = repairable.MaxDurability;
	}
}
