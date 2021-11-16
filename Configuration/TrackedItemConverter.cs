using System;
using EFT.Trainer.Features;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable enable

namespace EFT.Trainer.Configuration
{
	public class TrackedItemConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is not TrackedItem item)
				return;

			if (item.Color.HasValue | item.Rarity.HasValue)
				serializer.Serialize(writer, JObject.FromObject(item));
			else
				serializer.Serialize(writer, item.Name);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var value = serializer.Deserialize(reader);
			return value switch
			{
				string name => new TrackedItem(name),
				JObject jobject => jobject.ToObject<TrackedItem>()!,
				_ => new TrackedItem("corrupted trainer.ini")
			};
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TrackedItem);
		}
	}
}
