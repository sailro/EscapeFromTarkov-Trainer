using System.Collections.Generic;
using EFT.Interactive;
using EFT.Trainer.Extensions;

namespace EFT.Trainer.Features
{
	public class Doors : CachableMonoBehaviour<IEnumerable<Door>>
	{
		public override float CacheTimeInSec => 10f;
		public override bool Enabled => true;

		public override IEnumerable<Door> RefreshData()
		{
			var doors = FindObjectsOfType<Door>();
			foreach (var door in doors)
			{
				if (!door.IsValid())
					continue;

				yield return door;
			}
		}

		public override void ProcessData(IEnumerable<Door> data)
		{
			var player = GameState.Current?.LocalPlayer;
			if (player == null)
				return;

			foreach (var door in data)
			{
				if (door.DoorState != EDoorState.Locked)
					continue;

				var offset = player.Transform.position - door.transform.position;
				var sqrLen = offset.sqrMagnitude;

				// only unlock if near me, else you'll get a ban from BattlEye if you brute-force-unlock all doors
				if (sqrLen <= 20.0f)
					door.DoorState = EDoorState.Shut;
			}
		}
	}
}
