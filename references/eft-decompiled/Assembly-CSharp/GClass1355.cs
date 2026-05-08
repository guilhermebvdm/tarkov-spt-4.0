using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class GClass1355
{
	[CompilerGenerated]
	public class Class910
	{
		public string groupName;

		public bool method_0(GClass1361 x)
		{
			return x.GroupName == groupName;
		}
	}

	[CompilerGenerated]
	public class Class911
	{
		public string groupName;

		public bool method_0(GClass1359 x)
		{
			return x.GroupName == groupName;
		}
	}

	public static readonly List<GClass1361> TIME_MEASURERS = new List<GClass1361>(100);

	public static readonly List<GClass1359> COUNTERS = new List<GClass1359>(100);

	public static readonly List<GClass1362> VALUE_CHANGES = new List<GClass1362>(100);

	public static readonly List<GClass1354> AVGS = new List<GClass1354>(100);

	public static readonly List<GClass1360> MAXS = new List<GClass1360>(100);

	public static GClass1361 CreateMeasureTimer(string groupName, int valuesForAverageCalculation = 60)
	{
		GClass1361 gClass = new GClass1361(groupName, new GClass1353(groupName, valuesForAverageCalculation));
		TIME_MEASURERS.Add(gClass);
		return gClass;
	}

	public static GClass1362 CreateValueChangeMeasurer(string groupName, int valuesForAverageCalculation = 60)
	{
		GClass1362 gClass = new GClass1362(groupName, new GClass1353(groupName, valuesForAverageCalculation));
		VALUE_CHANGES.Add(gClass);
		return gClass;
	}

	public static GClass1359 CreateDiagnosticsCounter(string groupName, int valuesForAverageCalculation = 60)
	{
		GClass1359 gClass = new GClass1359(groupName, new GClass1353(groupName, valuesForAverageCalculation));
		COUNTERS.Add(gClass);
		return gClass;
	}

	public static GClass1361 GetTimeMeasurer(string groupName)
	{
		return TIME_MEASURERS.Find((GClass1361 x) => x.GroupName == groupName);
	}

	public static GClass1359 GetIncrementalMeasurer(string groupName)
	{
		return COUNTERS.Find((GClass1359 x) => x.GroupName == groupName);
	}

	public static GClass1354 CreateAvgMeasurer(string groupName, int valuesForAverageCalculation = 60)
	{
		GClass1354 gClass = new GClass1354(groupName, new GClass1353(groupName, valuesForAverageCalculation));
		AVGS.Add(gClass);
		return gClass;
	}

	public static GClass1360 CreateMaxMeasurer(string groupName, int valuesForAverageCalculation = 60)
	{
		GClass1360 gClass = new GClass1360(groupName, new GClass1353(groupName, valuesForAverageCalculation));
		MAXS.Add(gClass);
		return gClass;
	}
}
