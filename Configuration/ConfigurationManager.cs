using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EFT.Trainer.Features;
using EFT.UI;
using Newtonsoft.Json;

#nullable enable

namespace EFT.Trainer.Configuration
{
	internal static class ConfigurationManager
	{
		public static JsonConverter[] Converters => new JsonConverter[]{new TrackedItemConverter(), new ColorConverter(), new KeyCodeConverter()};

		private static void AddConsoleLog(string log, string from = "config")
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from);
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
							AddConsoleLog($"{key} seems corrupted in {filename}. Please fix.".Red());
						}
					}
				}

				AddConsoleLog($"Loaded {filename}");
			}
			catch (Exception ioe)
			{
				AddConsoleLog($"Unable to load {filename}. {ioe.Message}".Red());
			}
		}

		public static void LoadPropertyValue(string filename, Feature feature, string propertyName)
		{
			try
			{
				if (!File.Exists(filename))
				{
					AddConsoleLog($"{filename} not found!");
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
					AddConsoleLog($"{filename} seems corrupted. Please fix.".Red());
				}

				AddConsoleLog($"Loaded {filename}");
			}
			catch (Exception ioe)
			{
				AddConsoleLog($"Unable to load {filename}. {ioe.Message}".Red());
			}
		}

		public static void Save(string filename, Feature[] features)
		{
			try
			{
				var content = new StringBuilder();
				content.AppendLine("; Be careful when updating this file :)");
				content.AppendLine("; For keys, use https://docs.unity3d.com/ScriptReference/KeyCode.html");
				content.AppendLine("; Colors are stored as an array of 'RGBA' floats");
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
				AddConsoleLog($"Saved {filename}");
			}
			catch (Exception ioe)
			{
				AddConsoleLog($"Unable to save {filename}. {ioe.Message}".Red());
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

				AddConsoleLog($"Saved {filename}");
			}
			catch (Exception ioe)
			{
				AddConsoleLog($"Unable to save {filename}. {ioe.Message}".Red());
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

			return properties
				.Select(p => new {property = p, attribute = p.GetCustomAttribute<ConfigurationPropertyAttribute>(true)})
				.Where(p => p.attribute is {Skip: false})
				.Select(op => new OrderedProperty(op.attribute, op.property))
				.OrderBy(op => op.Attribute.Order)
				.ThenBy(op => op.Property.Name)
				.ToArray();
		}
	}
}
