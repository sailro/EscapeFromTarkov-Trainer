using EFT.HealthSystem;

#nullable enable

namespace EFT.Trainer.Model;

internal class ActiveHealthControllerWrapper(ActiveHealthController instance) : ReflectionWrapper(instance)
{
	public Player? Player => GetFieldValue<Player>(nameof(Player));
}
