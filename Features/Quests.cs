using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.Quests;
using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Quests : PointOfInterests
	{
		public static readonly Color QuestColor = Color.magenta;

		public override float CacheTimeInSec => 5f;
		public override bool Enabled { get; set; } = false;

		public static PointOfInterest[] Empty => Array.Empty<PointOfInterest>();

		public override PointOfInterest[] RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return Empty;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return Empty;

			var camera = Camera.main;
			if (camera == null)
				return Empty;

			var profile = player!.Profile;
			var triggers = FindObjectsOfType<PlaceItemTrigger>();
			var source = profile
				.Inventory
				.AllPlayerItems
				.ToArray();

			var records = new List<PointOfInterest>();

			// Step 1: find all locations to place quest items we have in the player inventory
			foreach (var trigger in triggers)
			{
				var ìtems = profile.
					Quests.
					GetConditionHandlersByZone<ConditionZone>(trigger.Id)
					.ToArray();

				foreach (var item in ìtems)
				{
					var conditionZone = (ConditionZone)item.Condition;
					var result = source.FirstOrDefault(x => conditionZone.target.Contains(x.TemplateId));
					if (result == null)
						continue;

					var isMultitool = result.TemplateId == KnownTemplateIds.MultiTool;
					var position = trigger.transform.position;
					records.Add(new PointOfInterest
					{
						Name = isMultitool ? "Repair" : $"Place {result.Template.NameLocalizationKey.Localized()}",
						Position = position,
						ScreenPosition = camera.WorldPointToScreenPoint(position),
						Color = QuestColor
					});
					break;
				}
			}

			// Step 2: search for all lootItems related to quests
			var lootItems = world.LootItems;

			var startedQuests = profile
				.Quests
				.LoadedList
				.Where(q => q.QuestStatus == EQuestStatus.Started)
				.ToArray();

			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				if (!lootItem.Item.QuestItem)
					continue;

				foreach (var quest in startedQuests)
				{
					foreach (var condition in quest.GetConditions<ConditionFindItem>(EQuestStatus.AvailableForFinish))
					{
						if (condition.target.Contains(lootItem.Item.TemplateId) && !quest.ConditionHandlers[condition].Test() && !quest.CompletedConditions.Contains(condition.id))
						{
							var position = lootItem.transform.position;
							records.Add(new PointOfInterest
							{
								Name = lootItem.Item.ShortName.Localized(),
								Position = position,
								ScreenPosition = camera.WorldPointToScreenPoint(position),
								Color = QuestColor
							});
						}
					}
				}
			}

			return records.ToArray();
		}
	}
}
