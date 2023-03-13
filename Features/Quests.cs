using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Counters;
using EFT.Interactive;
using EFT.Quests;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Quests : PointOfInterests
	{
		public override string Name => "quest";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.magenta;

		public override float CacheTimeInSec { get; set; } = 5f;
		public override bool Enabled { get; set; } = false;
		public override Color GroupingColor => Color;

		public override PointOfInterest[] RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return Empty;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return Empty;

			var profile = player.Profile;
			var records = new List<PointOfInterest>();

			var startedQuests = profile.QuestsData
				.Where(q => q.Status is EQuestStatus.Started && q.Template != null)
				.ToArray();

			RefreshPlaceOrRepairItemLocations(startedQuests, profile, records, camera);
			RefreshVisitPlaceLocations(startedQuests, profile, records, camera); 
			RefreshFindItemLocations(startedQuests, world, records, camera);

			return records.ToArray();
		}

		private void RefreshVisitPlaceLocations(QuestDataClass[] startedQuests, Profile profile, List<PointOfInterest> records, Camera camera)
		{
			var triggers = FindObjectsOfType<ExperienceTrigger>();

			foreach (var quest in startedQuests)
			{
				var conditions = quest.Template!.GetConditions<ConditionCounterCreator>(EQuestStatus.AvailableForFinish).ToArray();
				foreach (var condition in conditions)
				{
					if (quest.CompletedConditions.Contains(condition.id))
						continue;

					foreach (var cvp in condition.counter.conditions.OfType<ConditionVisitPlace>())
					{
						var trigger = triggers.FirstOrDefault(t => t.Id == cvp.target);
						if (trigger == null)
							continue;

						var visited = profile.Stats.OverallCounters.GetInt(CounterTag.TriggerVisited, trigger.Id) > 0;
						if (visited)
							continue;

						var position = trigger.transform.position;
						AddQuestRecord(records, camera, condition, quest, position);
						break;
					}
				}
			}
		}

		private void RefreshFindItemLocations(QuestDataClass[] startedQuests, GameWorld world, List<PointOfInterest> records, Camera camera)
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
					foreach (var condition in quest.Template!.GetConditions<ConditionFindItem>(EQuestStatus.AvailableForFinish))
					{
						if (!condition.target.Contains(lootItem.Item.TemplateId) || quest.CompletedConditions.Contains(condition.id)) 
							continue;

						var position = lootItem.transform.position;
						AddQuestRecord(records, camera, condition, quest, position);
					}
				}
			}
		}

		private void RefreshPlaceOrRepairItemLocations(QuestDataClass[] startedQuests, Profile profile, List<PointOfInterest> records, Camera camera)
		{
			var allPlayerItems = profile
				.Inventory
				.AllPlayerItems
				.ToArray();

			var triggers = FindObjectsOfType<PlaceItemTrigger>();

			foreach (var quest in startedQuests)
			{
				var conditions = quest.Template!.GetConditions<ConditionZone>(EQuestStatus.AvailableForFinish).ToArray();
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
					AddQuestRecord(records, camera, condition, quest, position);
					break;
				}
			}
		}

		private void AddQuestRecord(List<PointOfInterest> records, Camera camera, Condition condition, QuestDataClass quest, Vector3 position)
		{
			records.Add(new PointOfInterest
			{
				Name = $"{condition.FormattedDescription} ({quest.Template!.Name})",
				Position = position,
				Color = Color
			});
		}
	}
}
