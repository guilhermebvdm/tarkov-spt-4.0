using System;
using Comfort.Common;

public class GClass720 : LoggerClass
{
	[NonSerialized]
	public const string String_0 = "Default";

	public static void Setup(LoggerMode mode, bool logApplicationMessages = true)
	{
		Singleton<GClass720>.Create(new GClass720(mode, logApplicationMessages));
	}

	public GClass720(LoggerMode mode, bool logApplicationMessages)
		: base("Default", mode, logApplicationMessages)
	{
	}

	public static void Release()
	{
		if (Singleton<GClass720>.Instantiated)
		{
			GClass720 instance = Singleton<GClass720>.Instance;
			Singleton<GClass720>.Release(instance);
			instance.Dispose();
		}
	}
}
