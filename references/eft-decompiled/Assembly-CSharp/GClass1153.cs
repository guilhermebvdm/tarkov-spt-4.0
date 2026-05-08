using System;
using UnityEngine;

public class GClass1153
{
	[NonSerialized]
	public const float Float_0 = 1E-05f;

	[NonSerialized]
	public Transform Transform_0;

	public GClass1153(Transform listenerTransform)
	{
		Transform_0 = listenerTransform;
	}

	public GStruct89 Calculate(Vector3 soundPos)
	{
		Vector3 position = Transform_0.position;
		Vector3 vector = soundPos - position;
		if (vector.sqrMagnitude < 1E-05f)
		{
			return new GStruct89(1f, 0f, 0f, 0f);
		}
		Vector3 normalized = vector.normalized;
		float num = Mathf.Clamp01(normalized.z);
		float num2 = Mathf.Clamp01(0f - normalized.z);
		float num3 = Mathf.Clamp01(normalized.y);
		float num4 = Mathf.Clamp01(0f - normalized.y);
		float num5 = num + num2 + num3 + num4;
		if (num5 < Mathf.Epsilon)
		{
			return new GStruct89(1f, 0f, 0f, 0f);
		}
		return new GStruct89(num / num5, num2 / num5, num3 / num5, num4 / num5);
	}

	public GStruct89 Influence(GStruct89 w, float rotationCoefficient, float distanceCoefficient, float distancePercentage)
	{
		float num = 1f + rotationCoefficient * w.Back;
		float num2 = 1f + rotationCoefficient * w.Up;
		float num3 = 1f + rotationCoefficient * w.Down;
		float num4 = 1f + distanceCoefficient * distancePercentage;
		float front = w.Front;
		float num5 = w.Back * num * num4;
		float num6 = w.Up * num2 * num4;
		float num7 = w.Down * num3 * num4;
		float num8 = front + num5 + num6 + num7;
		if (num8 > Mathf.Epsilon)
		{
			return new GStruct89(front / num8, num5 / num8, num6 / num8, num7 / num8);
		}
		return new GStruct89(1f, 0f, 0f, 0f);
	}
}
