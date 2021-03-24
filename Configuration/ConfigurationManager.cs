using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EFT.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT.Trainer.Configuration
{
	public static class ConfigurationManager
	{
		public static JsonConverter[] Converters => new JsonConverter[]{new ColorConverter(), new KeyCodeConverter()};

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
				var properties = featureType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				foreach (var property in properties)
				{
					if (property.GetCustomAttribute<ConfigurationPropertyAttribute>(true) == null)
						continue;

					var key = $"{featureType.FullName}.{property.Name}=";
					var line = lines.FirstOrDefault(l => l.StartsWith(key));
					if (line == null)
						continue;

					var value = JsonConvert.DeserializeObject(line.Substring(key.Length), property.PropertyType, Converters);
					property.SetValue(feature, value);
				}
			}

			AddConsoleLog($"Loaded {filename}");
		}

		public static void Save(string filename, Component[] features)
		{
			var content = new StringBuilder();

			foreach (var feature in features.OrderBy(f => f.GetType().FullName))
			{
				var featureType = feature.GetType();
				var properties = featureType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

				bool matches = false;
				foreach (var property in properties.OrderBy(p => p.Name))
				{
					if (property.GetCustomAttribute<ConfigurationPropertyAttribute>(true) == null)
						continue;

					matches = true;

					var key = $"{featureType.FullName}.{property.Name}";
					var value = JsonConvert.SerializeObject(property.GetValue(feature), Formatting.None, Converters);

					content.AppendLine($"{key}={value}");
				}

				if (matches)
					content.AppendLine();
			}

			File.WriteAllText(filename, content.ToString());
			AddConsoleLog($"Saved {filename}");
		}
	}

	public class ColorConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var color = (Color)value;
			serializer.Serialize(writer, new[] {color.r, color.g, color.b, color.a});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var array = serializer.Deserialize<float[]>(reader);
			return new Color(array[0], array[1], array[2], array[3]);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Color);
		}
	}

	public class KeyCodeConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, Enum.GetName(typeof(KeyCode), value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return Enum.Parse(typeof(KeyCode), serializer.Deserialize<string>(reader));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(KeyCode);
		}
	}
}
