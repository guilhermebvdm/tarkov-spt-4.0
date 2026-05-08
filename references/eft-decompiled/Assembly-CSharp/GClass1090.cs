using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GClass1090 : GClass1087
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public TextAsset TextAsset_0;

	public GClass1090(string textAssetName, [NotNull] GInterface66 serializer, params GClass1087[] fallbackProviders)
		: base(serializer, fallbackProviders)
	{
		String_0 = textAssetName;
	}

	public override async Task<JObject> LoadJson()
	{
		if (TextAsset_0 == null)
		{
			ResourceRequest resourceRequest = Resources.LoadAsync<TextAsset>(String_0);
			await GClass841.Await(resourceRequest);
			TextAsset_0 = (TextAsset)resourceRequest.asset;
		}
		return JObject.Parse(TextAsset_0.text);
	}

	public override Task SaveJson(JObject json)
	{
		throw new Exception("JsonResourceSettingsProvider does not support saving");
	}

	public override string ToString()
	{
		return "JsonResourceSettingsProvider:" + String_0;
	}
}
