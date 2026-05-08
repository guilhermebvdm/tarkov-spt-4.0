using System;
using System.Globalization;

namespace EFT.Rendering.Clouds;

[Serializable]
public struct WindOrientation
{
	public enum WindOverrideMode
	{
		Custom,
		Global,
		Additive,
		Multiply
	}

	public WindOverrideMode mode;

	public float customValue;

	public float additiveValue;

	public float multiplyValue;

	public override string ToString()
	{
		if (mode == WindOverrideMode.Global)
		{
			return mode.ToString();
		}
		string text = null;
		if (mode == WindOverrideMode.Custom)
		{
			text = customValue.ToString(CultureInfo.InvariantCulture);
		}
		if (mode == WindOverrideMode.Additive)
		{
			text = additiveValue.ToString(CultureInfo.InvariantCulture);
		}
		if (mode == WindOverrideMode.Multiply)
		{
			text = multiplyValue.ToString(CultureInfo.InvariantCulture);
		}
		return text + " (" + mode.ToString() + ")";
	}
}
