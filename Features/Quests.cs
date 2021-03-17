using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.Interactive;
using EFT.Quests;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using EFT.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Quests : CachableMonoBehaviour<IEnumerable<QuestRecord>>
	{
		public static readonly Color QuestColor = Color.magenta;

		public override float CacheTimeInSec => 5f;
		public override bool Enabled { get; set; } = false;

		public override IEnumerable<QuestRecord> RefreshData()
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				yield break;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				yield break;

			var camera = Camera.main;
			if (camera == null)
				yield break;

			var profile = player!.Profile;
			var triggers = FindObjectsOfType<PlaceItemTrigger>();
			var source = profile.Inventory.AllPlayerItems.ToArray();

			foreach (var trigger in triggers)
			{
				foreach (var item in profile.Quests.GetConditionHandlersByZone<ConditionZone>(trigger.Id))
				{
					var conditionZone = (ConditionZone)item.Condition;
					var result = source.FirstOrDefault(x => conditionZone.target.Contains(x.TemplateId));
					if (result == null)
						continue;

					var isMultitool = result.TemplateId == KnownTemplateIds.MultiTool;
					var position = trigger.transform.position;
					yield return new QuestRecord
					{
						Name = isMultitool ? "Repair" : $"Place {result.Template.NameLocalizationKey.Localized()}",
						Position = position,
						ScreenPosition = camera.WorldPointToScreenPoint(position),
						Color = QuestColor
					};
					break;
				}
			}
		}

		public override void ProcessDataOnGUI(IEnumerable<QuestRecord> data)
		{
			var camera = Camera.main;
			if (camera == null)
				return;

			foreach (var quest in data)
			{
				var position = quest.Position;

				var screenPosition = camera.WorldPointToScreenPoint(position);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Math.Round(Vector3.Distance(camera.transform.position, position));
				var caption = $"{quest.Name} [{distance}m]";
				Render.DrawString(new Vector2(screenPosition.x - 50f, screenPosition.y), caption, quest.Color);
			}
		}
	}

	public struct QuestRecord
	{
		public string Name { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 ScreenPosition { get; set; }
		public Color Color { get; set; }
	}
}
