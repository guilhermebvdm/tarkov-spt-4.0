using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1862 : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(IDictionary).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		IDictionary dictionary = (IDictionary)value;
		JObject jObject = new JObject();
		foreach (object key in dictionary.Keys)
		{
			object obj = dictionary[key];
			jObject.Add(key.ToString(), (obj == null) ? null : JToken.FromObject(obj, serializer));
		}
		jObject.WriteTo(writer, JsonSerializerSettingsClass.Converters);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		IDictionary dictionary = (IDictionary)serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray.Load(reader);
			return dictionary;
		}
		if (reader.TokenType != JsonToken.StartObject)
		{
			reader.Read();
			return dictionary;
		}
		using JsonReader reader2 = JObject.Load(reader).CreateReader();
		serializer.Populate(reader2, dictionary);
		return dictionary;
	}
}
