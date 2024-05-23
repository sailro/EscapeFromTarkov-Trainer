using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EFT.InputSystem;
using EFT.Trainer.Configuration;
using EFT.Trainer.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal abstract class FeatureRenderer : ToggleFeature
{
	public abstract float X { get; set; }
	public abstract float Y { get; set; }

	protected const float DefaultX = 40f;
	protected const float DefaultY = 20f;

	private static GUIStyle LabelStyle => new() {wordWrap = false, normal = {textColor = Color.white}, margin = new RectOffset(8,0,8,0), fixedWidth = 150f, stretchWidth = false};
	private static GUIStyle DescriptionStyle => new() {wordWrap = true, normal = {textColor = Color.white}, margin = new RectOffset(8,0,8,0), stretchWidth = true};
	private static GUIStyle BoxStyle => new(GUI.skin.box) {normal = {background = Texture2D.whiteTexture, textColor = Color.white}};

	protected void SetupWindowCoordinates()
	{
		bool needfix = false;
		X = FixCoordinate(X, Screen.width, DefaultX, ref needfix);
		Y = FixCoordinate(Y, Screen.height, DefaultY, ref needfix);

		if (needfix)
			SaveSettings();
	}

	private static float FixCoordinate(float coord, float maxValue, float defaultValue, ref bool needfix)
	{
		if (coord < 0 || coord >= maxValue)
		{
			coord = defaultValue;
			needfix = true;
		}

		return coord;
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
		SetupInputNode();

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
				Context
					.Features
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
				var feature = Context.Features.Value[_selectedTabIndex - fixedTabs.Length];
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

	protected static void SaveSettings()
	{
		ConfigurationManager.Save(Context.ConfigFile, Context.Features.Value);
	}

	protected void LoadSettings(bool warnIfNotExists = true)
	{
		var cx = X;
		var cy = Y;

		ConfigurationManager.Load(Context.ConfigFile, Context.Features.Value, warnIfNotExists);
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

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		return command switch
		{
			// We do not want the player to shoot while clicking on a menu button
			ECommand.ToggleShooting when Enabled => ETranslateResult.BlockAll,
			_ => ETranslateResult.Ignore
		};
	}

	public override void TranslateAxes(ref float[] axes)
	{
		// this will disable the axes for player movement
		if (Enabled)
			axes = null!;
	}

	public override ECursorResult ShouldLockCursor()
	{
		return Enabled ? ECursorResult.ShowCursor : ECursorResult.Ignore;
	}

	private void SetupInputNode()
	{
		var player = GameState.Current?.LocalPlayer;
		if (player == null)
			return;

		if (!player.TryGetComponent<PlayerOwner>(out var owner))
			return;

		if (owner.InputTree.Contains(this))
			return;

		owner.InputTree.Add(this);
	}
}
