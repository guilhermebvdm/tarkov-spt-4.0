using System;
using System.Runtime.CompilerServices;

public class GClass1127 : IDisposable
{
	public float ObstructionVolumeFactor = 1f;

	public float PropagationVolumeFactor = 1f;

	public float OcclusionVolumeFactor = 1f;

	public float DistanceHiCutFrequency = 22000f;

	public float DistanceLowCutFrequency;

	public float DistancePercentage;

	public float IsolationFactor;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2 = 1f;

	public float HighPassResonance
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

	public float LowPassResonance
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float OcclusionRolloffScale
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

	public void Apply(GClass1127 parameters)
	{
		OcclusionVolumeFactor = parameters.OcclusionVolumeFactor;
		DistanceHiCutFrequency = parameters.DistanceHiCutFrequency;
		DistanceLowCutFrequency = parameters.DistanceLowCutFrequency;
		DistancePercentage = parameters.DistancePercentage;
		HighPassResonance = parameters.HighPassResonance;
		LowPassResonance = parameters.LowPassResonance;
		OcclusionRolloffScale = parameters.OcclusionRolloffScale;
		IsolationFactor = parameters.IsolationFactor;
	}

	public void Dispose()
	{
		DistancePercentage = 0f;
		DistanceHiCutFrequency = 22000f;
		DistanceLowCutFrequency = 0f;
		OcclusionVolumeFactor = 1f;
		ObstructionVolumeFactor = 1f;
		PropagationVolumeFactor = 1f;
		HighPassResonance = 1f;
		LowPassResonance = 1f;
		OcclusionRolloffScale = 1f;
		IsolationFactor = 0f;
	}
}
