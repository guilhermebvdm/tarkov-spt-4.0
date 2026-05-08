using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

public class GClass1088 : GClass1087
{
	[NonSerialized]
	public JObject Jobject_0;

	public GClass1088(JObject json, [NotNull] GInterface66 serializer, params GClass1087[] fallbackProviders)
		: base(serializer, fallbackProviders)
	{
		Jobject_0 = json;
	}

	public GClass1088(string json, [NotNull] GInterface66 serializer, params GClass1087[] fallbackProviders)
		: base(serializer, fallbackProviders)
	{
		Jobject_0 = JObject.Parse(json);
	}

	public override Task<JObject> LoadJson()
	{
		return Task.FromResult(Jobject_0);
	}

	public override Task SaveJson(JObject json)
	{
		Jobject_0 = json;
		return Task.CompletedTask;
	}

	public override string ToString()
	{
		return string.Format("{0}:\n{1}", "JsonDirectSettingsProvider", Jobject_0);
	}
}
