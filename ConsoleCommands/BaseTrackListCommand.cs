using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class BaseTrackListCommand : ConsoleCommandWithArgument
{
	public override string Pattern => RequiredArgumentPattern;

	protected static bool TryGetTrackListFilename(Match match, [NotNullWhen(true)] out string? filename)
	{
		filename = null;

		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return false;

		filename = matchGroup.Value;

		if (!Path.IsPathRooted(filename))
			filename = Path.Combine(Context.UserPath, filename);

		if (!Path.HasExtension(filename))
			filename += ".tl";

		return true;
	}
}
