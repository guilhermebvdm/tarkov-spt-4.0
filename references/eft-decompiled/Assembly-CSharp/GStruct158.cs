using UnityEngine;

public struct GStruct158
{
	public double x;

	public double y;

	public GStruct158(Vector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public GStruct159 ToDoubleVector3()
	{
		return new GStruct159(x, y);
	}

	public GStruct158(double _x, double _y)
	{
		x = _x;
		y = _y;
	}
}
