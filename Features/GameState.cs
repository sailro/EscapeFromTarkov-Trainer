using System.Collections.Generic;
using Comfort.Common;
using EFT.Trainer.Extensions;

namespace EFT.Trainer
{
	public class GameState : CachableMonoBehaviour<GameStateSnapshot>
	{
		public static GameStateSnapshot Current { get; private set; }

		public override float CacheTimeInSec => 4f;
		public override bool Enabled { get; set; } = true;

		public override GameStateSnapshot RefreshData()
		{
			var snapshot = new GameStateSnapshot();
			var world = Singleton<GameWorld>.Instance;

			if (world == null)
				return null;

			var players = world.RegisteredPlayers;
			if (players == null)
				return null;

			var hostiles = new List<Player>();
			snapshot.Hostiles = hostiles;

			foreach (var player in players)
			{
				if (player.IsYourPlayer())
				{
					snapshot.LocalPlayer = player;
					continue;
				}

				if (!player.IsAlive())
					continue;

				hostiles.Add(player);
			}

			Current = snapshot;
			return snapshot;
		}
	}

	public class GameStateSnapshot
	{
		public Player LocalPlayer { get; set; }
		public IEnumerable<Player> Hostiles { get; set; }
	}
}
