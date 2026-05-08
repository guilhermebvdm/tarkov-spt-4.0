using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BackendConfigAbstractClass
{
	public const string DEFAULT_BACKEND_URL = "https://prod.escapefromtarkov.com";

	public static readonly GClass639 GitVersion;

	[NonSerialized]
	public static string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public static ApplicationConfigClass ApplicationConfigClass;

	public static string BackendUrl
	{
		get
		{
			if (!GClass856.IsNullOrEmpty(String_0))
			{
				return String_0;
			}
			if ((bool)EFTBackendSettings.Instance)
			{
				String_0 = EFTBackendSettings.Instance.SelectedBackendUrl;
			}
			else
			{
				String_0 = ((Config == null || GClass856.IsNullOrEmpty(Config.BackendUrl)) ? "https://prod.escapefromtarkov.com" : Config.BackendUrl);
			}
			return String_0;
		}
	}

	public static string BackendCacheDir => Application.dataPath + "/../cache/";

	public static string ConfigFilePath => Application.dataPath + "/../client.config.json";

	public static ApplicationConfigClass Config
	{
		[CompilerGenerated]
		get
		{
			return ApplicationConfigClass;
		}
		[CompilerGenerated]
		set
		{
			ApplicationConfigClass = value;
		}
	}

	static BackendConfigAbstractClass()
	{
		GitVersion = new GClass639();
		Assert.raiseExceptions = true;
	}

	public static bool LoadApplicationConfig(ApplicationConfigClass @default = null)
	{
		if (Config != null)
		{
			return false;
		}
		if (!File.Exists(ConfigFilePath))
		{
			Config = @default;
			return false;
		}
		try
		{
			Config = JsonParserClass.ParseJsonTo<ApplicationConfigClass>(File.ReadAllText(ConfigFilePath), Array.Empty<JsonConverter>());
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Config = @default;
			return false;
		}
	}

	public static string smethod_0()
	{
		if (File.Exists(ConfigFilePath))
		{
			return File.ReadAllText(ConfigFilePath);
		}
		if (Config == null)
		{
			return JsonParserClass.ToJson(new ApplicationConfigClass());
		}
		return JsonParserClass.ToJson(Config);
	}

	public static void smethod_1(JObject jObject, string commandKey, string commandValue)
	{
		string propertyName = commandKey;
		string commandKey2 = string.Empty;
		if (commandKey.Contains("__"))
		{
			int num = commandKey.IndexOf("__", StringComparison.Ordinal);
			propertyName = commandKey.Substring(0, num);
			commandKey2 = commandKey.Substring(num + 2);
		}
		if (jObject.TryGetValue(propertyName, out JToken value))
		{
			if (value is JObject jObject2)
			{
				smethod_1(jObject2, commandKey2, commandValue);
			}
			else if (value.Parent is JProperty jProperty)
			{
				jProperty.Value = commandValue;
			}
		}
	}

	public static ApplicationConfigClass MergeConfigWith(GClass1676 commandLine)
	{
		JObject jObject = JObject.Parse(smethod_0());
		foreach (var (commandKey, commandValue) in commandLine.ParsedArgs)
		{
			smethod_1(jObject, commandKey, commandValue);
		}
		Config = JsonParserClass.ParseJsonTo<ApplicationConfigClass>(JsonConvert.SerializeObject(jObject), Array.Empty<JsonConverter>());
		return Config;
	}
}
