using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Comfort.Common;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.UI;
using JsonType;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace EFT.Trainer.Features
{
	public class Commands : ToggleMonoBehaviour
	{
		[ConfigurationProperty(Skip = true)] // we do not want to offer save/load support for this
		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public virtual float X { get; set; } = 40f;

		[ConfigurationProperty]
		public virtual float Y { get; set; } = 20f;

		public override KeyCode Key { get; set; } = KeyCode.RightAlt;

		public bool Registered { get; set; } = false;
		public const string ValueGroup = "value";
		public const string ExtraGroup = "extra";
		public string UserPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov");
		private readonly Dictionary<string, Type> _features = new()
		{
			{"autogun", typeof(AutomaticGun)},
			{"crosshair", typeof(CrossHair)},
			{"exfil", typeof(ExfiltrationPoints)},
			{"grenade", typeof(Grenades)},
			{"hud", typeof(Hud)},
			{"loot", typeof(LootItems)},
			{"night", typeof(NightVision)},
			{"nocoll", typeof(NoCollision)},
			{"norecoil", typeof(NoRecoil)},
			{"novisor", typeof(NoVisor)},
			{"quest", typeof(Quests)},
			{"stamina", typeof(Stamina)},
			{"stash", typeof(LootableContainers)},
			{"thermal", typeof(ThermalVision)},
			{"wallhack", typeof(Players)},
			{"wallshoot", typeof(WallShoot)},
		};

		protected override void Update()
		{
			if (Registered)
			{
				base.Update();
				return;
			}

			if (!PreloaderUI.Instantiated)
				return;

			RegisterCommands();
		}

		private Rect _clientWindowRect;
		protected override void OnGUIWhenEnabled()
		{
			_clientWindowRect = new Rect(X, Y, _clientWindowRect.width, _clientWindowRect.height);
			_clientWindowRect = GUILayout.Window(0, _clientWindowRect, RenderFeatureWindow, "EFT Trainer", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			X = _clientWindowRect.x;
			Y = _clientWindowRect.y;
		}

		private void RenderFeatureWindow(int id)
		{
			GUILayout.BeginVertical();
			foreach (var (featureName, featureType) in _features.OrderBy(kvp => kvp.Key))
			{
				if (Loader.HookObject.GetComponent(featureType) is not ToggleMonoBehaviour feature)
					continue;

				var toggleText = " " + GetFeatureHelpText(feature, featureName);
				feature.Enabled = GUILayout.Toggle(feature.Enabled, toggleText);
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		private void RegisterCommands()
		{
			var commands = ConsoleScreen.Commands;
			if (commands.Count == 0)
				return;

			foreach (var (featureName, featureType) in _features.OrderByDescending(kvp => kvp.Key))
			{
				CreateCommand(commands, $"{featureName} (?<{ValueGroup}>(on)|(off))", m => OnTriggerFeature(featureType, m));
			}

			CreateCommand(commands, "dump", _ => Dump());
			CreateCommand(commands, "status", _ => Status());

			var feature = Loader.HookObject.GetComponent<LootItems>();
			if (feature != null)
			{
				CreateCommand(commands, $"list {{0}}( (?<{ValueGroup}>.*))?", m => ListLootItems(m, feature));
				CreateCommand(commands, $"listr {{0}}( (?<{ValueGroup}>.*))?", m => ListLootItems(m, feature, ELootRarity.Rare));
				CreateCommand(commands, $"listsr {{0}}( (?<{ValueGroup}>.*))?", m => ListLootItems(m, feature, ELootRarity.Superrare));

				CreateCommand(commands, $"track (?<value>.+?)(?<extra> ({string.Join("|", ColorConverter.ColorNames())}|\\[[\\.,\\d ]*\\]{{1}}))?", m => TrackLootItem(m, feature));
				CreateCommand(commands, $"untrack (?<{ValueGroup}>.+)", m => UnTrackLootItem(m, feature));
				CreateCommand(commands, "tracklist", _ => ShowTrackList(feature));
			}

			var configFile = Path.Combine(UserPath, "trainer.ini");
			var features = Loader.HookObject.GetComponents(typeof(MonoBehaviour));
			CreateCommand(commands, "load", _ => ConfigurationManager.Load(configFile, features));
			CreateCommand(commands, "save", _ => ConfigurationManager.Save(configFile, features));

			// Load default configuration
			ConfigurationManager.Load(configFile, features, false);

			Registered = true;
		}

		private static void CreateCommand(IList commands, string regex, Action<Match> match)
		{
			// 'commands' field is a List<?> where ? is an obfuscated type, distinct for every EFT build
			// so use reflection instead of breaking the build every time
			// and use the non generic IList interface to add a new item
			var listType = commands.GetType();
			var commandType = listType.GetGenericArguments().FirstOrDefault();
			if (commandType == null)
				return;

			var command = Activator.CreateInstance(commandType, regex, match);
			if (command == null)
				return;

			commands.Add(command);
		}

		private static void ShowTrackList(LootItems feature, bool changed = false)
		{
			if (changed)
				AddConsoleLog("Tracking list updated...", "tracker");

			foreach (var item in feature.TrackedNames)
			{
				if (item.Color.HasValue)
					AddConsoleLog($"Tracking: {item.Name.Color(item.Color.Value)}", "tracker");
				else
					AddConsoleLog($"Tracking: {item.Name}", "tracker");
			}
		}

		private static void UnTrackLootItem(Match match, LootItems feature)
		{
			var matchGroup = match.Groups[ValueGroup];
			if (matchGroup is not {Success: true})
				return;

			ShowTrackList(feature, feature.UnTrack(matchGroup.Value));
		}

		private static void TrackLootItem(Match match, LootItems feature)
		{
			var matchGroup = match.Groups[ValueGroup];
			if (matchGroup is not {Success: true})
				return;

			Color? color = null;
			var extraGroup = match.Groups[ExtraGroup];
			if (extraGroup is {Success: true})
				color = ColorConverter.Parse(extraGroup.Value);

			ShowTrackList(feature, feature.Track(matchGroup.Value, color));
		}

		private static void ListLootItems(Match match, LootItems feature, ELootRarity rarityFilter = ELootRarity.Not_exist)
		{
			var search = string.Empty;
			var matchGroup = match.Groups[ValueGroup];
			if (matchGroup is {Success: true})
				search = matchGroup.Value.Trim();

			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return;

			var itemsPerName = new Dictionary<string, List<Item>>();

			// Step 1 - look outside containers and inside corpses (loot items)
			FindLootItems(world, itemsPerName, feature);

			// Step 2 - look inside containers (items)
			if (feature.SearchInsideContainers)
				FindItemsInContainers(itemsPerName);

			var names = itemsPerName.Keys.ToList();
			names.Sort();
			names.Reverse();

			var count = 0;
			foreach (var itemName in names)
			{
				if (itemName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
					continue;

				var list = itemsPerName[itemName];
				var rarity = list.First().Template.Rarity;
				if (rarityFilter != ELootRarity.Not_exist && rarityFilter != rarity)
					continue;

				var extra = rarity != ELootRarity.Not_exist ? $" ({rarity.Color()})" : string.Empty;
				AddConsoleLog($"{itemName} [{list.Count.ToString().Cyan()}]{extra}", "list");

				count += list.Count;
			}

			AddConsoleLog("------", "list");
			AddConsoleLog($"found {count.ToString().Cyan()} items", "list");
		}

		private static void FindItemsInContainers(Dictionary<string, List<Item>> itemsPerName)
		{
			var containers = FindObjectsOfType<LootableContainer>();
			foreach (var container in containers)
			{
				if (!container.IsValid())
					continue;

				FindItemsInRootItem(itemsPerName, container.ItemOwner?.RootItem);
			}
		}

		private static void FindItemsInRootItem(Dictionary<string, List<Item>> itemsPerName, Item? rootItem)
		{
			var items = rootItem?
				.GetAllItems()?
				.ToArray();

			if (items == null)
				return;

			IndexItems(items, itemsPerName);
		}

		private static void FindLootItems(GameWorld world, Dictionary<string, List<Item>> itemsPerName, LootItems feature)
		{
			var lootItems = world.LootItems;
			var filteredItems = new List<Item>();
			for (var i = 0; i < lootItems.Count; i++)
			{
				var lootItem = lootItems.GetByIndex(i);
				if (!lootItem.IsValid())
					continue;

				if (lootItem is Corpse corpse)
				{
					if (feature.SearchInsideCorpses)
						FindItemsInRootItem(itemsPerName, corpse.ItemOwner?.RootItem);

					continue;
				}

				filteredItems.Add(lootItem.Item);
			}

			IndexItems(filteredItems, itemsPerName);
		}

		private static void IndexItems(IEnumerable<Item> items, Dictionary<string, List<Item>> itemsPerName)
		{
			foreach (var item in items)
			{
				if (!item.IsValid())
					continue;

				if (item.IsFiltered())
					continue;

				var itemName = item.ShortName.Localized();
				if (!itemsPerName.TryGetValue(itemName, out var pnList))
				{
					pnList = new List<Item>();
					itemsPerName[itemName] = pnList;
				}

				pnList.Add(item);
			}
		}

		private static string GetFeatureHelpText(ToggleMonoBehaviour feature, string featureName)
		{
			var toggleKey = feature.Key != KeyCode.None ? $" ({feature.Key} to toggle)" : string.Empty;
			return $"{featureName} is {(feature.Enabled ? "on".Green() : "off".Red())}{toggleKey}";
		}

		private void Status()
		{
			foreach (var (featureName, featureType) in _features.OrderByDescending(kvp => kvp.Key))
			{
				if (Loader.HookObject.GetComponent(featureType) is not ToggleMonoBehaviour feature)
				{
					AddConsoleLog($"{featureName} is not loaded!".Red(), "status");
					continue;
				}

				AddConsoleLog(GetFeatureHelpText(feature, featureName), "status");
			}
		}

		private void Dump()
		{
			var dumpfolder = Path.Combine(UserPath, "Dumps");
			var thisDump = Path.Combine(dumpfolder, $"{DateTime.Now:yyyyMMdd-HHmmss}");

			Directory.CreateDirectory(thisDump);

			AddConsoleLog("Dumping scenes...", "dump");
			for (int i = 0; i < SceneManager.sceneCount; i++) 
			{
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded)
					continue;

				var json = SceneDumper.DumpScene(scene).ToPrettyJson();
				File.WriteAllText(Path.Combine(thisDump, GetSafeFilename($"@scene - {scene.name}.txt")), json);
			}

			AddConsoleLog("Dumping game objects...", "dump");
			foreach (var go in FindObjectsOfType<GameObject>())
			{
				if (go == null || go.transform.parent != null || !go.activeSelf) 
					continue;

				var filename = GetSafeFilename(go.name + "-" + go.GetHashCode() + ".txt");
				var json = SceneDumper.DumpGameObject(go).ToPrettyJson();
				File.WriteAllText(Path.Combine(thisDump, filename), json);
			}

			AddConsoleLog($"Dump created in {thisDump}", "dump");
		}

		private static string GetSafeFilename(string filename)
		{
			return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));  	
		}

		public void OnTriggerFeature(Type featureType, Match match)
		{
			var matchGroup = match.Groups[ValueGroup];
			if (matchGroup is not {Success: true})
				return;

			if (Loader.HookObject.GetComponent(featureType) is not ToggleMonoBehaviour feature)
				return;

			feature.Enabled = matchGroup.Value switch
			{
				"on" => true,
				"off" => false,
				_ => feature.Enabled
			};
		}

		private static void AddConsoleLog(string log, string from = "scheduler")
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from);
		}
	}
}
