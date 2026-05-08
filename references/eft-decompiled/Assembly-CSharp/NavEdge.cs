using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class NavEdge
{
	[NonSerialized]
	public NavPoint Point1;

	[NonSerialized]
	public NavPoint Point2;

	[NonSerialized]
	public NavTriangle Triangle;

	[NonSerialized]
	public NavEdge Pair;

	[NonSerialized]
	public int IdToCompare;

	[NonSerialized]
	public float Magnitude;

	public int Idpoint1;

	public int Idpoint2;

	public int IdTriangle;

	public int Id;

	[field: NonSerialized]
	public bool Deleted { get; set; }

	public bool HavePair => Pair != null;

	public NavEdge(int id, NavPoint p1, NavPoint p2, NavTriangle triangle)
	{
		Id = id;
		method_0(p1, p2);
		if (triangle != null)
		{
			Triangle = triangle;
			IdTriangle = Triangle.Id;
		}
	}

	public void CalcCompareString(int halfSize)
	{
		int value;
		int value2;
		if (Idpoint1 < Idpoint2)
		{
			value = Idpoint1;
			value2 = Idpoint2;
		}
		else
		{
			value2 = Idpoint1;
			value = Idpoint2;
		}
		int length = value2.ToString().Length;
		int num = halfSize - length;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value);
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(0);
		}
		stringBuilder.Append(value2);
		string s = stringBuilder.ToString();
		IdToCompare = int.Parse(s);
	}

	public void Restore(Dictionary<int, NavPoint> dc, Dictionary<int, NavTriangle> dcTriang)
	{
		Point1 = dc[Idpoint1];
		Point2 = dc[Idpoint2];
		Triangle = dcTriang[IdTriangle];
	}

	public float GetMagnitude()
	{
		Magnitude = (Point1.Pos - Point2.Pos).magnitude;
		return Magnitude;
	}

	public NavPoint GetOther(NavPoint key)
	{
		if (key.Id == Point1.Id)
		{
			return Point2;
		}
		return Point1;
	}

	public void MarkDeleted()
	{
		Deleted = true;
	}

	public void DebugDraw(float up, Color? color = null)
	{
		if (!color.HasValue)
		{
			color = Color.blue;
		}
		Debug.DrawLine(Point1.Pos + Vector3.up * up, Point2.Pos + Vector3.up * up, color.Value, 5f);
	}

	public override string ToString()
	{
		if (Deleted)
		{
			return $"Edge DELETED id:{Id}";
		}
		return $"Edge {Id} ({Idpoint1}_{Idpoint2}) Pair:{HavePair}";
	}

	public Vector3 GetPos()
	{
		return (Point1.Pos + Point2.Pos) * 0.5f;
	}

	public float CalcDist()
	{
		return (Point1.Pos - Point2.Pos).magnitude;
	}

	public void ChangePoints(NavPoint toRemove, NavPoint connectTo)
	{
		NavPoint p;
		NavPoint p2;
		if (Point1.Id == toRemove.Id)
		{
			p = connectTo;
			p2 = Point2;
		}
		else
		{
			p2 = connectTo;
			p = Point1;
		}
		method_0(p, p2);
	}

	public void method_0(NavPoint p1, NavPoint p2)
	{
		if (p1.Id < p2.Id)
		{
			Point1 = p1;
			Point2 = p2;
		}
		else
		{
			Point1 = p2;
			Point2 = p1;
		}
		Idpoint1 = Point1.Id;
		Idpoint2 = Point2.Id;
	}
}
