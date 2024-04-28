using System.Text.RegularExpressions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class UnTrack : BaseTrackCommand
{
	public override string Name => "untrack";

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		TrackList.ShowTrackList(this, LootItemsFeature, LootItemsFeature.UnTrack(matchGroup.Value));
	}
}
