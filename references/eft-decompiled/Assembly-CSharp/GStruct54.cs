using EFT;
using UnityEngine;

public struct GStruct54 : ISerializer<Bounds>
{
	public Vector3 center;

	public Vector3 extents;

	public Bounds Deserialize()
	{
		return new Bounds(center, extents * 2f);
	}

	public object Serialize(Bounds t)
	{
		center = t.center;
		extents = t.extents;
		return this;
	}
}
