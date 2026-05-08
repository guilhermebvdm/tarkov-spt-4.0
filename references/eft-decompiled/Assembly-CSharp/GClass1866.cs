using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GClass1866<T> : JsonConverter where T : struct, Enum
{
	[NonSerialized]
	public static Type Type_0 = typeof(T);

	[NonSerialized]
	public Dictionary<string, T> Dictionary_0;

	[NonSerialized]
	public Dictionary<T, string> Dictionary_1;

	[NonSerialized]
	public bool Bool_0;

	public GClass1866()
		: this(true)
	{
	}

	public GClass1866(bool caseSensitive = true)
	{
		Bool_0 = caseSensitive;
		int count = GClass866<T>.Count;
		Dictionary_0 = new Dictionary<string, T>(count);
		Dictionary_1 = new Dictionary<T, string>(count);
		for (int num = 0; num < count; num++)
		{
			T val = GClass866<T>.Values[num];
			string text = GClass866<T>.Names[num];
			GAttribute24 obj = Type_0.GetMember(text).First().GetCustomAttributes(typeof(GAttribute24), inherit: false)
				.FirstOrDefault() as GAttribute24;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = obj.Name;
				if (obj2 != null)
				{
					goto IL_0085;
				}
			}
			obj2 = text;
			goto IL_0085;
			IL_0085:
			text = (string)obj2;
			Dictionary_1.Add(val, text);
			if (!Bool_0)
			{
				text = text.ToLower();
			}
			Dictionary_0.Add(text, val);
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(Dictionary_1[(T)value]);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		string text = serializer.Deserialize<string>(reader);
		if (!Bool_0)
		{
			text = text.ToLower();
		}
		T value;
		bool flag = GClass866<T>.TryGetValueByName(text, out value);
		if (text == null)
		{
			Debug.LogError($"Null Enum value at {reader.Path}, fallback to {typeof(T).Name}.{value}");
			return value;
		}
		if (Dictionary_0.TryGetValue(text, out var value2))
		{
			return value2;
		}
		if (!flag)
		{
			Debug.LogError($"Incorrect Enum value {text} at {reader.Path}, fallback to {typeof(T).Name}.{value}");
		}
		return value;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == Type_0;
	}
}
