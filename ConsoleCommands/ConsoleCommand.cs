using EFT.UI;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class ConsoleCommand
{
	public abstract string Name { get; }

	internal void AddConsoleLog(string log)
	{
		if (PreloaderUI.Instantiated)
			ConsoleScreen.Log(log);
	}

	public abstract void Register();
}
