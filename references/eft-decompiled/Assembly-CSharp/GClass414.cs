using UnityEngine;

public class GClass414
{
	public GroupPoint Point;

	public NavTriangle Triangle;

	public Vector3 Position => Point.Position;

	public GClass414(GroupPoint point, NavTriangle triangle)
	{
		Point = point;
		Triangle = triangle;
	}
}
