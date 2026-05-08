using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1863 : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, ((GClass846)value).JToken);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return new GClass846
		{
			JToken = serializer.Deserialize<JToken>(reader)
		};
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(GClass846);
	}
}
