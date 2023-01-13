#nullable enable

namespace EFT.Trainer.Model
{
	internal class HealthControllerWrapper : ReflectionWrapper
	{
		public HealthControllerWrapper(object instance) : base(instance)
		{
		}

		public Player? Player
		{
			get
			{
				// <= 0.12.12.17107
				var player = GetFieldValue<Player>(nameof(Player), warnOnFailure: false);

				// after, when cleaned-up by spt-aki
				if (player == null)
					player = GetFieldValue<Player>("player_0");

				return player;
			}
		}
	}
}
