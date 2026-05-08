using System;
using RootMotion;
using UnityEngine;

public class GClass1462
{
	public static float Float(float t, InterpolationMode mode)
	{
		float num = 0f;
		return mode switch
		{
			InterpolationMode.None => smethod_0(t, 0f, 1f), 
			InterpolationMode.InOutCubic => smethod_1(t, 0f, 1f), 
			InterpolationMode.InOutQuintic => smethod_2(t, 0f, 1f), 
			InterpolationMode.InOutSine => smethod_22(t, 0f, 1f), 
			InterpolationMode.InQuintic => smethod_3(t, 0f, 1f), 
			InterpolationMode.InQuartic => smethod_4(t, 0f, 1f), 
			InterpolationMode.InCubic => smethod_5(t, 0f, 1f), 
			InterpolationMode.InQuadratic => smethod_6(t, 0f, 1f), 
			InterpolationMode.InElastic => smethod_24(t, 0f, 1f), 
			InterpolationMode.InElasticSmall => smethod_18(t, 0f, 1f), 
			InterpolationMode.InElasticBig => smethod_19(t, 0f, 1f), 
			InterpolationMode.InSine => smethod_20(t, 0f, 1f), 
			InterpolationMode.InBack => smethod_25(t, 0f, 1f), 
			InterpolationMode.OutQuintic => smethod_7(t, 0f, 1f), 
			InterpolationMode.OutQuartic => smethod_8(t, 0f, 1f), 
			InterpolationMode.OutCubic => smethod_9(t, 0f, 1f), 
			InterpolationMode.OutInCubic => smethod_10(t, 0f, 1f), 
			InterpolationMode.OutInQuartic => smethod_10(t, 0f, 1f), 
			InterpolationMode.OutElastic => smethod_24(t, 0f, 1f), 
			InterpolationMode.OutElasticSmall => smethod_16(t, 0f, 1f), 
			InterpolationMode.OutElasticBig => smethod_17(t, 0f, 1f), 
			InterpolationMode.OutSine => smethod_21(t, 0f, 1f), 
			InterpolationMode.OutBack => smethod_26(t, 0f, 1f), 
			InterpolationMode.OutBackCubic => smethod_14(t, 0f, 1f), 
			InterpolationMode.OutBackQuartic => smethod_15(t, 0f, 1f), 
			InterpolationMode.BackInCubic => smethod_12(t, 0f, 1f), 
			InterpolationMode.BackInQuartic => smethod_13(t, 0f, 1f), 
			_ => 0f, 
		};
	}

	public static Vector3 V3(Vector3 v1, Vector3 v2, float t, InterpolationMode mode)
	{
		float num = Float(t, mode);
		return (1f - num) * v1 + num * v2;
	}

	public static float LerpValue(float value, float target, float increaseSpeed, float decreaseSpeed)
	{
		if (value == target)
		{
			return target;
		}
		if (value < target)
		{
			return Mathf.Clamp(value + Time.deltaTime * increaseSpeed, float.NegativeInfinity, target);
		}
		return Mathf.Clamp(value - Time.deltaTime * decreaseSpeed, target, float.PositiveInfinity);
	}

	public static float smethod_0(float t, float b, float c)
	{
		return b + c * t;
	}

	public static float smethod_1(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (-2f * num2 + 3f * num);
	}

	public static float smethod_2(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (6f * num2 * num + -15f * num * num + 10f * num2);
	}

	public static float smethod_3(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (num2 * num);
	}

	public static float smethod_4(float t, float b, float c)
	{
		float num = t * t;
		return b + c * (num * num);
	}

	public static float smethod_5(float t, float b, float c)
	{
		float num = t * t * t;
		return b + c * num;
	}

	public static float smethod_6(float t, float b, float c)
	{
		float num = t * t;
		return b + c * num;
	}

	public static float smethod_7(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (num2 * num + -5f * num * num + 10f * num2 + -10f * num + 5f * t);
	}

	public static float smethod_8(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (-1f * num * num + 4f * num2 + -6f * num + 4f * t);
	}

	public static float smethod_9(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (num2 + -3f * num + 3f * t);
	}

	public static float smethod_10(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (4f * num2 + -6f * num + 3f * t);
	}

	public static float smethod_11(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (6f * num2 + -9f * num + 4f * t);
	}

	public static float smethod_12(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (4f * num2 + -3f * num);
	}

	public static float smethod_13(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (2f * num * num + 2f * num2 + -3f * num);
	}

	public static float smethod_14(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (4f * num2 + -9f * num + 6f * t);
	}

	public static float smethod_15(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (-2f * num * num + 10f * num2 + -15f * num + 8f * t);
	}

	public static float smethod_16(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * t);
	}

	public static float smethod_17(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (56f * num2 * num + -175f * num * num + 200f * num2 + -100f * num + 20f * t);
	}

	public static float smethod_18(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (33f * num2 * num + -59f * num * num + 32f * num2 + -5f * num);
	}

	public static float smethod_19(float t, float b, float c)
	{
		float num = t * t;
		float num2 = num * t;
		return b + c * (56f * num2 * num + -105f * num * num + 60f * num2 + -10f * num);
	}

	public static float smethod_20(float t, float b, float c)
	{
		c -= b;
		return (0f - c) * Mathf.Cos(t / 1f * (MathF.PI / 2f)) + c + b;
	}

	public static float smethod_21(float t, float b, float c)
	{
		c -= b;
		return c * Mathf.Sin(t / 1f * (MathF.PI / 2f)) + b;
	}

	public static float smethod_22(float t, float b, float c)
	{
		c -= b;
		return (0f - c) / 2f * (Mathf.Cos(MathF.PI * t / 1f) - 1f) + b;
	}

	public static float smethod_23(float t, float b, float c)
	{
		c -= b;
		float num = 1f;
		float num2 = 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (t == 0f)
		{
			return b;
		}
		if ((t /= num) == 1f)
		{
			return b + c;
		}
		if (num4 != 0f && num4 >= Mathf.Abs(c))
		{
			num3 = num2 / (MathF.PI * 2f) * Mathf.Asin(c / num4);
		}
		else
		{
			num4 = c;
			num3 = num2 / 4f;
		}
		return 0f - num4 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * num - num3) * (MathF.PI * 2f) / num2) + b;
	}

	public static float smethod_24(float t, float b, float c)
	{
		c -= b;
		float num = 1f;
		float num2 = 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (t == 0f)
		{
			return b;
		}
		if ((t /= num) == 1f)
		{
			return b + c;
		}
		if (num4 != 0f && num4 >= Mathf.Abs(c))
		{
			num3 = num2 / (MathF.PI * 2f) * Mathf.Asin(c / num4);
		}
		else
		{
			num4 = c;
			num3 = num2 / 4f;
		}
		return num4 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * num - num3) * (MathF.PI * 2f) / num2) + c + b;
	}

	public static float smethod_25(float t, float b, float c)
	{
		c -= b;
		t /= 1f;
		float num = 1.70158f;
		return c * t * t * (2.70158f * t - num) + b;
	}

	public static float smethod_26(float t, float b, float c)
	{
		float num = 1.70158f;
		c -= b;
		t = t / 1f - 1f;
		return c * (t * t * (2.70158f * t + num) + 1f) + b;
	}
}
