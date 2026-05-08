using EFT;
using UnityEngine;

public struct GStruct52 : ISerializer<Vector3>
{
	public float x;

	public float y;

	public float z;

	public Vector3 Deserialize()
	{
		return new Vector3(x, y, z);
	}

	public object Serialize(Vector3 t)
	{
		x = t.x;
		y = t.y;
		z = t.z;
		return this;
	}
}
