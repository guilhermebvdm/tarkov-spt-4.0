using System;
using UnityEngine;

[Serializable]
public abstract class NavGraphVoxelBase
{
	public const float SIZE = 10f;

	public const float SIZE_Y = 5f;

	public Vector3 Position;

	public ushort IndexX;

	public ushort IndexZ;

	public ushort IndexY;

	public ushort Id;

	public NavGraphVoxelBase(Vector3 pos, int indexX, int indexY, int indexZ, int id)
	{
		Id = (ushort)id;
		Position = pos;
		IndexX = (ushort)indexX;
		IndexY = (ushort)indexY;
		IndexZ = (ushort)indexZ;
	}
}
