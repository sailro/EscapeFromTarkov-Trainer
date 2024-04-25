using EFT.Trainer.Features;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class Status : ConsoleCommandWithoutArgument
{
	public override string Name => "status";

	private static string GetFeatureHelpText(ToggleFeature feature)
	{
		var toggleKey = feature.Key != KeyCode.None ? $" ({feature.Key} to toggle)" : string.Empty;
		return $"{feature.Name} is {(feature.Enabled ? "on".Green() : "off".Red())}{toggleKey}";
	}

	public override void Execute()
	{
		foreach (var feature in Context.ToggleableFeatures.Value)
		{
			if (feature is Commands or GameState)
				continue;

			AddConsoleLog(GetFeatureHelpText(feature));
		}
	}
}
