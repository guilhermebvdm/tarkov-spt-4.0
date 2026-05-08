using System;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public class ChartAdvancedSettings
{
	[NonSerialized]
	public static ChartAdvancedSettings MInstance;

	[NonSerialized]
	public static string[] FractionDigits = new string[8] { "{0:0}", "{0:0.#}", "{0:0.##}", "{0:0.###}", "{0:0.####}", "{0:0.#####}", "{0:0.######}", "{0:0.#######}" };

	[Range(0f, 7f)]
	public int ValueFractionDigits = 2;

	[Range(0f, 7f)]
	public int AxisFractionDigits = 2;

	public static ChartAdvancedSettings Instance
	{
		get
		{
			if (MInstance == null)
			{
				MInstance = new ChartAdvancedSettings();
			}
			return MInstance;
		}
	}

	public string method_0(string format, double val)
	{
		try
		{
			return string.Format(format, val);
		}
		catch
		{
		}
		return " ";
	}

	public string method_1(int value)
	{
		value = Mathf.Clamp(value, 0, 7);
		return FractionDigits[value];
	}

	public string FormatFractionDigits(int digits, double val)
	{
		return method_0(method_1(digits), val);
	}
}
