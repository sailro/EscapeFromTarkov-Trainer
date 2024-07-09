using EFT.Trainer.Properties;
using EFT.UI;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class ConsoleCommandWithoutArgument : ConsoleCommand
{
	public abstract void Execute();

	public override void Register()
	{
#if DEBUG
		AddConsoleLog(string.Format(Strings.DebugRegisteringCommandFormat, Name));
#endif
		ConsoleScreen.Processor.RegisterCommand(Name, Execute);
	}
}
