using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass805
{
	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	public float Time
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public float Value => Float_2;

	public GClass805(float init, float tolerance, float time)
	{
		Time = time;
		Float_2 = init;
		Float_1 = tolerance;
	}

	public float Calculate(float targetValue)
	{
		Float_2 = Mathf.SmoothDamp(Float_2, targetValue, ref Float_3, Time);
		if (Mathf.Abs(Float_2 - targetValue) < Float_1)
		{
			Float_2 = targetValue;
			Float_3 = 0f;
		}
		return Float_2;
	}
}
