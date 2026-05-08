using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1094 : GClass1087.GInterface66
{
	public interface GInterface67
	{
		void Upgrade(JObject data);
	}

	public const string VERSION_KEY = "Version";

	[NonSerialized]
	public uint Uint_0;

	[NonSerialized]
	public GInterface67[] Ginterface67_0;

	[NonSerialized]
	public JProperty Jproperty_0;

	[NonSerialized]
	public JsonSerializer JsonSerializer_0;

	public GClass1094([CanBeNull] JsonSerializerSettings serializerSettings, params GInterface67[] versions)
	{
		Uint_0 = (uint)versions.Length;
		Ginterface67_0 = versions;
		Jproperty_0 = new JProperty("Version", Uint_0);
		JsonSerializer_0 = JsonSerializer.CreateDefault(serializerSettings);
	}

	public T Populate<T>(JObject json, T item, out bool needReSave)
	{
		if (json == null)
		{
			needReSave = true;
			return item;
		}
		needReSave = method_0(json);
		using JsonReader reader = json.CreateReader();
		JsonSerializer_0.Populate(reader, item);
		return item;
	}

	public JObject PrepareToSave<T>(T item)
	{
		JObject jObject = JObject.FromObject(item);
		if (Uint_0 != 0)
		{
			jObject.First?.AddBeforeSelf(Jproperty_0);
		}
		return jObject;
	}

	public bool method_0([NotNull] JObject json)
	{
		int num = json["Version"]?.Value<int>() ?? 0;
		if (num > Uint_0)
		{
			throw new Exception(string.Format("Version {0} of Settings JSON is more than {1}", num, "_currentVersion"));
		}
		if (num == Uint_0)
		{
			return false;
		}
		while (num < Uint_0)
		{
			Ginterface67_0[num++].Upgrade(json);
		}
		return true;
	}
}
