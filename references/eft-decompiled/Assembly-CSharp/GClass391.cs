using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass391
{
	public delegate int GDelegate2(CustomNavigationPoint point, CoverSearchData data);

	[NonSerialized]
	public List<HashSet<CustomNavigationPoint>> List_0 = new List<HashSet<CustomNavigationPoint>>();

	[NonSerialized]
	public List<int> List_1 = new List<int>();

	[NonSerialized]
	public List<float> List_2 = new List<float>();

	[NonSerialized]
	public HashSet<CustomNavigationPoint> HashSet_0 = new HashSet<CustomNavigationPoint>();

	public Tuple<float, float> GetSearchRadius(int iteration)
	{
		if (iteration + 1 < List_2.Count)
		{
			return new Tuple<float, float>(List_2[iteration], List_2[iteration + 1]);
		}
		return new Tuple<float, float>(-1f, -1f);
	}

	public HashSet<CustomNavigationPoint> GetNearGroups(CustomNavigationPoint[] allCoverPoints, CoverSearchData data, int recursionLevel)
	{
		if (recursionLevel == 0)
		{
			method_0(data);
			GDelegate2 gDelegate = method_1;
			foreach (CustomNavigationPoint customNavigationPoint in allCoverPoints)
			{
				if (!customNavigationPoint.IsSpotted && customNavigationPoint.IsFreeById(data.Bot.Id))
				{
					int index = gDelegate(customNavigationPoint, data);
					List_0[index].Add(customNavigationPoint);
					List_1[index]++;
				}
			}
		}
		if (recursionLevel < List_0.Count && recursionLevel >= 0)
		{
			_ = List_0[recursionLevel].Count;
			_ = List_1[recursionLevel];
			return List_0[recursionLevel];
		}
		if (HashSet_0.Count > 0)
		{
			HashSet_0.Clear();
		}
		return HashSet_0;
	}

	public void method_0(CoverSearchData data)
	{
		while (List_0.Count <= data.MaxIterations)
		{
			List_0.Add(new HashSet<CustomNavigationPoint>());
			List_1.Add(0);
		}
		while (List_2.Count <= data.MaxIterations)
		{
			List_2.Add(0f);
		}
		List_0[0].Clear();
		List_0[1].Clear();
		List_2[0] = data.MinDistSqr;
		List_2[1] = data.MaxDistSqr;
		float num = Mathf.Sqrt(List_2[1]);
		List_1[0] = 0;
		List_1[1] = 0;
		for (int i = 2; i <= data.MaxIterations; i++)
		{
			List_1[i] = 0;
			List_0[i].Clear();
			num += LocalBotSettingsProviderClass.Core.COVER_SEARCH_METERS_ADD;
			List_2[i] = num * num;
		}
	}

	public int method_1(CustomNavigationPoint point, CoverSearchData data)
	{
		float sqrMagnitude = (data.CenterPos - point.Position).sqrMagnitude;
		return method_2(sqrMagnitude, data.MaxIterations);
	}

	public int method_2(float sDist, int maxIterations)
	{
		int num = 0;
		int num2 = maxIterations;
		int num3 = 1000;
		while (num < num2 && --num3 > 0)
		{
			int num4 = (num + num2) / 2;
			if (!(List_2[num4] <= sDist) || sDist > List_2[num4 + 1])
			{
				if (!(List_2[num] <= sDist) || sDist > List_2[num + 1])
				{
					if (!(List_2[num2 - 1] <= sDist) || sDist > List_2[num2])
					{
						if (List_2[num + 1] <= sDist && sDist <= List_2[num4])
						{
							num2 = num4;
							continue;
						}
						if (num4 > num)
						{
							num = num4;
							continue;
						}
						return num4;
					}
					return num2 - 1;
				}
				return num;
			}
			return num4;
		}
		return num2;
	}

	public void Dispose()
	{
		List_0.Clear();
		List_1.Clear();
		List_2.Clear();
		HashSet_0.Clear();
	}
}
