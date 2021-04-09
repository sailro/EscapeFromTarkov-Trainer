using System;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Configuration
{
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
}
