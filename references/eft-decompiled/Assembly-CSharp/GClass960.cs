using System;
using UnityEngine;

public abstract class GClass960
{
	[NonSerialized]
	public static Rect Rect_0 = smethod_0();

	public static Rect Empty => Rect_0;

	public static bool Contains(this Rect self, Rect rect)
	{
		if (self.xMin <= rect.xMin && self.xMin + self.width >= rect.xMin + rect.width && self.yMin <= rect.yMin && self.yMin + self.height >= rect.yMin + rect.height)
		{
			return true;
		}
		return false;
	}

	public static Rect smethod_0()
	{
		return new Rect
		{
			x = float.PositiveInfinity,
			y = float.PositiveInfinity,
			width = float.NegativeInfinity,
			height = float.NegativeInfinity
		};
	}

	public static bool IsEmpty(this Rect self)
	{
		return self.width < 0f;
	}
}
