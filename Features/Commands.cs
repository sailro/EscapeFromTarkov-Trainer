using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.CameraControl;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using EFT.UI;
using JsonType;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace EFT.Trainer.Features;

internal class Commands : ToggleFeature
{
	public override string Name => "commands";
	public override string Description => "This main popup window.";

	[ConfigurationProperty(Skip = true)] // we do not want to offer save/load support for this
	public override bool Enabled { get; set; } = false;

	[ConfigurationProperty]
	public virtual float X { get; set; } = DefaultX;

	[ConfigurationProperty]
	public virtual float Y { get; set; } = DefaultY;

	public override KeyCode Key { get; set; } = KeyCode.RightAlt;

	private bool Registered { get; set; } = false;
	private const string ValueGroup = "value";
	private const string ExtraGroup = "extra";
	private const float DefaultX = 40f;
	private const float DefaultY = 20f;

	private static string UserPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov");
	private static string ConfigFile => Path.Combine(UserPath, "trainer.ini");

	private static Lazy<Feature[]> Features => new(() => [.. FeatureFactory.GetAllFeatures().OrderBy(f => f.Name)]);
	private static Lazy<ToggleFeature[]> ToggleableFeatures => new(() => [.. FeatureFactory.GetAllToggleableFeatures().OrderByDescending(f => f.Name)]);

	private static GUIStyle LabelStyle => new() {wordWrap = false, normal = {textColor = Color.white}, margin = new RectOffset(8,0,8,0), fixedWidth = 150f, stretchWidth = false};
	private static GUIStyle DescriptionStyle => new() {wordWrap = true, normal = {textColor = Color.white}, margin = new RectOffset(8,0,8,0), stretchWidth = true};
	private static GUIStyle BoxStyle => new(GUI.skin.box) {normal = {background = Texture2D.whiteTexture, textColor = Color.white}};

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

	internal abstract class SelectionContext<T>
	{
		protected SelectionContext(IFeature feature, OrderedProperty orderedProperty, float parentX, float parentY, Func<T, Picker<T>> builder)
		{
			Feature = feature;
			OrderedProperty = orderedProperty;
			Picker = builder((T)orderedProperty.Property.GetValue(feature));
				
			var position = Event.current.mousePosition;
			Picker.SetWindowPosition(parentX + LabelStyle.fixedWidth * 3 + LabelStyle.margin.left * 6, position.y + parentY - 32f);
		}

		public IFeature Feature { get; }
		public OrderedProperty OrderedProperty { get; }
		public Picker<T> Picker { get; }
		public abstract int Id { get; }
	}

	internal class ColorSelectionContext(IFeature feature, OrderedProperty orderedProperty, float parentX, float parentY) : SelectionContext<Color>(feature, orderedProperty, parentX, parentY, color => new ColorPicker(color))
	{
		public override int Id => 1;
	}

	internal class KeyCodeSelectionContext(IFeature feature, OrderedProperty orderedProperty, float parentX, float parentY) : SelectionContext<KeyCode>(feature, orderedProperty, parentX, parentY, color => new EnumPicker<KeyCode>(color))
	{
		public override int Id => 2;
	}

