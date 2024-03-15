using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class NoCollision : ToggleFeature
{
	public override string Name => "nocoll";
	public override string Description => "No physical collisions, making you immune to bullets, grenades and barbed wires.";

	public override bool Enabled { get; set; } = false;

	protected override void Update()
	{
		base.Update();

		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return;

		foreach (var rigidbody in player.GetComponentsInChildren<Rigidbody>())
		{
			if (rigidbody.detectCollisions == !Enabled)
				continue;

			rigidbody.detectCollisions = !Enabled;
		}
	}
}
