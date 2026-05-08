using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NavGraphVoxel(Vector3 pos, int indexX, int indexY, int indexZ, int id) : NavGraphVoxelBase(pos, indexX, indexY, indexZ, id)
{
	[NonSerialized]
	public List<NavGraphEditorPoint> Points = new List<NavGraphEditorPoint>();

	[NonSerialized]
	public List<NavGraphEditorPathValue> Pathes = new List<NavGraphEditorPathValue>();

	[NonSerialized]
	public List<NavGraphVoxel> Neightbourhoods = new List<NavGraphVoxel>();

	public NavMeshDoorLink[] DoorLinks;

	public NavMeshDoorLink[] DoorLinksAffected;

	public List<int> PointsIds = new List<int>();

	public List<int> NeightbourhoodsIds = new List<int>();

	public void AddPoint(NavGraphEditorPoint newPoint)
	{
		Points.Add(newPoint);
		PointsIds.Add(newPoint.Id);
	}

	public NavGraphEditorPoint GetClosestPoint(Vector3 crn, out float sDist, int excuedPoint = -1)
	{
		sDist = float.MaxValue;
		if (Points.Count == 0)
		{
			return null;
		}
		NavGraphEditorPoint result = null;
		foreach (NavGraphEditorPoint point in Points)
		{
			if (excuedPoint <= 0 || point.Id != excuedPoint)
			{
				float sqrMagnitude = (point.Position - crn).sqrMagnitude;
				if (sqrMagnitude < sDist)
				{
					sDist = sqrMagnitude;
					result = point;
				}
			}
		}
		return result;
	}

	public Vector3 GetSize()
	{
		return new Vector3(10f, 5f, 10f);
	}

	public void OnDrawGizmos(Color color, float sizeCoef = 1f)
	{
		Gizmos.color = color;
		Vector3 size = GetSize();
		Gizmos.DrawWireCube(Position + size * 0.5f, size * sizeCoef);
		Gizmos.DrawWireCube(Position, Vector3.one * 0.01f);
		if (DoorLinks != null)
		{
			NavMeshDoorLink[] doorLinks = DoorLinks;
			foreach (NavMeshDoorLink obj in doorLinks)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(obj.Close1, new Vector3(0.1f, 2f, 0.1f));
			}
		}
	}

	public void AddNeightbourhood(NavGraphVoxel voxeles)
	{
		Neightbourhoods.Add(voxeles);
		NeightbourhoodsIds.Add(voxeles.Id);
	}

	public override string ToString()
	{
		return $"Voxel:{Position} X:{IndexX} Y:{IndexY} Z:{IndexZ}";
	}

	public void RemovePoint(NavGraphEditorPoint point)
	{
		Points.Remove(point);
		PointsIds.Remove(point.Id);
	}
}
