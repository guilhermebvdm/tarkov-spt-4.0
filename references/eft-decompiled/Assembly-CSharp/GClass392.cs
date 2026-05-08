using System;
using UnityEngine;
using UnityEngine.AI;

public class GClass392
{
	public CoverLevel CoverLevel;

	public Vector3 FirePosition;

	public Vector3 PointHit;

	public Vector3 Pos;

	public Vector3? AltPos;

	public Vector3 WallDirection;

	public int Id;

	public bool AlwaysGood;

	public PointWithNeighborType NeighborType;

	[NonSerialized]
	public static int Int_0;

	public float MagnitudeForSort;

	public NavPoint Point;

	public int CorePoint;

	public GClass392(NavPoint point, Vector3 pos, Vector3 wallDirection, int corePoint)
	{
		CorePoint = corePoint;
		Id = Int_0++;
		Pos = pos;
		Point = point;
		WallDirection = wallDirection;
		Vector3 vector = -WallDirection * 0.6f;
		Vector3 targetPosition = pos + vector;
		vector.y = 0f;
		float sqrMagnitude = vector.sqrMagnitude;
		NavMeshPath navMeshPath = new NavMeshPath();
		if (!NavMesh.CalculatePath(pos, targetPosition, -1, navMeshPath) || navMeshPath.status != NavMeshPathStatus.PathComplete)
		{
			return;
		}
		Vector3 vector2 = navMeshPath.corners[navMeshPath.corners.Length - 1];
		Vector3 vector3 = vector2 - pos;
		if (Mathf.Abs(vector3.y) < 0.2f)
		{
			vector3.y = 0f;
			if (Mathf.Abs(sqrMagnitude - vector3.sqrMagnitude) < 0.01f)
			{
				AltPos = vector2;
			}
		}
	}
}
