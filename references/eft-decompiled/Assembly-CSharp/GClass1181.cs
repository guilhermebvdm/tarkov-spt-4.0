using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1181 : GInterface92
{
	[NonSerialized]
	public AudioSource AudioSource_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	public float CurrentPan
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

	public GClass1181(AudioSource source)
	{
		AudioSource_0 = source;
	}

	public void SetPan(float value)
	{
		if (!(AudioSource_0.spatialBlend > 0f))
		{
			AudioSource_0.panStereo = value;
			CurrentPan = value;
		}
	}
}
