#nullable enable

namespace EFT.Trainer.Model
{
	internal class HealthControllerWrapper : ReflectionWrapper
	{
		public HealthControllerWrapper(object instance) : base(instance)
		{
		}

		public Player? Player => GetFieldValue<Player>(nameof(Player));
	}
}
