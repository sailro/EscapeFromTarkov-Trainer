using EFT.Interactive;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Doors : TriggerMonoBehaviour
	{
		public override KeyCode Key { get; set; } = KeyCode.KeypadPeriod;

		protected override void UpdateWhenTriggered()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var doors = FindObjectsOfType<Door>();
			foreach (var door in doors)
			{
				if (!door.IsValid())
					continue;

				if (door.DoorState != EDoorState.Locked)
					continue;

				var offset = player!.Transform.position - door.transform.position;
				var sqrLen = offset.sqrMagnitude;

				// only unlock if near me, else you'll get a ban from BattlEye if you brute-force-unlock all doors
				if (sqrLen <= 20.0f)
					door.DoorState = EDoorState.Shut;
			}
		}
	}
}
