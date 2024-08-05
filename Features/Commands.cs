using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Trainer.Configuration;
using EFT.Trainer.ConsoleCommands;
using EFT.Trainer.Properties;
using EFT.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal class Commands : FeatureRenderer
{
	public override string Name => Strings.FeatureCommandsName;
	public override string Description => Strings.FeatureCommandsDescription;

	[ConfigurationProperty(Skip = true)] // we do not want to offer save/load support for this
	public override bool Enabled { get; set; } = false;

	[ConfigurationProperty]
	public override float X { get; set; } = DefaultX;

	[ConfigurationProperty]
	public override float Y { get; set; } = DefaultY;

	public override KeyCode Key { get; set; } = KeyCode.RightAlt;

	private bool Registered { get; set; } = false;
	private Dictionary<string, string> PropertyDisplays { get; } = new();

	protected override void Update()
	{
		if (Registered)
		{
			base.Update();
			return;
		}

		if (!PreloaderUI.Instantiated)
			return;

		RegisterPropertyDisplays();
		RegisterCommands();
	}

	private void RegisterPropertyDisplays()
	{
		const string prefix = nameof(OrderedProperty.Property);

		var properties = typeof(Strings)
				.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
			    .Where(p => p.Name.StartsWith(prefix));

		PropertyDisplays.Clear();

		foreach (var property in properties)
		{
			var key = property.Name.Substring(prefix.Length);
			var value = property.GetValue(null) as string ?? string.Empty;
			
			PropertyDisplays.Add(key, value);
		}
	}

	protected override string GetPropertyDisplay(string propertyName)
	{
		if (PropertyDisplays.TryGetValue(propertyName, out var value))
			return value;

		return $"!! [{propertyName}] !!"; // missing translation in Strings.resx
	}

	private void RegisterCommands()
	{
		foreach(var feature in Context.ToggleableFeatures.Value)
		{
			if (feature is Commands or GameState)
				continue;

			new ToggleFeatureCommand(feature)
				.Register();
		}

		// Dynamically register commands
		foreach (var command in GetCommands())
			command.Register();

		// built-in commands
		new BuiltInCommand(Strings.CommandLoadName, () => LoadSettings())
			.Register();

		new BuiltInCommand(Strings.CommandSaveName, SaveSettings)
			.Register();

		// Load default configuration
		LoadSettings(false);
		SetupWindowCoordinates();

		Registered = true;
	}

	private IEnumerable<ConsoleCommand> GetCommands()
	{
		var types = GetType()
			.Assembly
			.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(ConsoleCommand)) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

		foreach (var type in types)
			yield return (ConsoleCommand) Activator.CreateInstance(type);
	}
}
