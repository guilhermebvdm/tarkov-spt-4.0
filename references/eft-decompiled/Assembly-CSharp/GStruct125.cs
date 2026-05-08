using System;
using System.Diagnostics;
using System.Threading;

public struct GStruct125
{
	[NonSerialized]
	public Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public GStruct124 Gstruct124_0;

	public double average;

	public GStruct125(int n)
	{
		Stopwatch_0.Start();
		Gstruct124_0 = new GStruct124(n);
		Double_0 = 0.0;
		average = 0.0;
	}

	public void Begin()
	{
		Double_0 = Stopwatch_0.Elapsed.TotalSeconds;
	}

	public void End()
	{
		double newValue = Stopwatch_0.Elapsed.TotalSeconds - Double_0;
		Gstruct124_0.Add(newValue);
		Interlocked.Exchange(ref average, Gstruct124_0.Value);
	}
}
