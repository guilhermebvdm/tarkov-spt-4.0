using UnityEngine;

public class GClass739
{
	public readonly int Id;

	public readonly int Layer;

	public readonly bool IsTrigger;

	public readonly GStruct32 Obb;

	public GClass739(int id, int layer, bool isTrigger, Vector3 center, Vector3 size, Quaternion orientation)
	{
		Id = id;
		Layer = layer;
		IsTrigger = isTrigger;
		Obb = new GStruct32(center, size, orientation);
	}
}
