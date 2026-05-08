using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass377
{
	[Serializable]
	[CompilerGenerated]
	public class Class106
	{
		public static readonly Class106 class106_0 = new Class106();

		public static Func<GClass374, float> func_0;

		public float method_0(GClass374 x)
		{
			return x.Magnitude;
		}
	}

	[NonSerialized]
	public AIPointsCounter AipointsCounter_0;

	public int Id;

	public float collectedAng;

	public List<GClass374> Segments = new List<GClass374>();

	[NonSerialized]
	public Dictionary<NavPoint, List<GClass374>> Dictionary_0 = new Dictionary<NavPoint, List<GClass374>>();

	public int Count => Segments.Count;

	public GClass377(int id, AIPointsCounter parent)
	{
		AipointsCounter_0 = parent;
		Id = id;
	}

	public bool TryAdd(GClass375 pair)
	{
		float num = 180f - pair.Ang;
		if (num < 35f)
		{
			collectedAng += num;
			method_0(pair.S1);
			method_0(pair.S2);
			return true;
		}
		return false;
	}

	public bool TryAdd(GClass374 s1, float ang)
	{
		float num = 180f - ang;
		if (collectedAng + num < 35f)
		{
			collectedAng += num;
			method_0(s1);
			return true;
		}
		return false;
	}

	public List<GClass376> GetBorderPoints()
	{
		List<GClass376> list = new List<GClass376>();
		foreach (KeyValuePair<NavPoint, List<GClass374>> item in Dictionary_0)
		{
			if (item.Value.Count == 1)
			{
				list.Add(new GClass376(item.Key, item.Value[0]));
			}
		}
		return list;
	}

	public GClass376 GetNextPoint(GClass376 prev)
	{
		NavPoint other = prev.Segment.GetOther(prev.Point);
		if (Dictionary_0.TryGetValue(other, out var value))
		{
			foreach (GClass374 item in value)
			{
				if (item != prev.Segment)
				{
					return new GClass376(other, item);
				}
			}
		}
		return null;
	}

	public void method_0(GClass374 segment)
	{
		Segments.Add(segment);
		segment.GroupId = Id;
		if (!Dictionary_0.TryGetValue(segment.P1, out var value))
		{
			value = new List<GClass374>();
			Dictionary_0.Add(segment.P1, value);
		}
		value.Add(segment);
		if (!Dictionary_0.TryGetValue(segment.P2, out var value2))
		{
			value2 = new List<GClass374>();
			Dictionary_0.Add(segment.P2, value2);
		}
		value2.Add(segment);
	}

	public float GetTotalDist()
	{
		return Segments.Sum((GClass374 x) => x.Magnitude);
	}

	public void DebugDraw(float up)
	{
		foreach (GClass374 segment in Segments)
		{
			segment.DebugDraw(Vector3.up * up);
		}
	}
}
