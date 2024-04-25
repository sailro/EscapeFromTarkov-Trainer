using System.Text.RegularExpressions;
using EFT.Trainer.Features;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal class ToggleFeatureCommand(ToggleFeature feature) : ConsoleCommandWithArgument
{
	public override string Name => feature.Name;
	public override string Pattern => $"(?<{ValueGroup}>(on)|(off))";

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		feature.Enabled = matchGroup.Value switch
		{
			"on" => true,
			"off" => false,
			_ => feature.Enabled
		};
	}
}
