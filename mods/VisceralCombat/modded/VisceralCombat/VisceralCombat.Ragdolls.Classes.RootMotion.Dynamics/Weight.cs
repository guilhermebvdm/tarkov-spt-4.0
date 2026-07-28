using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public class Weight
{
	[Serializable]
	public enum Mode
	{
		Float,
		Curve
	}

	public Mode mode;

	public float floatValue;

	public AnimationCurve curve;

	public string tooltip = "";

	public Weight(float floatValue)
	{
		this.floatValue = floatValue;
	}

	public Weight(float floatValue, string tooltip)
	{
		this.floatValue = floatValue;
		this.tooltip = tooltip;
	}

	public float GetValue(float param)
	{
		Mode mode = this.mode;
		Mode mode2 = mode;
		if (mode2 == Mode.Curve)
		{
			return curve.Evaluate(param);
		}
		return floatValue;
	}
}
