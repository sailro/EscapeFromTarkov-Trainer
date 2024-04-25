using System;
using System.Collections.Generic;
using System.Linq;
using EFT.Trainer.Configuration;
using EFT.Trainer.ConsoleCommands;
using EFT.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal class Commands : FeatureRenderer
{
	public override string Name => "commands";
	public override string Description => "This main popup window.";

	[ConfigurationProperty(Skip = true)] // we do not want to offer save/load support for this
	public override bool Enabled { get; set; } = false;

	[ConfigurationProperty]
	public override float X { get; set; } = DefaultX;

	[ConfigurationProperty]
	public override float Y { get; set; } = DefaultY;

	public override KeyCode Key { get; set; } = KeyCode.RightAlt;

	private bool Registered { get; set; } = false;

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
		new BuiltInCommand("load", () => LoadSettings())
			.Register();

		new BuiltInCommand("save", SaveSettings)
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
