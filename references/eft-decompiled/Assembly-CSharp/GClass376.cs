using UnityEngine;

public class GClass376
{
	public NavPoint Point;

	public GClass374 Segment;

	public GClass376(NavPoint key, GClass374 aIPointsCounterSegment)
	{
		Point = key;
		Segment = aIPointsCounterSegment;
	}

	public Vector3 GetDirection()
	{
		return Segment.GetOther(Point).Pos - Point.Pos;
	}

	public float GetSDist()
	{
		return (Segment.P1.Pos - Segment.P2.Pos).sqrMagnitude;
	}

	public bool CreatedPoint(GClass392 np1)
	{
		return Segment.CreatedPoint(np1);
	}
}
