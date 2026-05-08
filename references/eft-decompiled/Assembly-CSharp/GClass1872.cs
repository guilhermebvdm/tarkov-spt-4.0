using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GClass1872<T, U, V> : JsonConverter where T : GClass1867<U, V>, new()
{
	[NonSerialized]
	[ThreadStatic]
	public static List<GClass1872<T, U, V>> List_0;

	[JsonIgnore]
	public override bool CanWrite => !method_1();

	[JsonIgnore]
	public override bool CanRead => !method_1();

	public void method_0()
	{
		if (List_0 == null)
		{
			List_0 = new List<GClass1872<T, U, V>>();
		}
		List_0.Add(this);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		method_0();
		return ((T)serializer.Deserialize(reader, typeof(T))).Deserialize(reader, objectType, serializer);
	}

	public override bool CanConvert(Type objectType)
	{
		Type typeFromHandle = typeof(U);
		if (!typeFromHandle.IsInterface)
		{
			if (!(typeFromHandle == objectType))
			{
				return objectType.IsSubclassOf(typeFromHandle);
			}
			return true;
		}
		return typeFromHandle.IsAssignableFrom(objectType);
	}

	public bool method_1()
	{
		int num;
		if (List_0 != null)
		{
			num = (List_0.Contains(this) ? 1 : 0);
			if (num != 0)
			{
				List_0.Remove(this);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
		method_0();
		new T().Serialize(writer, value, serializer);
	}
}
