using System;

public class GClass869
{
	public static float Floor(float value, int decimals = 0)
	{
		return smethod_0(value, decimals, Math.Floor);
	}

	public static float Ceil(float value, int decimals = 0)
	{
		return smethod_0(value, decimals, Math.Ceiling);
	}

	public static float Round(float value, int decimals = 0)
	{
		return smethod_0(value, decimals, Math.Round);
	}

	public static float smethod_0(float value, int decimals, Func<double, double> rounder)
	{
		double num = Math.Pow(10.0, decimals);
		return (float)(rounder(value * (float)num) / num);
	}
}
