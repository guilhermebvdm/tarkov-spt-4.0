using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class GClass808
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct84
	{
		public float value;
	}

	public static Color Saturation(this Color color, float value)
	{
		return new Color(color.r * value, color.g * value, color.b * value);
	}

	public static Color Gamma(this Color color, float value)
	{
		Struct84 struct84_ = default(Struct84);
		struct84_.value = value;
		return new Color(smethod_0(color.r, ref struct84_), smethod_0(color.g, ref struct84_), smethod_0(color.b, ref struct84_));
	}

	public static Color ColorFromHSV(float hue, float saturation, float value)
	{
		int num = Convert.ToInt32(Math.Floor(hue / 60f)) % 6;
		double num2 = (double)(hue / 60f) - Math.Floor(hue / 60f);
		value *= 255f;
		byte b = Convert.ToByte(value);
		byte b2 = Convert.ToByte(value * (1f - saturation));
		byte b3 = Convert.ToByte((double)value * (1.0 - num2 * (double)saturation));
		byte b4 = Convert.ToByte((double)value * (1.0 - (1.0 - num2) * (double)saturation));
		return num switch
		{
			0 => new Color32(b, b4, b2, byte.MaxValue), 
			1 => new Color32(b3, b, b2, byte.MaxValue), 
			2 => new Color32(b2, b, b4, byte.MaxValue), 
			3 => new Color32(b2, b3, b, byte.MaxValue), 
			4 => new Color32(b4, b2, b, byte.MaxValue), 
			_ => new Color32(b, b2, b3, byte.MaxValue), 
		};
	}

	public static Color SetAlpha(this Color color, float alpha)
	{
		color.a = alpha;
		return color;
	}

	[CompilerGenerated]
	public static float smethod_0(float originalChannel, ref Struct84 struct84_0)
	{
		if (struct84_0.value > 0f)
		{
			return originalChannel + (1f - originalChannel) * struct84_0.value;
		}
		return originalChannel - originalChannel * struct84_0.value;
	}
}
