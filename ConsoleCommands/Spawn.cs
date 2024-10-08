﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Utils;
using EFT.CameraControl;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class Spawn : BaseTemplateCommand
{
	public override string Name => Strings.CommandSpawn;

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not { Success: true })
			return;

		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return;

		var search = matchGroup.Value;
		var templates = FindTemplates(search);

		switch (templates.Length)
		{
			case 0:
				AddConsoleLog(Strings.ErrorNoTemplateFound.Red());
				return;
			case > 1:
				foreach (var template in templates)
					AddConsoleLog(string.Format(Strings.CommandTemplateEnumerateFormat, template._id, template.ShortNameLocalizationKey.Localized().Green(), template.NameLocalizationKey.Localized()));

				AddConsoleLog(string.Format(Strings.ErrorTooManyTemplatesFormat, templates.Length.ToString().Cyan()));
				return;
		}

		var tpl = templates[0];
		SpawnTemplate(tpl, player, this);
	}

	internal static void SpawnTemplate(string template, Player player, ConsoleCommand command, Func<ItemTemplate, bool> filter)
	{
		var result = FindTemplates(template)
			.FirstOrDefault(filter);

		if (result == null)
			return;

		SpawnTemplate(result, player, command);
	}

	private static void SpawnTemplate(ItemTemplate template, Player player, ConsoleCommand command)
	{
		var poolManager = Singleton<PoolManager>.Instance;

		poolManager
			.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [.. template.AllResources], JobPriority.Immediate)
			.ContinueWith(task =>
			{
				AsyncWorker.RunInMainTread(delegate
				{
					if (task.IsFaulted)
					{
						command.AddConsoleLog(Strings.ErrorFailedToLoadItemBundle.Red());
					}
					else
					{
						var itemFactory = Singleton<ItemFactory>.Instance;
						var item = itemFactory.CreateItem(MongoID.Generate(), template._id, null);
						if (item == null)
						{
							command.AddConsoleLog(Strings.ErrorFailedToCreateItem.Red());
						}
						else
						{
							item.SpawnedInSession = true; // found in raid

							_ = new TraderControllerClass(item, item.Id, item.ShortName);
							var go = poolManager.CreateLootPrefab(item, ECameraType.Default);

							go.SetActive(value: true);
							var lootItem = Singleton<GameWorld>.Instance.CreateLootWithRigidbody(go, item, item.ShortName, Singleton<GameWorld>.Instance, randomRotation: false, null, out _);

							var transform = player.Transform;
							var position = transform.position
										   + transform.right * Random.Range(-1f, 1f)
										   + transform.forward * 2f
										   + transform.up * 0.5f;

							lootItem.transform.SetPositionAndRotation(position, transform.rotation);
							lootItem.LastOwner = player;
						}
					}
				});

				return Task.CompletedTask;
			});
	}
}
