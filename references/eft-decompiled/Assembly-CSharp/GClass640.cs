using System;

public abstract class GClass640
{
	[NonSerialized]
	public static GClass827.GStruct50 Gstruct50_0;

	public static void LogMemoryConsumption(string mark)
	{
	}

	public static string smethod_0(long value)
	{
		double num = (double)value / 1000000.0;
		if (num < 0.0)
		{
			return num.ToString("0.###");
		}
		return ((long)num).ToString("#,##0");
	}
}
