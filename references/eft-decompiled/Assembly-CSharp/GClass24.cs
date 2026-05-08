using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class GClass24 : IDisposable
{
	public struct GStruct6
	{
		public double Time;

		public List<GClass23.GStruct5> Samples;
	}

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	[CompilerGenerated]
	public List<GStruct6> List_0 = new List<GStruct6>();

	public List<GStruct6> SpikeSamples
	{
		[CompilerGenerated]
		get
		{
			return List_0;
		}
	}

	public GClass24(double triggerValue, int maxRecords = int.MaxValue)
	{
		Double_0 = triggerValue;
		Int_0 = maxRecords;
	}

	public void Dispose()
	{
	}
}
