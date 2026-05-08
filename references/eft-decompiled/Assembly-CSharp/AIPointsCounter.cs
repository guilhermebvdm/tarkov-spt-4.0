using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AIPointsCounter
{
	public const float POSSIBLE_TOTAL_ANG = 35f;

	[NonSerialized]
	public const float MIN_SEGMENT_LENGHT_TO_ADD = 2f;

	[SerializeField]
	public List<NavEdge> _edges = new List<NavEdge>();

	public bool IsAlwaysGood;

	[NonSerialized]
	public AITriangleData Triangle;

	public int CorePointId;

	public AIPointsCounter(AITriangleData triangle)
	{
		Triangle = triangle;
	}

	public void Add(NavEdge edge)
	{
		_edges.Add(edge);
	}

	public List<GClass377> CalculateSegments()
	{
		List<GClass374> list = new List<GClass374>();
		List<GClass375> list2 = new List<GClass375>();
		for (int i = 0; i < _edges.Count - 1; i++)
		{
			GClass374 item = new GClass374(_edges[i], Triangle.AddCut, CorePointId);
			list.Add(item);
		}
		GClass374 s = list[list.Count - 1];
		GClass374 s2 = list[0];
		list2.Add(new GClass375(s, s2));
		for (int j = 0; j < list.Count - 1; j++)
		{
			s = list[j];
			s2 = list[j + 1];
			list2.Add(new GClass375(s, s2));
		}
		Dictionary<int, GClass377> dictionary = method_0(list2);
		int num = dictionary.Count + 100;
		List<GClass377> list3 = dictionary.Values.ToList();
		foreach (GClass374 item2 in list)
		{
			if (item2.GroupId <= 0 && item2.Magnitude > 2f)
			{
				GClass377 gClass = new GClass377(num++, this);
				gClass.TryAdd(item2, 180f);
				list3.Add(gClass);
			}
		}
		return list3;
	}

	public Dictionary<int, GClass377> method_0(List<GClass375> pairs)
	{
		Dictionary<int, GClass377> dictionary = new Dictionary<int, GClass377>();
		int num = 1;
		pairs.Sort(method_1);
		foreach (GClass375 pair in pairs)
		{
			if (pair.S1.GroupId > 0 && pair.S2.GroupId > 0)
			{
				continue;
			}
			if (pair.S1.GroupId > 0)
			{
				dictionary[pair.S1.GroupId].TryAdd(pair.S2, pair.Ang);
				continue;
			}
			if (pair.S2.GroupId > 0)
			{
				dictionary[pair.S2.GroupId].TryAdd(pair.S1, pair.Ang);
				continue;
			}
			GClass377 gClass = new GClass377(num, this);
			if (gClass.TryAdd(pair))
			{
				dictionary.Add(gClass.Id, gClass);
			}
			num++;
		}
		return dictionary;
	}

	public int method_1(GClass375 x, GClass375 y)
	{
		if (x.Ang < y.Ang)
		{
			return 1;
		}
		if (x.Ang > y.Ang)
		{
			return -1;
		}
		return 0;
	}

	public void DebugDrawCounter(float duration = 10f)
	{
		Color cyan = Color.cyan;
		Vector3 vector = Vector3.up * 0.3f;
		for (int i = 0; i < _edges.Count; i++)
		{
			NavEdge navEdge = _edges[i];
			NavPoint point = navEdge.Point1;
			NavPoint point2 = navEdge.Point2;
			Debug.DrawLine(point.Pos + vector, point2.Pos + vector, cyan, duration);
		}
	}

	public Vector3 GetCenterPoint()
	{
		Vector3 zero = Vector3.zero;
		foreach (NavEdge edge in _edges)
		{
			zero += edge.GetPos();
		}
		return zero / _edges.Count;
	}

	public NavPoint RandomPointFirst()
	{
		return _edges.First().Point1;
	}

	public void SetCorePoint(AICorePoint holderTestObject)
	{
		CorePointId = holderTestObject.Id;
	}
}
