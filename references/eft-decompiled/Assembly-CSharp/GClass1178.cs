using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1178 : IDisposable
{
	[NonSerialized]
	[CompilerGenerated]
	public float Float_0 = 1f;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3 = 1f;

	[NonSerialized]
	public float Float_4;

	public float Volume
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

	public float EndVolume
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public float StartVolume
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public void Tick(float dt)
	{
		Float_4 += dt;
		Volume = method_0();
	}

	public float method_0()
	{
		if (!(Float_1 <= 0f) && Float_4 < Float_1)
		{
			float t = Float_4 / Float_1;
			return Mathf.Lerp(StartVolume, EndVolume, t);
		}
		return EndVolume;
	}

	public void FadeTo(float target, float duration)
	{
		Float_1 = duration;
		Float_4 = 0f;
		StartVolume = Volume;
		EndVolume = target;
		if (duration <= 0f)
		{
			Volume = EndVolume;
		}
	}

	public void Reset()
	{
		Volume = 0f;
		StartVolume = 0f;
		EndVolume = 0f;
	}

	public void Dispose()
	{
		Reset();
	}

	~GClass1178()
	{
		Dispose();
	}
}
