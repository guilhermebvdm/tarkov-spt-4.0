using System;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public class GClass1221
{
	public enum Mode
	{
		Overwrite,
		Append,
		Union,
		SuperSample
	}

	[NonSerialized]
	[CompilerGenerated]
	public int[] Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3[] Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface114 Ginterface114_0;

	[NonSerialized]
	[CompilerGenerated]
	public PerfectCullingSuperSamplingVolume PerfectCullingSuperSamplingVolume_0;

	[NonSerialized]
	[CompilerGenerated]
	public Mode Mode_0;

	public int[] CellIndices
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public Vector3[] SamplingPoints
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public GInterface114 OutputData
	{
		[CompilerGenerated]
		get
		{
			return Ginterface114_0;
		}
		[CompilerGenerated]
		set
		{
			Ginterface114_0 = value;
		}
	}

	public PerfectCullingSuperSamplingVolume OptionalVolume
	{
		[CompilerGenerated]
		get
		{
			return PerfectCullingSuperSamplingVolume_0;
		}
		[CompilerGenerated]
		set
		{
			PerfectCullingSuperSamplingVolume_0 = value;
		}
	}

	public Mode WriteMode
	{
		[CompilerGenerated]
		get
		{
			return Mode_0;
		}
		[CompilerGenerated]
		set
		{
			Mode_0 = value;
		}
	}

	public void RemapIds()
	{
	}

	public GClass1221(int[] cellIndices, Vector3[] samplingPoints, Mode writeMode, GInterface113 sampleSetFactory, PerfectCullingSuperSamplingVolume optionalVolume = null)
	{
		OptionalVolume = optionalVolume;
		CellIndices = cellIndices;
		SamplingPoints = samplingPoints;
		OutputData = sampleSetFactory.CreateSamplingTaskResult(CellIndices.Length);
		WriteMode = writeMode;
	}

	public void Dispose()
	{
		OptionalVolume = null;
		CellIndices = null;
		SamplingPoints = null;
		if (OutputData != null)
		{
			OutputData.Dispose();
		}
		OutputData = null;
	}
}
