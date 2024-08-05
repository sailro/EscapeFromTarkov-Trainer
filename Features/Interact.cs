using EFT.Trainer.Configuration;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Interact : ToggleFeature
{
	public override string Name => Strings.FeatureInteractName;
	public override string Description => Strings.FeatureInteractDescription;

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
