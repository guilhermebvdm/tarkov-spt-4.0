using EFT;
using UnityEngine;

public class GClass1878 : ISerializer<GClass1877>
{
	public float px;

	public float py;

	public float pz;

	public float rw;

	public float rx;

	public float ry;

	public float rz;

	public GClass1877 Deserialize()
	{
		return new GClass1877
		{
			Position = new Vector3(px, py, pz),
			Rotation = new Quaternion(rx, ry, rz, rw)
		};
	}

	public object Serialize(GClass1877 t)
	{
		px = t.Position.x;
		py = t.Position.y;
		pz = t.Position.z;
		rx = t.Rotation.x;
		ry = t.Rotation.y;
		rz = t.Rotation.z;
		rw = t.Rotation.w;
		return this;
	}
}
