using EFT.Trainer.Configuration;
using JetBrains.Annotations;

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Interact : ToggleFeature
{
	public override string Name => "interact";
	public override string Description => "Change distance for loot/door interaction.";

	public override bool Enabled { get; set; } = false;

	[ConfigurationProperty]
	public float Distance { get; set; } = 1f;

	public static float DefaultLootDistance { get; set; } = EFTHardSettings.Instance.LOOT_RAYCAST_DISTANCE;
	public static float DefaultDoorDistance { get; set; } = EFTHardSettings.Instance.DOOR_RAYCAST_DISTANCE;

	protected override void UpdateWhenEnabled()
	{
		EFTHardSettings.Instance.LOOT_RAYCAST_DISTANCE = Distance;
		EFTHardSettings.Instance.DOOR_RAYCAST_DISTANCE = Distance;
	}

	protected override void UpdateWhenDisabled()
	{
		EFTHardSettings.Instance.LOOT_RAYCAST_DISTANCE = DefaultLootDistance;
		EFTHardSettings.Instance.DOOR_RAYCAST_DISTANCE = DefaultDoorDistance;
	}
}
