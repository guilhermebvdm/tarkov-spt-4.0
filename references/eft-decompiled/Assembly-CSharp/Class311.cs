using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

public abstract class Class311
{
	public static GClass648 smethod_0([CanBeNull] this Class310 cache, string url)
	{
		try
		{
			string text = cache?.Load(url);
			if (text != null)
			{
				return JsonParserClass.ParseJsonTo<GClass648>(text, Array.Empty<JsonConverter>());
			}
		}
		catch (Exception)
		{
		}
		return null;
	}
}
