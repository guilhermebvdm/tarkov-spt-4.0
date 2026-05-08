using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass589 : GInterface12
{
	public const float COEF = 100f;

	[NonSerialized]
	public List<NavGraphEditorPathValue> List_0 = new List<NavGraphEditorPathValue>();

	[NonSerialized]
	public NavMeshDoorLink[] NavMeshDoorLink_0;

	[NonSerialized]
	[CompilerGenerated]
	public NavGraphVoxel NavGraphVoxel_0;

	public NavGraphVoxel Voxel
	{
		[CompilerGenerated]
		get
		{
			return NavGraphVoxel_0;
		}
		[CompilerGenerated]
		set
		{
			NavGraphVoxel_0 = value;
		}
	}

	public void SetPos(Vector3 pos, bool withLogs = false)
	{
	}

	public int AffectedElements()
	{
		return List_0.Count;
	}

	public int AffectedDoorElements()
	{
		if (NavMeshDoorLink_0 == null)
		{
			return 0;
		}
		return NavMeshDoorLink_0.Length;
	}

	public Vector3 GetVoxelPos()
	{
		return Voxel.Position;
	}

	public void TryDrawAffectionGizmos()
	{
		if (Voxel == null)
		{
			return;
		}
		Voxel.OnDrawGizmos(Color.red, 0.95f);
		Color yellow = Color.yellow;
		yellow.a = 0.5f;
		Gizmos.color = yellow;
		foreach (NavGraphEditorPoint point in Voxel.Points)
		{
			Gizmos.DrawSphere(point.Position, 0.3f);
		}
		yellow = Color.green;
		yellow.a = 0.5f;
		Gizmos.color = yellow;
		NavMeshDoorLink[] doorLinks = Voxel.DoorLinks;
		for (int i = 0; i < doorLinks.Length; i++)
		{
			Gizmos.DrawCube(doorLinks[i].Close1, new Vector3(0.2f, 1f, 0.2f));
		}
	}

	public void method_0()
	{
		if (List_0 != null)
		{
			foreach (NavGraphEditorPathValue item in List_0)
			{
				item.DropCoef();
			}
		}
		if (NavMeshDoorLink_0 != null)
		{
			NavMeshDoorLink[] navMeshDoorLink_ = NavMeshDoorLink_0;
			for (int i = 0; i < navMeshDoorLink_.Length; i++)
			{
				navMeshDoorLink_[i].DropCoef();
			}
		}
	}

	public void Dispose()
	{
		method_0();
	}
}
