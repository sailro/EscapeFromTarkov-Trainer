using EFT.UI;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class ConsoleCommandWithoutArgument : ConsoleCommand
{
	public abstract void Execute();

	public override void Register()
	{
#if DEBUG
		AddConsoleLog($"Registering {Name} command...");
#endif
		ConsoleScreen.Processor.RegisterCommand(Name, Execute);
	}
}
