using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Class1030
{
	[NonSerialized]
	public Dictionary<string, Dictionary<string, GClass1621>> Dictionary_0 = new Dictionary<string, Dictionary<string, GClass1621>>();

	[NonSerialized]
	public static Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	public GClass1621 Gclass1621_0;

	public bool Enabled;

	[Conditional("DEBUG")]
	public void BeginSample(string jobId, string jobStageId)
	{
		if (Enabled)
		{
			Gclass1621_0 = method_0(jobId, jobStageId);
			Stopwatch_0.Start();
		}
	}

	[Conditional("DEBUG")]
	public void EndSample()
	{
		if (Enabled)
		{
			Stopwatch_0.Stop();
			Gclass1621_0.AddValue(Stopwatch_0.ElapsedMilliseconds);
			Gclass1621_0 = null;
			Stopwatch_0.Reset();
		}
	}

	public GClass1621 method_0(string jobId, string jobStageId)
	{
		if (!Dictionary_0.TryGetValue(jobId, out var value))
		{
			value = new Dictionary<string, GClass1621>();
			Dictionary_0[jobId] = value;
		}
		if (!value.TryGetValue(jobStageId, out var value2))
		{
			value2 = (value[jobStageId] = new GClass1621());
		}
		return value2;
	}

	public string GetReport()
	{
		return "<disabled>";
	}
}
