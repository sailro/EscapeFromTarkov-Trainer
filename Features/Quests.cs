using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Counters;
using EFT.Interactive;
using EFT.Quests;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Quests : PointOfInterests
{
	public override string Name => Strings.FeatureQuestsName;
	public override string Description => Strings.FeatureQuestsDescription;

	[ConfigurationProperty]
	public Color Color { get; set; } = Color.magenta;

	public override float CacheTimeInSec { get; set; } = 5f;
	public override bool Enabled { get; set; } = false;
	public override Color GroupingColor => Color;

	public override void RefreshData(List<PointOfInterest> data)
	{
		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var camera = GameState.Current?.Camera;
		if (camera == null)
			return;

		var profile = player.Profile;

		var startedQuests = profile.QuestsData
			.Where(q => q.Status is EQuestStatus.Started && q.Template != null)
			.ToArray();

		if (!startedQuests.Any())
			return;

		RefreshPlaceOrRepairItemLocations(startedQuests, profile, data);
		RefreshVisitPlaceLocations(startedQuests, profile, data); 
		RefreshFindItemLocations(startedQuests, world, data);
	}

	private void RefreshVisitPlaceLocations(QuestDataClass[] startedQuests, Profile profile, List<PointOfInterest> records)
	{
		var triggers = FindObjectsOfType<ExperienceTrigger>();

		foreach (var quest in startedQuests)
		{
			var conditions = quest.Template!.Conditions[EQuestStatus.AvailableForFinish].OfType<ConditionCounterCreator>().ToArray();
			foreach (var condition in conditions)
			{
				if (quest.CompletedConditions.Contains(condition.id))
					continue;

				foreach (var cvp in condition.Conditions.OfType<ConditionVisitPlace>())
				{
					var trigger = triggers.FirstOrDefault(t => t.Id == cvp.target);
					if (trigger == null)
						continue;

					var visited = profile.Stats.Eft.OverallCounters.GetInt(CounterTag.TriggerVisited, trigger.Id) > 0;
					if (visited)
						continue;

					var position = trigger.transform.position;
					AddQuestRecord(records, condition, quest, position);
					break;
				}
			}
		}
	}

	private void RefreshFindItemLocations(QuestDataClass[] startedQuests, GameWorld world, List<PointOfInterest> records)
	{
		var lootItems = world.LootItems;

		for (var i = 0; i < lootItems.Count; i++)
		{
			var lootItem = lootItems.GetByIndex(i);
			if (!lootItem.IsValid())
				continue;

			if (!lootItem.Item.QuestItem)
				continue;

			foreach (var quest in startedQuests)
			{
				foreach (var condition in quest.Template!.Conditions[EQuestStatus.AvailableForFinish].OfType<ConditionFindItem>())
				{
					if (!condition.target.Contains(lootItem.Item.TemplateId) || quest.CompletedConditions.Contains(condition.id)) 
						continue;

					var position = lootItem.transform.position;
					AddQuestRecord(records, condition, quest, position);
				}
			}
		}
	}

	private void RefreshPlaceOrRepairItemLocations(QuestDataClass[] startedQuests, Profile profile, List<PointOfInterest> records)
	{
		var allPlayerItems = profile
			.Inventory
			.GetPlayerItems()
			.ToArray();

		var triggers = FindObjectsOfType<PlaceItemTrigger>();

		foreach (var quest in startedQuests)
		{
			var conditions = quest.Template!.Conditions[EQuestStatus.AvailableForFinish].OfType<ConditionZone>().ToArray();
			foreach (var condition in conditions)
			{
				if (quest.CompletedConditions.Contains(condition.id))
					continue;

				var result = allPlayerItems.FirstOrDefault(x => condition.target.Contains(x.TemplateId));
				if (result == null)
					continue;

				var trigger = triggers.FirstOrDefault(t => t.Id == condition.zoneId);
				if (trigger == null)
					continue;

				var position = trigger.transform.position;
				AddQuestRecord(records, condition, quest, position);
				break;
			}
		}
	}

	private void AddQuestRecord(List<PointOfInterest> records, Condition condition, QuestDataClass quest, Vector3 position)
	{
		var poi = Pool.Get();
		poi.Name = $"{condition.FormattedDescription} ({quest.Template!.Name})";
		poi.Position = position;
		poi.Color = Color;
		poi.Owner = null;

		records.Add(poi);
	}
}
