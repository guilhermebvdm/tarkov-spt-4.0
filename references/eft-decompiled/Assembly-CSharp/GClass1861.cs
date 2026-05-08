using System;
using EFT;
using Newtonsoft.Json;

public class GClass1861<T, U> : JsonConverter where U : ISerializer<T>, new()
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, new U().Serialize((T)value));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		U val = serializer.Deserialize<U>(reader);
		return (val != null) ? val.Deserialize() : default(T);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(T);
	}
}
