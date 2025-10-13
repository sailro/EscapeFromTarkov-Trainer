using System;
using Newtonsoft.Json;

#nullable enable

namespace EFT.Trainer.Configuration;

public class EnumConverter<T> : JsonConverter where T : Enum
{
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		value ??= default(T)!;
		serializer.Serialize(writer, Enum.GetName(typeof(T), value));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var value = serializer.Deserialize<string>(reader);
		return value == null ? default(T)! : Enum.Parse(typeof(T), value);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(T);
	}
}
