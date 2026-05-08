using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotNearDoorData([NotNull] BotOwner owner) : GClass429(owner)
{
	public float RecentlyClosedDoorCheckTime;

	[NonSerialized]
	public List<NavMeshDoorLink> StubLinks = new List<NavMeshDoorLink>();

	public void Activate()
	{
		Update();
	}

	public void Update()
	{
	}

	public List<NavMeshDoorLink> CurrentDoorLinks()
	{
		if (BotOwner_0.VoxelesPersonalData.CurVoxel == null)
		{
			return StubLinks;
		}
		return BotOwner_0.VoxelesPersonalData.CurVoxel.DoorLinks;
	}

	public NavMeshDoorLink GetNearestDoor()
	{
		List<NavMeshDoorLink> list = CurrentDoorLinks();
		NavMeshDoorLink result = null;
		float num = 4f;
		for (int i = 0; i < list.Count; i++)
		{
			NavMeshDoorLink navMeshDoorLink = list[i];
			float sqrMagnitude = (navMeshDoorLink.transform.position - BotOwner_0.Transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = navMeshDoorLink;
			}
		}
		return result;
	}

	public void DrawGizmosSelected()
	{
		foreach (NavMeshDoorLink item in CurrentDoorLinks())
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(item.MidClose, BotOwner_0.MyHead.position);
		}
	}
}
