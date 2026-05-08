using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GClass1089 : GClass1087
{
	[NonSerialized]
	public string String_0;

	public GClass1089(string path, [NotNull] GInterface66 serializer, params GClass1087[] fallbackProviders)
		: base(serializer, fallbackProviders)
	{
		String_0 = path;
	}

	public override async Task<JObject> LoadJson()
	{
		if (File.Exists(String_0))
		{
			try
			{
				using StreamReader streamReader = new StreamReader(String_0);
				return JObject.Parse(await streamReader.ReadToEndAsync());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}
		Debug.LogError("File " + String_0 + " doesn't exist!");
		return null;
	}

	public override async Task SaveJson(JObject json)
	{
		using StreamWriter streamWriter = new StreamWriter(String_0);
		await streamWriter.WriteAsync(json.ToString(Formatting.Indented));
	}

	public void MakeCopy(string postfix)
	{
		if (!string.IsNullOrEmpty(String_0) && File.Exists(String_0))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(String_0);
			File.Copy(String_0, String_0.Replace(fileNameWithoutExtension, fileNameWithoutExtension + postfix), overwrite: true);
		}
	}

	public override string ToString()
	{
		return "JsonFileSettingsProvider:" + String_0;
	}
}
