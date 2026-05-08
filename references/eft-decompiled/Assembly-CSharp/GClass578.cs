using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass578
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_4;

	public float EndTime
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
	}

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public float Radius
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
	}

	public float Duration
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
	}

	public float StartTime
	{
		[CompilerGenerated]
		get
		{
			return Float_4;
		}
	}

	public GClass578(Vector3 pos, float duration, float rad)
	{
		Float_3 = duration;
		Vector3_0 = pos;
		Float_4 = Time.time;
		Float_1 = Time.time + duration;
		Float_2 = rad;
		Float_0 = rad * rad;
	}

	public bool IsInRadius(Vector3 pos)
	{
		return (pos - Position).sqrMagnitude < Float_0;
	}
}
