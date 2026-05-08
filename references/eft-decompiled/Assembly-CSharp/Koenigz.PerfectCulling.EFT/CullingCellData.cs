using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
public class CullingCellData : ISpatialItem, IBinarySerializable
{
	public OrientedBounds CellBounds;

	public List<CullingGroupData> CullingData;

	public List<Vector3> IndividualSampleProbes;

	public bool UserModified;

	public bool Ignored;

	public List<int> Connections;

	[NonSerialized]
	public int RuntimeCellId;

	public Bounds SpatialItemBounds => CellBounds.AABB;

	public Vector3 ClosestPointOriented(Vector3 p, out bool isPointInside)
	{
		return CellBounds.ClosestPointOriented(p, out isPointInside);
	}

	public void Read(BinaryReader reader)
	{
		CellBounds.Read(reader);
		CullingData = new List<CullingGroupData>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			CullingGroupData cullingGroupData = new CullingGroupData();
			cullingGroupData.Read(reader);
			CullingData.Add(cullingGroupData);
		}
	}

	public void Write(BinaryWriter writer)
	{
		CellBounds.Write(writer);
		int num = ((CullingData != null) ? CullingData.Count : 0);
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			CullingData[i].Write(writer);
		}
	}
}
