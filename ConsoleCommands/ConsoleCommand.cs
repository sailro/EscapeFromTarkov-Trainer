using System;
using EFT.UI;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class ConsoleCommand
{
	// StayInTarkov (SIT) is exposing a LootItems type in the global namespace, so make sure we use a qualified name here
	private readonly Lazy<Features.LootItems> _lootItems = new(() => Features.FeatureFactory.GetFeature<Features.LootItems>()!);
	protected Features.LootItems LootItemsFeature => _lootItems.Value;

	public abstract string Name { get; }

	internal void AddConsoleLog(string log)
	{
		if (PreloaderUI.Instantiated)
			ConsoleScreen.Log(log);
	}

	public abstract void Register();
}
