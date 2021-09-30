using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Configuration
{
	public class ColorConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is not Color color)
				return;

			serializer.Serialize(writer, new[] {color.r, color.g, color.b, color.a});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var array = serializer.Deserialize<float[]>(reader);
			return array is null ? Color.clear : new Color(array[0], array[1], array[2], array[3]);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Color);
		}

		public static Color? Parse(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			value = value
				.Trim()
				.ToLower();

			var colorType = typeof(Color);
			var field = colorType.GetProperty(value, BindingFlags.Static | BindingFlags.Public);
			if (field != null)
				return (Color)field.GetValue(null);

			try
			{
				return JsonConvert.DeserializeObject<Color>(value, new ColorConverter());
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string[] ColorNames()
		{
			var colorType = typeof(Color);
			return colorType
				.GetProperties(BindingFlags.Static | BindingFlags.Public)
				.Where(p => p.PropertyType == colorType)
				.Select(p => p.Name)
				.ToArray();
		}
	}
}
