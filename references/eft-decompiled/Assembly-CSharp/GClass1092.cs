using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public abstract class GClass1092
{
	public static void ConvertIndexToObject<T>(JObject data, string name, IReadOnlyList<T> variants)
	{
		JProperty jProperty = data.Property(name);
		if (jProperty != null)
		{
			int index = smethod_0(jProperty.Value.Value<int>(), 0, variants.Count - 1);
			jProperty.Value.Replace(JObject.FromObject(variants[index]));
		}
	}

	public static void ConvertIndexToValue<T>(JObject data, string name, IReadOnlyList<T> variants) where T : struct
	{
		JProperty jProperty = data.Property(name);
		if (jProperty != null)
		{
			int index = smethod_0(jProperty.Value.Value<int>(), 0, variants.Count - 1);
			jProperty.Value.Replace(new JValue(variants[index]));
		}
	}

	public static void SetPropertyValue<T>(JObject data, string name, T newValue) where T : struct
	{
		data.Property(name)?.Value.Replace(new JValue(newValue));
	}

	public static void RenameProperty(JObject data, string oldName, string newName)
	{
		JProperty jProperty = data.Property(oldName);
		if (jProperty != null)
		{
			JToken value = jProperty.Value;
			jProperty.Replace(new JProperty(newName, value));
		}
	}

	public static void MakeIntOffset(JObject data, string name, int offset)
	{
		JProperty jProperty = data.Property(name);
		if (jProperty != null)
		{
			int num = jProperty.Value.Value<int>();
			jProperty.Value.Replace(new JValue(num + offset));
		}
	}

	public static int smethod_0(int value, int min, int max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
		return value;
	}
}
