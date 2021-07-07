using System;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Configuration
{
	public class KeyCodeConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, Enum.GetName(typeof(KeyCode), value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			return Enum.Parse(typeof(KeyCode), serializer.Deserialize<string>(reader));
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(KeyCode);
		}
	}
}
