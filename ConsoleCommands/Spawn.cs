using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Utils;
using EFT.CameraControl;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

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
		var templates = FindTemplates(search).ToArray();

		switch (templates.Length)
		{
			case 0:
				AddConsoleLog(Strings.ErrorNoTemplateFound.Red());
				return;
			case > 1:
			{
				foreach (var template in templates)
					AddConsoleLog(string.Format(Strings.CommandTemplateEnumerateFormat, template._id, template.ShortNameLocalizationKey.Localized().Green(), template.NameLocalizationKey.Localized()));

				AddConsoleLog(string.Format(Strings.ErrorTooManyTemplatesFormat, templates.Length.ToString().Cyan()));
				return;
			}
		}

		var tpl = templates[0];
		var poolManager = Singleton<PoolManager>.Instance;

		poolManager
			.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [..tpl.AllResources], JobPriority.Immediate)
			.ContinueWith(task =>
			{
				AsyncWorker.RunInMainTread(delegate
				{
					if (task.IsFaulted)
					{
						AddConsoleLog(Strings.ErrorFailedToLoadItemBundle.Red());
					}
					else
					{
						var itemFactory = Singleton<ItemFactory>.Instance;
						var item = itemFactory.CreateItem(MongoID.Generate(), tpl._id, null);
						if (item == null)
						{
							AddConsoleLog(Strings.ErrorFailedToCreateItem.Red());
						}
						else
						{
							item.SpawnedInSession = true; // found in raid

							_ = new TraderControllerClass(item, item.Id, item.ShortName);
							var go = poolManager.CreateLootPrefab(item, ECameraType.Default);

							go.SetActive(value: true);
							var lootItem = Singleton<GameWorld>.Instance.CreateLootWithRigidbody(go, item, item.ShortName, Singleton<GameWorld>.Instance, randomRotation: false, null, out _);
							lootItem.transform.SetPositionAndRotation(player.Transform.position + player.Transform.forward * 2f + player.Transform.up * 0.5f, player.Transform.rotation);
							lootItem.LastOwner = player;
						}
					}
				});

				return Task.CompletedTask;
			});
	}
}
