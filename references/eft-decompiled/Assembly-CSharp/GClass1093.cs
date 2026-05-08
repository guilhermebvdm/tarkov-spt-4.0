using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1093 : GClass1087.GInterface66
{
	[NonSerialized]
	public JsonSerializer JsonSerializer_0;

	public GClass1093([CanBeNull] JsonSerializerSettings serializerSettings)
	{
		JsonSerializer_0 = JsonSerializer.CreateDefault(serializerSettings);
	}

	public T Populate<T>(JObject json, T item, out bool needReSave)
	{
		needReSave = false;
		if (json == null)
		{
			return item;
		}
		using JsonReader reader = json.CreateReader();
		JsonSerializer_0.Populate(reader, item);
		return item;
	}

	public JObject PrepareToSave<T>(T item)
	{
		return JObject.FromObject(item);
	}
}
