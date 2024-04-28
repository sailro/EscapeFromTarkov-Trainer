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

		// StayInTarkov (SIT) is exposing a LootItems type in the global namespace, so make sure we use a qualified name here
		ConfigurationManager.SavePropertyValue(filename, LootItemsFeature, nameof(LootItemsFeature.TrackedNames));
	}
}
