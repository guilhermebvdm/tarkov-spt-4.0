using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass824
{
	[NonSerialized]
	public const long Long_0 = 10800000L;

	[NonSerialized]
	public Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	public long Long_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	public float DeltaTime
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

	public void RecordDeltaTime()
	{
		if (Stopwatch_0.IsRunning)
		{
			long elapsedMilliseconds = Stopwatch_0.ElapsedMilliseconds;
			DeltaTime = (float)((double)(elapsedMilliseconds - Long_1) / 1000.0);
			Long_1 = elapsedMilliseconds;
		}
		else
		{
			Stopwatch_0.Start();
			DeltaTime = Time.unscaledDeltaTime;
		}
		if (Long_1 > 10800000L)
		{
			Long_1 = 0L;
			Stopwatch_0.Restart();
		}
	}
}
