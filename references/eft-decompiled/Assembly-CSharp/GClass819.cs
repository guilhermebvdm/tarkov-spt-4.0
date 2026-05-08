using System;
using UnityEngine;

public abstract class GClass819
{
	[NonSerialized]
	public static Vector3 Vector3_0;

	[NonSerialized]
	public static Vector3 Vector3_1;

	public static Color smethod_0(float h, float s, float v, bool hdr)
	{
		Color wHITE_COLOR = GClass842.WHITE_COLOR;
		wHITE_COLOR.r = 0f;
		wHITE_COLOR.g = 0f;
		wHITE_COLOR.b = 0f;
		float num = h * 6f;
		int num2 = (int)Mathf.Floor(num);
		float num3 = num - (float)num2;
		float num4 = v * (1f - s);
		float r = v * (float)(1.0 - (double)s * (double)num3);
		float num5 = v * (float)(1.0 - (double)s * (1.0 - (double)num3));
		switch (num2)
		{
		case 0:
			wHITE_COLOR.r = v;
			wHITE_COLOR.g = num5;
			wHITE_COLOR.b = num4;
			break;
		case 1:
			wHITE_COLOR.r = r;
			wHITE_COLOR.g = v;
			wHITE_COLOR.b = num4;
			break;
		case 2:
			wHITE_COLOR.r = num4;
			wHITE_COLOR.g = v;
			wHITE_COLOR.b = num5;
			break;
		}
		if (hdr)
		{
			return wHITE_COLOR;
		}
		wHITE_COLOR.r = Mathf.Clamp(wHITE_COLOR.r, 0f, 1f);
		wHITE_COLOR.g = Mathf.Clamp(wHITE_COLOR.g, 0f, 1f);
		wHITE_COLOR.b = Mathf.Clamp(wHITE_COLOR.b, 0f, 1f);
		return wHITE_COLOR;
	}

	static GClass819()
	{
		Vector3_1 = default(Vector3);
		Color.RGBToHSV(GClass842.GREEN_COLOR, out Vector3_1.x, out Vector3_1.y, out Vector3_1.z);
		Vector3_0 = default(Vector3);
		Color.RGBToHSV(GClass842.RED_COLOR, out Vector3_0.x, out Vector3_0.y, out Vector3_0.z);
	}

	public static double smethod_1(double a, double b, double value)
	{
		if (!(Math.Abs(a - b) > (double)Mathf.Epsilon))
		{
			return 0.0;
		}
		return smethod_2((value - a) / (b - a));
	}

	public static double smethod_2(double value)
	{
		if (value < 0.0)
		{
			return 0.0;
		}
		if (!(value > 1.0))
		{
			return value;
		}
		return 1.0;
	}

	public static Color smethod_3(this double value, double minValue, double maxValue, Vector3 minColorHsv, Vector3 maxColorHsv)
	{
		return smethod_0(Mathf.Lerp(minColorHsv.x, maxColorHsv.x, (float)smethod_1(minValue, maxValue, value)), Vector3_1.y, Vector3_1.z, hdr: true);
	}

	public static Color GetColor(this double value, double minValue, double maxValue)
	{
		return smethod_3(value, minValue, maxValue, Vector3_1, Vector3_0);
	}

	public static string GetRichTextColor(this Color color)
	{
		if (color == Color.black)
		{
			return "black";
		}
		if (color == Color.white)
		{
			return "white";
		}
		if (color == Color.yellow)
		{
			return "yellow";
		}
		if (color == Color.red)
		{
			return "red";
		}
		if (color == Color.green)
		{
			return "green";
		}
		if (color == Color.blue)
		{
			return "blue";
		}
		if (color == Color.cyan)
		{
			return "cyan";
		}
		if (color == Color.magenta)
		{
			return "magenta";
		}
		if (color == Color.gray)
		{
			return "gray";
		}
		if (color == Color.clear)
		{
			return "clear";
		}
		return "#" + ColorUtility.ToHtmlStringRGB(color);
	}
}
