using System;
using System.Diagnostics;

public class GClass1361
{
	public readonly string GroupName;

	public readonly IFPSMeasureStatistics MeasureStatistics;

	[NonSerialized]
	public Stopwatch Stopwatch_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public long Long_0;

	public double CurrentMilliseconds => new TimeSpan(Stopwatch_0.ElapsedTicks).TotalMilliseconds;

	public GClass1361(string name, IFPSMeasureStatistics measureStatistics)
	{
		GroupName = name;
		Stopwatch_0 = new Stopwatch();
		Bool_0 = false;
		MeasureStatistics = measureStatistics;
	}

	public void StartMeasure()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			Stopwatch_0.Reset();
			Stopwatch_0.Start();
		}
	}

	public void StopMeasure(bool summarize = false)
	{
		if (Bool_0)
		{
			Stopwatch_0.Stop();
			Bool_0 = false;
			if (summarize)
			{
				Long_0 += Stopwatch_0.ElapsedTicks;
				return;
			}
			Long_0 = Stopwatch_0.ElapsedTicks;
			AddMeasurement();
		}
	}

	public void AddMeasurement()
	{
		TimeSpan timeSpan = new TimeSpan(Long_0);
		MeasureStatistics.AddMeasurement(new GStruct132
		{
			Value = timeSpan.TotalMilliseconds,
			DateTime = DateTime.UtcNow
		});
		Long_0 = 0L;
	}
}
