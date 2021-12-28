using System.Collections.Generic;
using System.Linq;
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
			var records = new List<PointOfInterest>();

			try
			{
				// Step 1: find all locations to place quest items we have in the player inventory
				RefreshPlaceItemLocations(profile, records, camera);

				// Step 2: search for all lootItems related to quests
				RefreshFindItemLocations(world, profile, records, camera);
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

		private void RefreshFindItemLocations(GameWorld world, Profile profile, List<PointOfInterest> records, Camera camera)
		{
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

		private void RefreshPlaceItemLocations(Profile profile, List<PointOfInterest> records, Camera camera)
		{
			var triggers = FindObjectsOfType<PlaceItemTrigger>();
			var allPlayerItems = profile
				.Inventory
				.AllPlayerItems
				.ToArray();

			foreach (var trigger in triggers)
			{
				var items = profile
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
	}
}
