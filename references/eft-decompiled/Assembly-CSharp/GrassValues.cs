using System;
using UnityEngine;

[Serializable]
public class GrassValues
{
	public float AngleInRads;

	public float TailLength;

	[Range(0.01f, 16f)]
	public float Radius;

	[Range(0.01f, 16f)]
	public float PressedRadius;

	public float ReturnTime;

	public GrassValues()
	{
	}

	public GrassValues(float angleInRads, float tailLength, float radius, float pressedRadius, float returnTime)
	{
		AngleInRads = angleInRads;
		TailLength = tailLength;
		Radius = radius;
		PressedRadius = pressedRadius;
		ReturnTime = returnTime;
	}

	public Vector4 ToVector4ExceptReturnTime()
	{
		return new Vector4(AngleInRads, TailLength, Radius, PressedRadius);
	}

	public void Lerp(GrassValues from, GrassValues to, float t)
	{
		Radius = Mathf.Lerp(from.Radius, to.Radius, t);
		TailLength = Mathf.Lerp(from.TailLength, to.TailLength, t);
		ReturnTime = Mathf.Lerp(from.ReturnTime, to.ReturnTime, t);
		AngleInRads = Mathf.Lerp(from.AngleInRads, to.AngleInRads, t);
		PressedRadius = Mathf.Lerp(from.PressedRadius, to.PressedRadius, t);
	}
}
