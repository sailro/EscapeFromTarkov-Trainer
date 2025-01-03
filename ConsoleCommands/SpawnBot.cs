using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using HarmonyLib;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class SpawnBot : ConsoleCommandWithArgument
{
	public const string MatchAll = "*";

	public override string Pattern => RequiredArgumentPattern;
	public override string Name => Strings.CommandSpawnBot;

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not { Success: true })
			return;

		var search = matchGroup.Value.Trim();
		var bots = FindBots(search);

		switch (bots.Length)
		{
			case 0:
				AddConsoleLog(Strings.ErrorNoBotFound.Red());
				return;

			case > 1 when search != MatchAll:
				foreach (var bot in bots)
					AddConsoleLog(string.Format(Strings.CommandSpawnBotEnumerateFormat, bot.Green()));

				AddConsoleLog(string.Format(Strings.ErrorTooManyBotsFormat, bots.Length.ToString().Cyan()));
				return;
		}

		SpawnBots(bots);
	}

	private void SpawnBots(string[] bots)
	{
		var instance = Singleton<IBotGame>.Instance;
		if (instance == null)
			return;

		var controller = instance.BotsController;

		foreach (var bot in bots)
			SpawnBotDebugServer(controller, EPlayerSide.Savage, false, (WildSpawnType)Enum.Parse(typeof(WildSpawnType), bot), BotDifficulty.normal, true);
	}

	private void SpawnBotDebugServer(BotsController controller, EPlayerSide side, bool canBeSnipe, WildSpawnType profile = WildSpawnType.assault, BotDifficulty botDifficulty = BotDifficulty.normal, bool forcedSpawn = false)
	{
		var spawner = controller.BotSpawner;
		if (spawner == null)
			return;

		var randomBotZone = spawner.GetRandomBotZone(canBeSnipe);
		Spawn(spawner, side, randomBotZone, profile, botDifficulty, forcedSpawn).HandleExceptions();
	}

	public async Task Spawn(BotSpawner spawner, EPlayerSide side, BotZone zone, WildSpawnType profileType = WildSpawnType.assault, BotDifficulty botDifficulty = BotDifficulty.normal, bool forcedSpawn = false)
	{
		if (profileType.IsBossOrFollower())
		{
			var bossLocationSpawn = new BossLocationSpawn { 
				BossZone = "", 
				Time = 1f, 
				Delay = 0f, 
				TriggerId = "",
				TriggerName = "",
				BossChance = 100f,
				BossName = profileType.ToString(),
				BossDifficult = BotDifficulty.normal.ToString(),
				BossEscortAmount = 0.ToString(),
				BossEscortDifficult = BotDifficulty.normal.ToString(),
				BossEscortType = WildSpawnType.followerBully.ToString()
			};
			bossLocationSpawn.ParseMainTypesTypes();
			bossLocationSpawn.ForceSpawn = forcedSpawn;
			bossLocationSpawn.IgnoreMaxBots = forcedSpawn;
			spawner.BossSpawner.Spawn(bossLocationSpawn, new BotSpawnParams()).HandleExceptions();
		}
		else
		{
			const string fieldName = "_botCreator";
			var field = AccessTools.Field(spawner.GetType(), fieldName);
			if (field?.GetValue(spawner) is not IBotCreator botCreator)
			{
				AddConsoleLog(string.Format(Strings.ErrorCannotFindField, fieldName, spawner.GetType().Name).Red());				
				return;
			}

			spawner.TryToSpawnInZoneAndDelay(zone, await BotCreationDataClass.Create(new GetProfileData(side, profileType, botDifficulty), botCreator, 1, spawner), withCheckMinMax: true, newWave: true, null, forcedSpawn);
		}
	}

	public class GetProfileData(EPlayerSide side, WildSpawnType spawnType, BotDifficulty botDifficulty, BotSpawnParams? spawnParams = null) : IGetProfileData
	{
		public EPlayerSide? Side { get; } = side;
		public BotSpawnParams? SpawnParams { get; set; } = spawnParams;

		public bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty)
		{
			role = spawnType;
			difficulty = botDifficulty;
			return true;
		}

		public Profile? ChooseProfile(List<Profile> profiles2Select, bool withDelete)
		{
			var list = profiles2Select.Where((x) => x.Info.Side == Side && x.Info.Settings.Role == spawnType && x.Info.Settings.BotDifficulty == botDifficulty).ToList();
			if (list.Count == 0)
				return null;

			var profile = list.Random();
			if (withDelete)
				profiles2Select.Remove(profile);

			return profile;
		}

		public WaveInfo[] PrepareToLoadBackend(int count)
		{
			var waveInfo = new WaveInfo(count, spawnType, botDifficulty);
			return [waveInfo];
		}

		public bool IsValidSpawnType(WildSpawnType wildSpawnType)
		{
			return wildSpawnType == spawnType;
		}

		public string GetDebugLocalName()
		{
			return $"{Side}{Random.Range(0, 256)} Profile";
		}

		public string GetDebugData()
		{
			return $" Side:{Side} Type:{spawnType}  BotDifficulty:{botDifficulty}";
		}

		public bool ShallChooseByData()
		{
			return spawnType is WildSpawnType.exUsec or WildSpawnType.pmcBot or WildSpawnType.assaultGroup or WildSpawnType.arenaFighter;
		}

		public bool IsBossOrFollowerByTime()
		{
			return IsBossOrFollower();
		}

		public bool IsZeroWave()
		{
			return false;
		}

		public bool IsBossOrFollower()
		{
			return spawnType.IsBossOrFollower();
		}

		public bool IsSpawnOnStart()
		{
			return false;
		}

		public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
		{
			return !botsControllerZonesLeaveController.IsZoneBlockFor(botZone, spawnType);
		}
	}

	private static string[] FindBots(string search)
	{
		var names = GetBotNames();

		if (search == MatchAll)
			return names;

		var exactMatch = names
			.Where(n => n.Equals(search, StringComparison.OrdinalIgnoreCase))
			.ToArray();

		if (exactMatch.Length == 1)
			return exactMatch;

		return names
			.Where(n => n.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToArray();
	}

	private static string[] GetBotNames()
	{
		var filter = new[] { "test", "event", "spirit", "shooterbtr" };

		return [.. Enum
			.GetNames(typeof(WildSpawnType))
			.Where(n => !filter.Any(f => n.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
			.OrderBy(n => n)];
	}
}
