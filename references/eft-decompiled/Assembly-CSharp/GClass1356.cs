using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class GClass1356
{
	[NonSerialized]
	[CompilerGenerated]
	public GClass1354 Gclass1354_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1354 Gclass1354_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1354 Gclass1354_2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1354 Gclass1354_3;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1354 Gclass1354_4;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1360 Gclass1360_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1360 Gclass1360_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1360 Gclass1360_2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1360 Gclass1360_3;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1360 Gclass1360_4;

	[NonSerialized]
	public Dictionary<string, uint> Dictionary_0 = new Dictionary<string, uint>();

	public GClass1354 PacketsQueueCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1354_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1354_0 = value;
		}
	}

	public GClass1354 ReceivedPacketsCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1354_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass1354_1 = value;
		}
	}

	public GClass1354 ProcessedPacketsCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1354_2;
		}
		[CompilerGenerated]
		set
		{
			Gclass1354_2 = value;
		}
	}

	public GClass1354 SkippedPacketsCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1354_3;
		}
		[CompilerGenerated]
		set
		{
			Gclass1354_3 = value;
		}
	}

	public GClass1354 UnprocessedPacketsCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1354_4;
		}
		[CompilerGenerated]
		set
		{
			Gclass1354_4 = value;
		}
	}

	public GClass1360 MaxPacketsQueueCount
	{
		[CompilerGenerated]
		get
		{
			return Gclass1360_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1360_0 = value;
		}
	}

	public GClass1360 MaxPacketsQueueTime
	{
		[CompilerGenerated]
		get
		{
			return Gclass1360_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass1360_1 = value;
		}
	}

	public GClass1360 MaxRtt
	{
		[CompilerGenerated]
		get
		{
			return Gclass1360_2;
		}
		[CompilerGenerated]
		set
		{
			Gclass1360_2 = value;
		}
	}

	public GClass1360 MaxLoss
	{
		[CompilerGenerated]
		get
		{
			return Gclass1360_3;
		}
		[CompilerGenerated]
		set
		{
			Gclass1360_3 = value;
		}
	}

	public GClass1360 MaxClientServerTimeDiff
	{
		[CompilerGenerated]
		get
		{
			return Gclass1360_4;
		}
		[CompilerGenerated]
		set
		{
			Gclass1360_4 = value;
		}
	}

	public uint[] MaxClientServerTimeDiffByPlayerValues => Dictionary_0.Values.ToArray();

	public void CreateMeasurers()
	{
		int framesCountForAvgForTimeMeasurers = BackendConfigAbstractClass.Config.Log.FramesCountForAvgForTimeMeasurers;
		PacketsQueueCount = GClass1355.CreateAvgMeasurer("PacketsQueueCount", framesCountForAvgForTimeMeasurers);
		ReceivedPacketsCount = GClass1355.CreateAvgMeasurer("ReceivedPacketsCount", framesCountForAvgForTimeMeasurers);
		ProcessedPacketsCount = GClass1355.CreateAvgMeasurer("ProcessedPacketsCount", framesCountForAvgForTimeMeasurers);
		SkippedPacketsCount = GClass1355.CreateAvgMeasurer("SkippedPacketsCount", framesCountForAvgForTimeMeasurers);
		UnprocessedPacketsCount = GClass1355.CreateAvgMeasurer("UnprocessedPacketsCount", framesCountForAvgForTimeMeasurers);
		MaxPacketsQueueCount = GClass1355.CreateMaxMeasurer("MaxPacketsQueueCount", framesCountForAvgForTimeMeasurers);
		MaxPacketsQueueTime = GClass1355.CreateMaxMeasurer("MaxPacketsQueueTime", framesCountForAvgForTimeMeasurers);
		MaxRtt = GClass1355.CreateMaxMeasurer("MaxRtt", framesCountForAvgForTimeMeasurers);
		MaxLoss = GClass1355.CreateMaxMeasurer("MaxLoss", framesCountForAvgForTimeMeasurers);
		MaxClientServerTimeDiff = GClass1355.CreateMaxMeasurer("MaxClientServerTimeDiff", framesCountForAvgForTimeMeasurers);
	}

	public void Commit()
	{
		PacketsQueueCount.Commit();
		ReceivedPacketsCount.Commit();
		ProcessedPacketsCount.Commit();
		SkippedPacketsCount.Commit();
		UnprocessedPacketsCount.Commit();
		MaxPacketsQueueCount.Commit();
		MaxPacketsQueueTime.Commit();
		MaxRtt.Commit();
		MaxLoss.Commit();
		MaxClientServerTimeDiff.Commit();
	}

	public void TrackMaxClientServerTimeDiffByPlayer(string player, uint value)
	{
		uint num = (Dictionary_0.ContainsKey(player) ? Dictionary_0[player] : 0u);
		if (value > num)
		{
			Dictionary_0[player] = value;
		}
	}

	public override string ToString()
	{
		return $"ReceivedPacketsCount={ReceivedPacketsCount.MeasureStatistics.Average:F1} ProcessedPacketsCount={ProcessedPacketsCount.MeasureStatistics.Average:F1} SkippedPacketsCount={SkippedPacketsCount.MeasureStatistics.Average:F1} UnprocessedPacketsCount={UnprocessedPacketsCount.MeasureStatistics.Average:F1} PacketsQueueCount={PacketsQueueCount.MeasureStatistics.Average:F1} MaxPacketsQueueCount={MaxPacketsQueueCount.MeasureStatistics.Average:F1} MaxPacketsQueueTime={MaxPacketsQueueTime.MeasureStatistics.Average:F1} MaxRtt={MaxRtt.MeasureStatistics.Average:F1} MaxLoss={MaxLoss.MeasureStatistics.Average:F1} MaxClientServerTimeDiff={MaxClientServerTimeDiff.MeasureStatistics.Average:F1}";
	}
}
