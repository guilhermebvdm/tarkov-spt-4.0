using System;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[Serializable]
public struct PortalData(int id, Vector3 position, Vector3 normal, float cost, byte state, float depth, short startIndex, short indoorRoomsCount)
{
	public int id = id;

	public Vector3 position = position;

	public Vector3 normal = normal;

	public float cost = cost;

	public float depth = depth;

	public byte state = state;

	public short startIndex = startIndex;

	public short indoorRoomsCount = indoorRoomsCount;

	public static PortalData Default()
	{
		return new PortalData(0, Vector3.zero, Vector3.zero, 1f, 1, float.MaxValue, 0, 0);
	}
}
