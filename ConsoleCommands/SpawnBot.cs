﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Comfort.Common;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class SpawnBot : BaseTemplateCommand
{
	public const string MatchAll = "*";

	public override string Name => Strings.CommandSpawnBot;

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not { Success: true })
			return;

		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return;

		var search = matchGroup.Value.Trim();
		var instance = Singleton<IBotGame>.Instance;
		if (instance == null)
			return;

		var names = GetBotNames();

		var bots = names
			.Where(n => n.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToArray();

		if (search == MatchAll)
		{
			bots = names;
		}
		else
		{
			switch (bots.Length)
			{
				case 0:
					AddConsoleLog(Strings.ErrorNoBotFound.Red());
					return;
				case > 1:
					foreach (var bot in bots)
						AddConsoleLog(string.Format(Strings.CommandSpawnBotEnumerateFormat, bot.Green()));

					AddConsoleLog(string.Format(Strings.ErrorTooManyBotsFormat, bots.Length.ToString().Cyan()));
					return;
			}
		}

		foreach (var bot in bots)
			instance.BotsController.SpawnBotDebugServer(EPlayerSide.Savage, false, (WildSpawnType)Enum.Parse(typeof(WildSpawnType), bot), BotDifficulty.normal, true);
	}

	private static string[] GetBotNames()
	{
		var filter = new[] { "test", "event", "spirit", "shooterbtr" };

		return Enum
			.GetNames(typeof(WildSpawnType))
			.Where(n => !filter.Any(f => n.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
			.OrderBy(n => n)
			.ToArray();
	}
}