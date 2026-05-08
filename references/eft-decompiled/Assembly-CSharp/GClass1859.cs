using System;
using Newtonsoft.Json;
using UnityEngine;

public class GClass1859<T> : JsonConverter<T> where T : Enum
{
	public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, Convert.ToInt32(value));
	}

	public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		T val = default(T);
		int? num = reader.ReadAsInt32();
		try
		{
			val = (T)Enum.Parse(typeof(T), num.ToString(), ignoreCase: true);
			return val;
		}
		catch
		{
			Debug.LogError(string.Format("Failed to convert {0} into {1}. Default value \"{2}\" has been read to avoid exceptions.", num, "T", val));
		}
		return val;
	}
}
