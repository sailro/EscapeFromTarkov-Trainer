using System.Text.RegularExpressions;
using EFT.Trainer.Configuration;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class SaveTrackList : BaseTrackListCommand
{
	public override string Name => "savetl";

	public override void Execute(Match match)
	{
		if (!TryGetTrackListFilename(match, out var filename))
			return;

		ConfigurationManager.SavePropertyValue(filename, LootItems, nameof(LootItems.TrackedNames));
	}
}
