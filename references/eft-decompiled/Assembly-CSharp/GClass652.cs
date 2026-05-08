using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

public abstract class GClass652
{
	public static bool IsNull([CanBeNull] this GClass846 unparsedData)
	{
		if (unparsedData != null && unparsedData.JToken != null)
		{
			return unparsedData.JToken.Type == JTokenType.Null;
		}
		return true;
	}

	public static bool IsNotNull([CanBeNull] this GClass846 unparsedData)
	{
		return !IsNull(unparsedData);
	}
}
