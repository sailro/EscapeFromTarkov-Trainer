using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using EFT.UI;
using Newtonsoft.Json;

#nullable enable

namespace EFT.Trainer.Configuration;

internal static class ConfigurationManager
{
	public static JsonConverter[] Converters => [new TrackedItemConverter(), new ColorConverter(), new KeyCodeConverter()];

	private static void AddConsoleLog(string log)
	{
		if (PreloaderUI.Instantiated)
			ConsoleScreen.Log(log);
	}

	public static void Load(string filename, Feature[] features, bool warnIfNotExists = true)
	{
		try
		{
			if (!File.Exists(filename))
			{
				if (warnIfNotExists)
					AddConsoleLog($"{filename} not found!");

				return;
			}

			var lines = File.ReadAllLines(filename);

			foreach (var feature in features)
			{
				var featureType = feature.GetType();
				var properties = GetOrderedProperties(featureType);

				foreach (var op in properties)
				{
					var key = $"{featureType.FullName}.{op.Property.Name}=";
					try
					{
						var line = lines.FirstOrDefault(l => l.StartsWith(key));
						if (line == null)
							continue;

						var value = JsonConvert.DeserializeObject(line.Substring(key.Length), op.Property.PropertyType, Converters);
						op.Property.SetValue(feature, value);
					}
					catch (JsonException)
					{
						AddConsoleLog(string.Format(Strings.ErrorCorruptedPropertyFormat, key, filename).Red());
					}
				}
			}

			AddConsoleLog(string.Format(Strings.CommandLoadSuccessFormat, filename));
		}
		catch (Exception ioe)
		{
			AddConsoleLog(string.Format(Strings.ErrorUnableToLoadFormat, filename, ioe.Message).Red());
		}
	}

	public static void LoadPropertyValue(string filename, Feature feature, string propertyName)
	{
		try
		{
			if (!File.Exists(filename))
			{
				AddConsoleLog(string.Format(Strings.ErrorFileNotFoundFormat, filename).Red());
				return;
			}

			var text = File.ReadAllText(filename);

			var tlProperty = GetOrderedProperties(feature.GetType())
				.First(p => p.Property.Name == propertyName);

			try
			{
				var value = JsonConvert.DeserializeObject(text, tlProperty.Property.PropertyType, Converters);
				tlProperty.Property.SetValue(feature, value);
			}
			catch (JsonException)
			{
				AddConsoleLog(string.Format(Strings.ErrorCorruptedFileFormat, filename).Red());
			}

			AddConsoleLog(string.Format(Strings.CommandLoadSuccessFormat, filename));
		}
		catch (Exception ioe)
		{
			AddConsoleLog(string.Format(Strings.ErrorUnableToLoadFormat, filename, ioe.Message).Red());
		}
	}

	public static void Save(string filename, Feature[] features)
	{
		try
		{
			var content = new StringBuilder();
			content.AppendLine(Strings.CommandSaveHeader);
			content.AppendLine();

			foreach (var feature in features.OrderBy(f => f.GetType().FullName))
			{
				var featureType = feature.GetType();
				var properties = GetOrderedProperties(featureType);

				foreach (var op in properties)
				{
					var key = $"{featureType.FullName}.{op.Property.Name}";
					var value = JsonConvert.SerializeObject(op.Property.GetValue(feature), Formatting.None, Converters);

					var comment = op.Attribute.Comment;
					if (!string.IsNullOrEmpty(comment)) 
						content.AppendLine($"; {comment}");

					content.AppendLine($"{key}={value}");
				}

				if (properties.Any())
					content.AppendLine();
			}

			File.WriteAllText(filename, content.ToString());
			AddConsoleLog(string.Format(Strings.CommandSaveSuccessFormat, filename));
		}
		catch (Exception ioe)
		{
			AddConsoleLog(string.Format(Strings.ErrorUnableToSaveFormat, filename, ioe.Message).Red());
		}
	}

	public static void SavePropertyValue(string filename, Feature feature, string propertyName)
	{
		try
		{
			var tlProperty = GetOrderedProperties(feature.GetType())
				.First(p => p.Property.Name == propertyName);

			var content = JsonConvert.SerializeObject(tlProperty.Property.GetValue(feature), Formatting.Indented, Converters);
			File.WriteAllText(filename, content);

			AddConsoleLog(string.Format(Strings.CommandSaveSuccessFormat, filename));
		}
		catch (Exception ioe)
		{
			AddConsoleLog(string.Format(Strings.ErrorUnableToSaveFormat, filename, ioe.Message).Red());
		}
	}

	public static bool IsSkippedProperty(Feature feature, string name)
	{
		return IsSkippedProperty(feature.GetType(), name);
	}

	public static bool IsSkippedProperty(Type featureType, string name)
	{
		var property = featureType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		if (property == null)
			return false;

		var attribute = property.GetCustomAttribute<ConfigurationPropertyAttribute>(true);
		return attribute is {Skip: true};
	}

	public static OrderedProperty[] GetOrderedProperties(Type featureType)
	{
		var properties = featureType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

		return
		[ .. properties
			.Select(p => new {property = p, attribute = p.GetCustomAttribute<ConfigurationPropertyAttribute>(true)})
			.Where(p => p.attribute is {Skip: false})
			.Select(op => new OrderedProperty(op.attribute, op.property))
			.OrderBy(op => op.Attribute.Order)
			.ThenBy(op => op.Property.Name)
		];
	}
}
