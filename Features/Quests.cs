using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
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
			var questController = new QuestController(profile);
			if (!questController.IsValid)
				return Empty;

			var records = new List<PointOfInterest>();

			try
			{
				// Step 1: find all locations to place quest items we have in the player inventory
				RefreshPlaceItemLocations(questController, records, camera);

				// Step 2: search for all lootItems related to quests
				RefreshFindItemLocations(world, questController, records, camera);
			}
			catch
			{
				// we are using dynamic objects here, so we can hit MissingMemberException in case the member names/scope change.
#if DEBUG
				throw;
#endif
			}

			return records.ToArray();
		}

		private void RefreshFindItemLocations(GameWorld world, QuestController questController, List<PointOfInterest> records, Camera camera)
		{
			var lootItems = world.LootItems;
			var startedQuests = questController.GetStartedQuests();

			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				if (!lootItem.Item.QuestItem)
					continue;

				foreach (var quest in startedQuests)
				{
					foreach (ConditionFindItem condition in quest.GetConditions<ConditionFindItem>(EQuestStatus.AvailableForFinish))
					{
						if (condition.target.Contains(lootItem.Item.TemplateId) && !quest.ConditionHandlers[condition].Test() &&
						    !quest.CompletedConditions.Contains(condition.id))
						{
							var position = lootItem.transform.position;
							records.Add(new PointOfInterest
							{
								Name = $"{lootItem.Item.ShortName.Localized()} ({quest.Template.Name})",
								Position = position,
								ScreenPosition = camera.WorldPointToScreenPoint(position),
								Color = Color
							});
						}
					}
				}
			}
		}

		private void RefreshPlaceItemLocations(QuestController questController, List<PointOfInterest> records, Camera camera)
		{
			var triggers = FindObjectsOfType<PlaceItemTrigger>();
			var profile = questController.Profile;
			var allPlayerItems = profile
				.Inventory
				.AllPlayerItems
				.ToArray();

			foreach (var trigger in triggers)
			{
				var items = questController
					.Quests
					.GetConditionHandlersByZone<ConditionZone>(trigger.Id);

				foreach (var item in items)
				{
					var conditionZone = (ConditionZone) item.Condition;
					var result = allPlayerItems.FirstOrDefault(x => conditionZone.target.Contains(x.TemplateId));
					if (result == null)
						continue;

					var isMultitool = result.TemplateId == KnownTemplateIds.MultiTool;
					var position = trigger.transform.position;
					records.Add(new PointOfInterest
					{
						Name = isMultitool ? "Repair" : $"Place {result.Template.NameLocalizationKey.Localized()}",
						Position = position,
						ScreenPosition = camera.WorldPointToScreenPoint(position),
						Color = Color
					});
					break;
				}
			}
		}

		internal class QuestController
		{
			private readonly object? _instance;
			public Profile Profile { get; }

			public QuestController(Profile profile)
			{
				const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				Profile = profile;

				// before 12.12, it's a public field
				var field = typeof(Profile).GetField("Quests", bindingFlags);
				if (field != null)
					_instance = profile; // in this case link directly to the Profile

				// after 12.12, it' s a non public field to a controller
				field = typeof(Profile).GetField("_questController", bindingFlags);
				if (field != null)
					_instance = field.GetValue(profile);  // in this case link to the Quest controller
			}

			public bool IsValid => _instance != null;

			public dynamic Quests
			{
				get
				{
					if (!IsValid)
						return Array.Empty<object>();

					return ((dynamic) _instance!).Quests;
				}
			} 

			public IList<dynamic> GetStartedQuests()
			{
				var result = new List<dynamic>();

				foreach (var quest in Quests)
				{
					if (quest.Template == null)
						continue;

					if (quest.QuestStatus != EQuestStatus.Started)
						continue;

					result.Add(quest);
				}

				return result;
			}
		}
	}
}
