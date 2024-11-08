using Comfort.Common;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Mortar : TriggerFeature
{
	public override string Name => Strings.FeatureMortarName;
	public override string Description => Strings.FeatureMortarDescription;

	public override KeyCode Key { get; set; } = KeyCode.None;

	protected override void UpdateOnceWhenTriggered()
	{
		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return;

		world.ServerShellingController?.StartShellingPosition(player.Transform.position);
	}
}
