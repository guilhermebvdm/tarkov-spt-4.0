using UnityEngine;

public struct GStruct30(int mask, QueryTriggerInteraction queryTriggerInteraction, Vector3 center, Vector3 size, Quaternion orientation)
{
	public int Mask = mask;

	public QueryTriggerInteraction QueryTriggerInteraction = queryTriggerInteraction;

	public Vector3 Center = center;

	public Vector3 Size = size;

	public Quaternion Orientation = orientation;
}
