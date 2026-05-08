using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NavTriangle
{
	[NonSerialized]
	public List<NavEdge> NavEgdes;

	[NonSerialized]
	public List<NavPoint> Points = new List<NavPoint>();

	public Vector3 Position;

	public List<int> PosIndexes = new List<int>();

	public int Id;

	public List<int> EdgesIds = new List<int>();

	public float Size;

	public NavTriangle(Func<int> getId, NavPoint p1, NavPoint p2, NavPoint p3)
	{
		Id = getId();
		Points.Add(p1);
		Points.Add(p2);
		Points.Add(p3);
		PosIndexes.Add(p1.Id);
		PosIndexes.Add(p2.Id);
		PosIndexes.Add(p3.Id);
		NavEdge navEdge = new NavEdge(getId(), p1, p2, this);
		NavEdge navEdge2 = new NavEdge(getId(), p2, p3, this);
		NavEdge navEdge3 = new NavEdge(getId(), p3, p1, this);
		NavEgdes = new List<NavEdge>();
		NavEgdes.Add(navEdge);
		NavEgdes.Add(navEdge2);
		NavEgdes.Add(navEdge3);
		EdgesIds.Add(navEdge.Id);
		EdgesIds.Add(navEdge2.Id);
		EdgesIds.Add(navEdge3.Id);
		method_1();
		method_0();
	}

	public void Restore(Dictionary<int, NavPoint> dc, Dictionary<int, NavEdge> dcEdge)
	{
		Points = new List<NavPoint>();
		foreach (int posIndex in PosIndexes)
		{
			Points.Add(dc[posIndex]);
		}
		NavEgdes = new List<NavEdge>();
		foreach (int edgesId in EdgesIds)
		{
			NavEdge item = dcEdge[edgesId];
			NavEgdes.Add(item);
		}
	}

	public void AddPoint(NavPoint navPoint)
	{
		PosIndexes.Add(navPoint.Id);
		Points.Add(navPoint);
		method_1();
	}

	public void AddEdge(NavEdge edge)
	{
		EdgesIds.Add(edge.Id);
		NavEgdes.Add(edge);
		edge.Triangle = this;
		edge.IdTriangle = Id;
		method_0();
	}

	public void DebugDraw(float up, Color color, float duration = 10f)
	{
		Vector3 vector = Vector3.up * up;
		foreach (NavEdge navEgde in NavEgdes)
		{
			Debug.DrawLine(navEgde.Point1.Pos + vector, vector + navEgde.Point2.Pos, color, duration);
		}
	}

	public void method_0()
	{
		if (NavEgdes.Count != 3)
		{
			Size = -1f;
			return;
		}
		float magnitude = NavEgdes[0].GetMagnitude();
		float magnitude2 = NavEgdes[1].GetMagnitude();
		float magnitude3 = NavEgdes[2].GetMagnitude();
		float num = (magnitude + magnitude2 + magnitude3) * 0.5f;
		Size = Mathf.Sqrt(num * (num - magnitude) * (num - magnitude2) * (num - magnitude3));
	}

	public void method_1()
	{
		if (Points.Count <= 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		foreach (NavPoint point in Points)
		{
			zero += point.Pos;
		}
		Position = zero / Points.Count;
	}
}
