using System.Text.RegularExpressions;
using EFT.Trainer.Configuration;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class LoadTrackList : BaseTrackListCommand
{
	public override string Name => "loadtl";

	public override void Execute(Match match)
	{
		if (!TryGetTrackListFilename(match, out var filename))
			return;

		ConfigurationManager.LoadPropertyValue(filename, LootItems, nameof(LootItems.TrackedNames));
	}
}
