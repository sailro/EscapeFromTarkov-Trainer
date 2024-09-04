using System.Collections.Generic;
using System.Linq;
using EFT.Quests;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class SpawnQuestItems : ConsoleCommandWithoutArgument
{
	public override string Name => Strings.CommandSpawnQuestItems;

	public override void Execute()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var profile = player.Profile;

		var startedQuests = profile.QuestsData
			.Where(q => q.Status is EQuestStatus.Started && q.Template != null)
			.ToArray();

		if (!startedQuests.Any())
			return;

		foreach (var quest in startedQuests)
		{
			foreach (var condition in GetConditions(quest))
			{
				var count = Mathf.RoundToInt(condition.value);
				if (count is <= 0 or > 20)
					continue;

				foreach (var target in condition.target)
				{
					for (var i = 0; i < count; i++)
						Spawn.SpawnTemplate(target, player, this, t => !t.QuestItem); // for now we are only able to spawn non location-specific items like batteries, meds, keys, etc.
				}
			}
		}
	}

	private static IEnumerable<ConditionMultipleTargets> GetConditions(QuestDataClass quest)
	{
		// do we need to add ConditionMultipleTargets / ConditionItem / ConditionPlaceItem
		var conditions = quest.Template!.Conditions[EQuestStatus.AvailableForFinish];
		return conditions.OfType<ConditionFindItem>();
	}
}
