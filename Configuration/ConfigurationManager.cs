using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EFT.UI;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Configuration
{
	public static class ConfigurationManager
	{
		public static JsonConverter[] Converters => new JsonConverter[]{new TrackedItemConverter(), new ColorConverter(), new KeyCodeConverter()};

		private static void AddConsoleLog(string log, string from = "config")
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from);
		}

		public static void Load(string filename, Component[] features, bool warnIfNotExists = true)
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
					var line = lines.FirstOrDefault(l => l.StartsWith(key));
					if (line == null)
						continue;

					var value = JsonConvert.DeserializeObject(line.Substring(key.Length), op.Property.PropertyType, Converters);
					op.Property.SetValue(feature, value);
				}
			}

			AddConsoleLog($"Loaded {filename}");
		}

		public static void Save(string filename, Component[] features)
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

		private static OrderedProperty[] GetOrderedProperties(Type featureType)
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

		private class OrderedProperty
		{
			public ConfigurationPropertyAttribute Attribute { get; }
			public PropertyInfo Property { get; }

			public OrderedProperty(ConfigurationPropertyAttribute attribute, PropertyInfo property)
			{
				Attribute = attribute;
				Property = property;
			}
		}
	}
}
