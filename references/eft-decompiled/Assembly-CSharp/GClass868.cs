using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class GClass868
{
	public static bool TryGetEndpointPath(this string url, out string result)
	{
		Match match = Regex.Match(url, "https?://[^/]+/(.+)");
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			result = value.Replace("/", "-");
			return true;
		}
		result = null;
		return false;
	}

	public static void SaveAsJson(this object obj, string fileName)
	{
		try
		{
			File.WriteAllText(Application.dataPath + "/" + fileName + ".json", JsonParserClass.ToPrettyJson(obj));
		}
		catch (Exception arg)
		{
			Debug.LogError($"SaveAsJson failed: {arg}");
		}
	}
}
