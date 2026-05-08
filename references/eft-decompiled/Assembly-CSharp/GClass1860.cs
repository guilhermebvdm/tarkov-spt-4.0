using System;
using Newtonsoft.Json;
using UnityEngine;

public class GClass1860<T> : JsonConverter<T> where T : Enum
{
	public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, Convert.ToInt32(value));
	}

	public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		T val = default(T);
		try
		{
			val = (T)(object)(int)(long)reader.Value;
			return val;
		}
		catch
		{
			Debug.LogError(string.Format("Failed to convert into {0}. Default value \"{1}\" has been read to avoid exceptions.", "T", val));
		}
		return val;
	}
}
