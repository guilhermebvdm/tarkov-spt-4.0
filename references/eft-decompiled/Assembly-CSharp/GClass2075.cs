using System;

public abstract class GClass2075
{
	[NonSerialized]
	public static Array Array_0;

	public static int Count => Array_0.Length;

	static GClass2075()
	{
		Array_0 = Enum.GetValues(typeof(EInteraction));
	}
}