	private Rect _clientWindowRect;
	private ColorSelectionContext? _colorSelectionContext = null;
	private KeyCodeSelectionContext? _keyCodeSelectionContext = null;
	protected override void OnGUIWhenEnabled()
	{
		_clientWindowRect = new Rect(X, Y, 490, _clientWindowRect.height);
		_clientWindowRect = GUILayout.Window(0, _clientWindowRect, RenderFeatureWindow, "EFT Trainer", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
		X = _clientWindowRect.x;
		Y = _clientWindowRect.y;

		HandleSelectionContext(_colorSelectionContext);

		if (HandleSelectionContext(_keyCodeSelectionContext))
			_keyCodeSelectionContext = null;
	}

	private static bool HandleSelectionContext<T>(SelectionContext<T>? context)
	{
		if (context == null) 
			return false;

		var property = context.OrderedProperty.Property;
		var picker = context.Picker;

		picker.DrawWindow(context.Id, property.Name);
		property.SetValue(context.Feature, picker.Value);

		return picker.IsSelected;
	}

	private int _selectedTabIndex = 0;
	private void RenderFeatureWindow(int id)
	{
		var fixedTabs = new[] {"[summary]"};

		var tabs = fixedTabs
			.Concat
			(
				Features
					.Value
					.Select(RenderFeatureText)
			)
			.ToArray();

		var style = new GUIStyle {wordWrap = false, normal = {textColor = Color.white}, alignment = TextAnchor.UpperLeft, fixedHeight = 1, stretchHeight = true};

		GUILayout.BeginHorizontal();
		var lastIndex = _selectedTabIndex;
		_selectedTabIndex = GUILayout.SelectionGrid(_selectedTabIndex, tabs, 1, GUILayout.Width(LabelStyle.fixedWidth));

		if (lastIndex != _selectedTabIndex)
		{
			_colorSelectionContext = null;
			_keyCodeSelectionContext = null;
		}

		GUILayout.BeginVertical(style);
		GUILayout.Space(4);

		switch (_selectedTabIndex)
		{
			case 0:
				RenderSummary();
				break;
			default:
				var feature = Features.Value[_selectedTabIndex - fixedTabs.Length];
				RenderFeature(feature);

				break;

		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUI.DragWindow();
	}

	private static string RenderFeatureText(Feature feature)
	{
		if (feature is not ToggleFeature toggleFeature || ConfigurationManager.IsSkippedProperty(feature, nameof(Enabled)))
			return feature.Name;

		return $"{toggleFeature.Name} is {(toggleFeature.Enabled ? "on".Green() : "off".Red())}";
	}

	private void RenderSummary()
	{
		GUILayout.BeginVertical();

		GUILayout.Label("<i><b>Welcome to EFT Trainer !</b></i>\n", DescriptionStyle);

		if (GUILayout.Button("Load settings"))
			LoadSettings();

		if (GUILayout.Button("Save settings"))
			SaveSettings();

		GUILayout.EndVertical();
	}

	private static void SaveSettings()
	{
		ConfigurationManager.Save(ConfigFile, Features.Value);
	}

	private void LoadSettings(bool warnIfNotExists = true)
	{
		var cx = X;
		var cy = Y;

		ConfigurationManager.Load(ConfigFile, Features.Value, warnIfNotExists);
		_controlValues.Clear();

		if (!Enabled)
			return;

		X = cx;
		Y = cy;
	}

	private void RenderFeature(Feature feature)
	{
		var orderedProperties = ConfigurationManager.GetOrderedProperties(feature.GetType());

		GUILayout.BeginVertical();

		GUILayout.Label($"<i><b>{feature.Description}</b></i>\n", DescriptionStyle);

		foreach (var property in orderedProperties)
			RenderFeatureProperty(feature, property);

		GUILayout.EndVertical();
	}

	private static readonly Dictionary<string, string> _controlValues = [];
	private void RenderFeatureProperty(Feature feature, OrderedProperty orderedProperty)
	{
		if (!orderedProperty.Attribute.Browsable)
			return;

		var property = orderedProperty.Property;

		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();

		GUILayout.Label(property.Name, LabelStyle);
		GUILayout.FlexibleSpace();

		var currentValue = property.GetValue(feature);
		var currentBackgroundColor = GUI.backgroundColor;

		if (currentValue == null)
			return;
			
		var width = GUILayout.Width(LabelStyle.fixedWidth);
		var newValue = RenderFeaturePropertyAsUIComponent(feature, orderedProperty, currentValue, width);

		if (currentValue != newValue)
			property.SetValue(feature, newValue);

		var focused = GUI.GetNameOfFocusedControl();

		if (ShouldResetSelectionContext(focused, _colorSelectionContext))
			_colorSelectionContext = null;

		if (ShouldResetSelectionContext(focused, _keyCodeSelectionContext))
			_keyCodeSelectionContext = null;

		GUI.backgroundColor = currentBackgroundColor;
		GUILayout.EndHorizontal();
	}

	private object RenderFeaturePropertyAsUIComponent(IFeature feature, OrderedProperty orderedProperty, object currentValue, GUILayoutOption width)
	{
		var property = orderedProperty.Property;
		var propertyType = property.PropertyType;

		var newValue = currentValue;
		var controlName = $"{feature.Name}.{property.Name}-{propertyType.Name}";
		GUI.SetNextControlName(controlName);

		switch (propertyType.Name)
		{
			case nameof(Boolean):
				newValue = RenderBooleanProperty(currentValue, width);
				break;

			case nameof(KeyCode):
				RenderKeyCodeProperty(currentValue, controlName, feature, orderedProperty, width);
				break;

			case nameof(Single):
				newValue = RenderFloatProperty(currentValue, controlName, width);
				break;

			case nameof(Int32):
				newValue = RenderIntProperty(currentValue, width);
				break;

			case nameof(Color):
				RenderColorProperty(currentValue, controlName, feature, orderedProperty, width);
				break;

			case nameof(String):
				newValue = RenderStringProperty(currentValue, width);
				break;

			default:
				// Look for inner properties
				if (currentValue is IFeature subFeature)
				{
					var subProperties = ConfigurationManager.GetOrderedProperties(propertyType);
					var length = subProperties.Length;

					if (length > 0)
					{
						width = GUILayout.Width(LabelStyle.fixedWidth / length - length);

						foreach (var innerOrderedProperty in subProperties)
						{
							var innerProperty = innerOrderedProperty.Property;
							var innerPropertyValue = innerProperty.GetValue(subFeature);
							RenderFeaturePropertyAsUIComponent(subFeature, innerOrderedProperty, innerPropertyValue, width);
						}

						break;
					}

				}

				GUILayout.Label($"Unsupported type: {propertyType.FullName}");
				break;
		}

		return newValue;
	}

	private static bool ShouldResetSelectionContext<T>(string focused, SelectionContext<T>? context)
	{
		return !string.IsNullOrEmpty(focused)
		       && !focused.EndsWith($"-{typeof(T).Name}")
		       && context != null;
	}

	private static object RenderIntProperty(object currentValue, GUILayoutOption option)
	{
		object newValue = currentValue;

		if (int.TryParse(GUILayout.TextField(currentValue.ToString(), option), out var intValue))
			newValue = intValue;

		return newValue;
	}

	private static object RenderStringProperty(object currentValue, GUILayoutOption width)
	{
		return GUILayout.TextField(currentValue.ToString(), width);
	}

	private void RenderKeyCodeProperty(object currentValue, string controlName, IFeature feature, OrderedProperty orderedProperty, GUILayoutOption option)
	{
		if (!GUILayout.Button(currentValue.ToString(), option))
			return;

		_keyCodeSelectionContext = new KeyCodeSelectionContext(feature, orderedProperty, X, Y);
		GUI.FocusControl(controlName);
	}

	private void RenderColorProperty(object currentValue, string controlName, IFeature feature, OrderedProperty orderedProperty, GUILayoutOption option)
	{
		GUI.backgroundColor = (Color) currentValue;

		if (!GUILayout.Button(string.Empty, BoxStyle, option, GUILayout.Height(22f)))
			return;

		_colorSelectionContext = new ColorSelectionContext(feature, orderedProperty, X, Y);
		GUI.FocusControl(controlName);
	}

	private static object RenderFloatProperty(object currentValue, string controlName, GUILayoutOption width)
	{
		const string decimalSeparator = ".";
		const string altDecimalSeparator = ",";

		var culture = CultureInfo.InvariantCulture;
		var newValue = currentValue;

		if (!_controlValues.TryGetValue(controlName, out var controlText))
			controlText = currentValue.ToString();

		if (controlText != currentValue.ToString())
			GUI.backgroundColor = Color.red;

		controlText = GUILayout
			.TextField(controlText, width)
			.Replace(altDecimalSeparator, decimalSeparator);

		if (!controlText.EndsWith(decimalSeparator) && float.TryParse(controlText, NumberStyles.Float, culture, out var floatValue))
		{
			newValue = floatValue;
			controlText = newValue.ToString();
		}

		_controlValues[controlName] = controlText;
		return newValue;
	}

	private object RenderBooleanProperty(object currentValue, GUILayoutOption option)
	{
		var boolValue = (bool) currentValue;
		var newValue = GUILayout.Toggle(boolValue, string.Empty, option);
		if (newValue != boolValue)
		{
			_colorSelectionContext = null;
			_keyCodeSelectionContext = null;
		}

		return newValue;
	}

	private void RegisterCommands()
	{
		foreach(var feature in ToggleableFeatures.Value)
		{
			if (feature is Commands or GameState)
				continue;

			CreateCommand($"{feature.Name}", $"(?<{ValueGroup}>(on)|(off))", m => OnToggleFeature(feature, m));

			if (feature is not LootItems liFeature) 
				continue;

			CreateCommand("list", $"(?<{ValueGroup}>.*)", m => ListLootItems(m, liFeature));
			CreateCommand("listr", $"(?<{ValueGroup}>.*)", m => ListLootItems(m, liFeature, ELootRarity.Rare));
			CreateCommand("listsr", $"(?<{ValueGroup}>.*)", m => ListLootItems(m, liFeature, ELootRarity.Superrare));

			var colorNames = string.Join("|", ColorConverter.ColorNames());
			CreateCommand("track", $"(?<value>.+?)(?<extra> ({colorNames}|\\[[\\.,\\d ]*\\]{{1}}))?", m => TrackLootItem(m, liFeature));
			CreateCommand("trackr", $"(?<value>.+?)(?<extra> ({colorNames}|\\[[\\.,\\d ]*\\]{{1}}))?", m => TrackLootItem(m, liFeature, ELootRarity.Rare));
			CreateCommand("tracksr", $"(?<value>.+?)(?<extra> ({colorNames}|\\[[\\.,\\d ]*\\]{{1}}))?", m => TrackLootItem(m, liFeature, ELootRarity.Superrare));

			CreateCommand("untrack", $"(?<{ValueGroup}>.+)", m => UnTrackLootItem(m, liFeature));
			CreateCommand("loadtl", $"(?<{ValueGroup}>.+)", m => LoadTrackList(m, liFeature));
			CreateCommand("savetl", $"(?<{ValueGroup}>.+)", m => SaveTrackList(m, liFeature));

			CreateCommand("tracklist", () => ShowTrackList(liFeature));
		}

		CreateCommand("dump", Dump);
		CreateCommand("status", Status);

		CreateCommand("load", () => LoadSettings());
		CreateCommand("save", SaveSettings);

		CreateCommand("spawn", $"(?<{ValueGroup}>.+)", SpawnItem);
		CreateCommand("template", $"(?<{ValueGroup}>.+)", FindTemplates);

		// Load default configuration
		LoadSettings(false);
		SetupWindowCoordinates();

		Registered = true;
	}

	private static IEnumerable<ItemTemplate> FindTemplates(string searchShortNameOrTemplateId)
	{
		if (!Singleton<ItemFactory>.Instantiated)
			return [];

		var templates = Singleton<ItemFactory>
			.Instance
			.ItemTemplates;

		// Match by TemplateId
		if (templates.TryGetValue(searchShortNameOrTemplateId, out var template))
			return [template];

		// Match by short name(s)
		return templates
			.Values
			.Where(t => t.ShortNameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0 
			            || t.NameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0);
	}

	private void FindTemplates(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		if (!Singleton<ItemFactory>.Instantiated)
			return;

		var search = matchGroup.Value;

		var templates = FindTemplates(search).ToArray();
		
		foreach (var template in templates)
			AddConsoleLog($"{template._id}: {template.ShortNameLocalizationKey.Localized().Green()} [{template.NameLocalizationKey.Localized()}]");

		AddConsoleLog("------");
		AddConsoleLog($"found {templates.Length.ToString().Cyan()} template(s)");
	}

	private void SpawnItem(Match match)
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
				AddConsoleLog("No template found!");
				return;
			case > 1:
			{
				foreach (var template in templates)
					AddConsoleLog($"{template._id}: {template.ShortNameLocalizationKey.Localized().Green()} [{template.NameLocalizationKey.Localized()}]");

				AddConsoleLog($"found {templates.Length.ToString().Cyan()} templates, be more specific");
				return;
			}
		}

		var tpl = templates[0];
		var poolManager = Singleton<PoolManager>.Instance;

		poolManager.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [..tpl.AllResources], JobPriority.Immediate).ContinueWith(delegate
		{
			var itemFactory = Singleton<ItemFactory>.Instance;
			var item = itemFactory.CreateItem(MongoID.Generate(), tpl._id, null);
			if (item == null)
			{
				AddConsoleLog("Failed to create item!");
				return Task.CompletedTask;
			}

			item.SpawnedInSession = true; // found in raid

			_ = new TraderControllerClass(item, item.Id, item.ShortName);
			var go = poolManager.CreateLootPrefab(item, ECameraType.Default);

			go.SetActive(value: true);
			var lootItem = Singleton<GameWorld>.Instance.CreateLootWithRigidbody(go, item, item.ShortName, Singleton<GameWorld>.Instance, randomRotation: false, null, out _);
			lootItem.transform.SetPositionAndRotation(player.Transform.position + player.Transform.forward * 2f + player.Transform.up * 0.5f, player.Transform.rotation);
			lootItem.LastOwner = player;

			return Task.CompletedTask;
		});
	}

	private void SetupWindowCoordinates()
	{
		bool needfix = false;
		X = FixCoordinate(X, Screen.width, DefaultX, ref needfix);
		Y = FixCoordinate(Y, Screen.height, DefaultY, ref needfix);

		if (needfix)
			SaveSettings();
	}

	private float FixCoordinate(float coord, float maxValue, float defaultValue, ref bool needfix)
	{
		if (coord < 0 || coord >= maxValue)
		{
			coord = defaultValue;
			needfix = true;
		}

		return coord;
	}

	private static void CreateCommand(string name, Action action)
	{
		ConsoleScreen.Processor.RegisterCommand(name, action);
	}

	private void CreateCommand(string cmdName, string pattern, Action<Match> action)
	{
#if DEBUG
		AddConsoleLog($"Registering {cmdName} command...");
#endif
		ConsoleScreen.Processor.RegisterCommand(cmdName, (string args) =>
		{
			var regex = new Regex("^" + pattern + "$");
			if (regex.IsMatch(args))
			{
				action(regex.Match(args));
			}
			else
			{
				ConsoleScreen.LogError("Invalid arguments");
			}
		});
	}

	private void ShowTrackList(LootItems feature, bool changed = false)
	{
		if (changed)
			AddConsoleLog("Tracking list updated...");

		foreach (var templateId in feature.Wishlist)
			AddConsoleLog($"Tracking: {templateId.LocalizedShortName()} (Wishlist)");

		foreach (var item in feature.TrackedNames)
		{
			var extra = item.Rarity.HasValue ? $" ({item.Rarity.Value.Color()})" : string.Empty;
			AddConsoleLog(item.Color.HasValue ? $"Tracking: {item.Name.Color(item.Color.Value)}{extra}" : $"Tracking: {item.Name}{extra}");
		}
	}

	private static bool TryGetTrackListFilename(Match match, [NotNullWhen(true)] out string? filename)
	{
		filename = null;

		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return false;

		filename = matchGroup.Value;

		if (!Path.IsPathRooted(filename))
			filename = Path.Combine(UserPath, filename);

		if (!Path.HasExtension(filename))
			filename += ".tl";

		return true;
	}

	private static void LoadTrackList(Match match, LootItems feature)
	{
		if (!TryGetTrackListFilename(match, out var filename))
			return;

		ConfigurationManager.LoadPropertyValue(filename, feature, nameof(LootItems.TrackedNames));
	}

	private static void SaveTrackList(Match match, LootItems feature)
	{
		if (!TryGetTrackListFilename(match, out var filename))
			return;

		ConfigurationManager.SavePropertyValue(filename, feature, nameof(LootItems.TrackedNames));
	}

	private void UnTrackLootItem(Match match, LootItems feature)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		ShowTrackList(feature, feature.UnTrack(matchGroup.Value));
	}

	private void TrackLootItem(Match match, LootItems feature, ELootRarity? rarity = null)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		Color? color = null;
		var extraGroup = match.Groups[ExtraGroup];
		if (extraGroup is {Success: true})
			color = ColorConverter.Parse(extraGroup.Value);

		ShowTrackList(feature, feature.Track(matchGroup.Value, color, rarity));
	}

	private void ListLootItems(Match match, LootItems feature, ELootRarity? rarityFilter = null)
	{
		var search = string.Empty;
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is {Success: true})
		{
			search = matchGroup.Value.Trim();
			if (search == TrackedItem.MatchAll)
				search = string.Empty;
		}

		var world = Singleton<GameWorld>.Instance;
		if (world == null)
			return;

		var itemsPerName = new Dictionary<string, List<Item>>();

		// Step 1 - look outside containers and inside corpses (loot items)
		FindLootItems(world, itemsPerName, feature);

		// Step 2 - look inside containers (items)
		if (feature.SearchInsideContainers)
			FindItemsInContainers(world, itemsPerName);

		var names = itemsPerName.Keys.ToList();
		names.Sort();
		names.Reverse();

		var count = 0;
		foreach (var itemName in names)
		{
			if (itemName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
				continue;

			var list = itemsPerName[itemName];
			var rarity = list.First().Template.GetEstimatedRarity();
			if (rarityFilter.HasValue && rarityFilter.Value != rarity)
				continue;

			var extra = rarity != ELootRarity.Not_exist ? $" ({rarity.Color()})" : string.Empty;
			AddConsoleLog($"{itemName} [{list.Count.ToString().Cyan()}]{extra}");

			count += list.Count;
		}

		AddConsoleLog("------");
		AddConsoleLog($"found {count.ToString().Cyan()} item(s)");
	}

	private static void FindItemsInContainers(GameWorld world, Dictionary<string, List<Item>> itemsPerName)
	{
		var owners = world.ItemOwners; // contains all containers: corpses, LootContainers, ...
		foreach (var owner in owners)
		{
			var rootItem = owner.Key.RootItem;
			if (rootItem is not { IsContainer: true })
				continue;

			if (!rootItem.IsValid() || rootItem.IsFiltered()) // filter default inventory container here, given we special case the corpse container
				continue;

			FindItemsInRootItem(itemsPerName, rootItem);
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
			if (!item.IsValid() || item.IsFiltered())
				continue;

			var itemName = item.ShortName.Localized();
			if (!itemsPerName.TryGetValue(itemName, out var pnList))
			{
				pnList = [];
				itemsPerName[itemName] = pnList;
			}

			pnList.Add(item);
		}
	}

	private static string GetFeatureHelpText(ToggleFeature feature)
	{
		var toggleKey = feature.Key != KeyCode.None ? $" ({feature.Key} to toggle)" : string.Empty;
		return $"{feature.Name} is {(feature.Enabled ? "on".Green() : "off".Red())}{toggleKey}";
	}

	private void Status()
	{
		foreach (var feature in ToggleableFeatures.Value)
		{
			if (feature is Commands or GameState)
				continue;

			AddConsoleLog(GetFeatureHelpText(feature));
		}
	}

	private void Dump()
	{
		var dumpfolder = Path.Combine(UserPath, "Dumps");
		var thisDump = Path.Combine(dumpfolder, $"{DateTime.Now:yyyyMMdd-HHmmss}");

		Directory.CreateDirectory(thisDump);

		AddConsoleLog("Dumping scenes...");
		for (int i = 0; i < SceneManager.sceneCount; i++) 
		{
			var scene = SceneManager.GetSceneAt(i);
			if (!scene.isLoaded)
				continue;

			var json = SceneDumper.DumpScene(scene).ToPrettyJson();
			File.WriteAllText(Path.Combine(thisDump, GetSafeFilename($"@scene - {scene.name}.txt")), json);
		}

		AddConsoleLog("Dumping game objects...");
		foreach (var go in FindObjectsOfType<GameObject>())
		{
			if (go == null || go.transform.parent != null || !go.activeSelf) 
				continue;

			var filename = GetSafeFilename(go.name + "-" + go.GetHashCode() + ".txt");
			var json = SceneDumper.DumpGameObject(go).ToPrettyJson();
			File.WriteAllText(Path.Combine(thisDump, filename), json);
		}

		AddConsoleLog($"Dump created in {thisDump}");
	}

	private static string GetSafeFilename(string filename)
	{
		return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));  	
	}

	public void OnToggleFeature(ToggleFeature feature, Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		feature.Enabled = matchGroup.Value switch
		{
			"on" => true,
			"off" => false,
			_ => feature.Enabled
		};
	}
}
