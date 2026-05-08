using UnityEngine;

public class GClass1665
{
	public double x;

	public double y;

	public double z;

	public double w;

	public GClass1665(double _x, double _y, double _z, double _w)
	{
		x = _x;
		y = _y;
		z = _z;
		w = _w;
	}

	public Vector4 ToVector4()
	{
		return new Vector4((float)x, (float)y, (float)z, (float)w);
	}

	public Vector3 ToVector3()
	{
		return new Vector3((float)x, (float)y, (float)z);
	}

	public GStruct159 ToDoubleVector3()
	{
		return new GStruct159(x, y, z);
	}
}
