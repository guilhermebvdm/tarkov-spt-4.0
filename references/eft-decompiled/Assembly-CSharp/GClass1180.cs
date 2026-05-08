using System;
using System.Runtime.CompilerServices;
using Audio.AuxiliaryAudioUtils;
using UnityEngine;

public class GClass1180
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	[NonSerialized]
	public int[] Int_3;

	public int HighAudioSourcePriority
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public int MedAudioSourcePriority
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
	}

	public int LowAudioSourcePriority
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
	}

	public GClass1180(int lowPriority = 256, int medPriority = 128, int highPriority = 64)
	{
		Int_0 = highPriority;
		Int_1 = medPriority;
		Int_2 = lowPriority;
		Int_3 = new int[3] { LowAudioSourcePriority, MedAudioSourcePriority, HighAudioSourcePriority };
	}

	public int CalculatePriority(float currentDistance, float maxDistance)
	{
		float num = Mathf.Clamp01(currentDistance / maxDistance);
		int value = Mathf.RoundToInt((1f - num) * (float)(Int_3.Length - 1));
		value = Mathf.Clamp(value, 0, Int_3.Length - 1);
		return Int_3[value];
	}

	public float CalculateQualityFactorSq(float currentDistanceSquared, float maxDistanceSquared)
	{
		int num = CalculatePriority(currentDistanceSquared, maxDistanceSquared);
		if (num == HighAudioSourcePriority)
		{
			return 1f;
		}
		if (num == MedAudioSourcePriority)
		{
			return 2f / 3f;
		}
		return 1f / 3f;
	}

	public EAudioSourcePriority GetPriority(float currentDistance, float maxDistance)
	{
		if (currentDistance > maxDistance)
		{
			return EAudioSourcePriority.OutOfRange;
		}
		int num = CalculatePriority(currentDistance, maxDistance);
		if (num == HighAudioSourcePriority)
		{
			return EAudioSourcePriority.High;
		}
		if (num == MedAudioSourcePriority)
		{
			return EAudioSourcePriority.Medium;
		}
		return EAudioSourcePriority.Low;
	}
}
