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
			value ??= KeyCode.None;
			serializer.Serialize(writer, Enum.GetName(typeof(KeyCode), value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var value = serializer.Deserialize<string>(reader);
			return value == null ? KeyCode.None : Enum.Parse(typeof(KeyCode), value);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(KeyCode);
		}
	}
}
