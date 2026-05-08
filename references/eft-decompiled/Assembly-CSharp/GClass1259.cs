using System.Collections.Generic;
using UnityEngine;

public class GClass1259 : GClass1258
{
	public Dictionary<int, Matrix4x4[]> detailInstanceList;

	public Dictionary<int, ComputeBuffer> detailInstanceBuffers;

	public float[] heightMapData;

	public List<int[]> detailMapData;

	public List<int> totalDetailCounts;

	public Vector3 instanceStartPosition;

	public GClass1259(int coordX, int coordZ)
	{
		base.coordX = coordX;
		coordY = 0;
		base.coordZ = coordZ;
	}
}
