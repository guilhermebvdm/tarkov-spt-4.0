using System;
using System.Collections.Generic;
using System.Diagnostics;

public class GClass411
{
	[NonSerialized]
	public AICoversData AicoversData_0;

	[NonSerialized]
	public Dictionary<int, List<CustomNavigationPoint>> Dictionary_0 = new Dictionary<int, List<CustomNavigationPoint>>();

	public GClass411(AICoversData data)
	{
		AicoversData_0 = data;
	}

	public void PreCache(int preCacheCount)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < preCacheCount; i++)
		{
			List<CustomNavigationPoint> value = method_0(i);
			Dictionary_0.Add(i, value);
		}
		stopwatch.Stop();
	}

	public List<CustomNavigationPoint> method_0(int botId)
	{
		List<CustomNavigationPoint> list = new List<CustomNavigationPoint>();
		foreach (GroupPoint point in AicoversData_0.Points)
		{
			list.Add(point.CreateCustomNavigationPoint(botId));
		}
		foreach (GroupPoint manualPoint in AicoversData_0.AIManualPointsHolder.ManualPoints)
		{
			list.Add(manualPoint.CreateCustomNavigationPoint(botId));
		}
		return list;
	}

	public List<CustomNavigationPoint> GetCovers(int botId)
	{
		if (Dictionary_0.TryGetValue(botId, out var value))
		{
			return value;
		}
		return method_0(botId);
	}
}
