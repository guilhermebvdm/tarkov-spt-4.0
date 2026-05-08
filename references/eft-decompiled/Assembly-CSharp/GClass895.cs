using System;
using UnityEngine;

public class GClass895
{
	[NonSerialized]
	public const float Float_0 = 25f;

	[NonSerialized]
	public const float Float_1 = 0.2f;

	public Vector3 Position;

	public double EndTime;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public Vector2 Vector2_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	public bool IsActive => AudioSettings.dspTime < EndTime;

	public Vector2 Radius
	{
		get
		{
			return Vector2_0;
		}
		set
		{
			Vector2_0 = value;
			Vector2_1 = Vector2_0 * Vector2_0;
		}
	}

	public float Rolloff
	{
		get
		{
			return Float_2;
		}
		set
		{
			Float_2 = value;
			Float_3 = Float_2 * Float_2;
		}
	}

	public bool IsChoked(Vector3 newPosition, Vector3 listener, float sqrDistance)
	{
		Vector3 normalized = (Position - listener).normalized;
		float t = Mathf.InverseLerp(25f, Float_3, sqrDistance);
		float num = Mathf.Lerp(Vector2_1.x, Vector2_1.y, t);
		return Vector3.SqrMagnitude(Position + normalized * (num - 0.2f) - newPosition) < num;
	}
}
