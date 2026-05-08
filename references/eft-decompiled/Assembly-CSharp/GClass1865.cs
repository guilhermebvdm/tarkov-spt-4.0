using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

public class GClass1865<T> : JsonConverter
{
	[Serializable]
	[CompilerGenerated]
	public class Class1150
	{
		public static readonly Class1150 class1150_0 = new Class1150();

		public static Func<string, string> func_0;

		public string method_0(string f)
		{
			return "\"" + f + "\"";
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		IEnumerable<string> values = from f in value.ToString().Replace(" ", string.Empty).Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries)
			select "\"" + f + "\"";
		writer.WriteRawValue("[" + string.Join(",", values) + "]");
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		int num = 0;
		if (reader.TokenType != JsonToken.StartArray)
		{
			return num;
		}
		reader.Read();
		while (reader.TokenType != JsonToken.EndArray)
		{
			num += (int)Enum.Parse(objectType, reader.Value.ToString());
			reader.Read();
		}
		return num;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(T);
	}
}
