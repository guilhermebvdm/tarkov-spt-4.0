using System;
using System.IO;

public class GClass1808
{
	public enum EQuestZoneType
	{
		Kill,
		Hide,
		Mark
	}

	[NonSerialized]
	public const string String_0 = "dl_";

	[NonSerialized]
	public const char Char_0 = '_';

	public string Name;

	public string Location;

	public EQuestZoneType ZoneType;

	public static bool IsDailyQuestZone(string zoneName)
	{
		if (zoneName.ToLower().StartsWith("dl_"))
		{
			return zoneName.Split('_').Length > 3;
		}
		return false;
	}

	public GClass1808(string zoneName, string locationId)
	{
		Name = zoneName;
		string[] array = zoneName.Split('_');
		Location = locationId;
		ZoneType = method_0(array[2].ToLower());
	}

	public EQuestZoneType method_0(string typePartOfName)
	{
		return typePartOfName switch
		{
			"mark" => EQuestZoneType.Mark, 
			"kill" => EQuestZoneType.Kill, 
			"hide" => EQuestZoneType.Hide, 
			_ => throw new InvalidDataException("bad naming for quest zone type | " + typePartOfName), 
		};
	}
}
