using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIVoxelesData : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class148
	{
		public static readonly Class148 class148_0 = new Class148();

		public static Func<NavGraphVoxelSimple, bool> func_0;

		public static Func<NavGraphVoxelSimple, bool> func_1;

		public bool method_0(NavGraphVoxelSimple x)
		{
			return x.HaveStartPointId;
		}

		public bool method_1(NavGraphVoxelSimple x)
		{
			return x.HaveNavMesh;
		}
	}

	public NavGraphVoxelSimple[,,] VoxelesArray;

	public List<NavGraphVoxelSimple> VoxelsList;

	public Vector3 MinVoxelesValues;

	public Vector3 MaxVoxelesValues;

	public bool DrawVoxeles;

	public bool DrawClosestsPoints;

	public bool DrawDoorLinks;

	public float DistDrawVoxeles;

	public int MaxX;

	public int MaxY;

	public int MaxZ;

	public void RestoreData(Dictionary<int, GroupPoint> data, AICoversData covers)
	{
		VoxelesArray = new NavGraphVoxelSimple[(int)MaxVoxelesValues.x, (int)MaxVoxelesValues.y, (int)MaxVoxelesValues.z];
		int num = VoxelsList.Count((NavGraphVoxelSimple x) => x.HaveStartPointId);
		int num2 = VoxelsList.Count((NavGraphVoxelSimple x) => x.HaveNavMesh);
		foreach (NavGraphVoxelSimple voxels in VoxelsList)
		{
			VoxelesArray[voxels.IndexX, voxels.IndexY, voxels.IndexZ] = voxels;
			voxels.Restore(data);
		}
		if (!Application.isPlaying)
		{
			Debug.Log($"voxelesWithPoints:{num}  voxelesWithNavMesh:{num2}");
		}
		NavMeshDoorLink[] array = GClass870.FindUnityObjectsOfType<NavMeshDoorLink>();
		List<AILootPoint> containerLootPoints = covers.Patrols.ContainerLootPoints;
		List<AILootPoint> simpleLootPoints = covers.Patrols.SimpleLootPoints;
		List<AIExfiltrationPoint> exfiltrationPoints = covers.Patrols.ExfiltrationPoints;
		Dictionary<int, NavMeshDoorLink> dictionary = new Dictionary<int, NavMeshDoorLink>();
		Dictionary<int, AILootPoint> dictionary2 = new Dictionary<int, AILootPoint>();
		Dictionary<int, AIExfiltrationPoint> dictionary3 = new Dictionary<int, AIExfiltrationPoint>();
		NavMeshDoorLink[] array2 = array;
		foreach (NavMeshDoorLink navMeshDoorLink in array2)
		{
			if (!dictionary.ContainsKey(navMeshDoorLink.Id))
			{
				dictionary.Add(navMeshDoorLink.Id, navMeshDoorLink);
			}
		}
		if (containerLootPoints != null)
		{
			foreach (AILootPoint item in containerLootPoints)
			{
				if (!dictionary2.ContainsKey(item.Id))
				{
					dictionary2.Add(item.Id, item);
				}
			}
		}
		if (simpleLootPoints != null)
		{
			foreach (AILootPoint item2 in simpleLootPoints)
			{
				if (!dictionary2.ContainsKey(item2.Id))
				{
					dictionary2.Add(item2.Id, item2);
				}
			}
		}
		if (exfiltrationPoints != null)
		{
			foreach (AIExfiltrationPoint item3 in exfiltrationPoints)
			{
				if (!dictionary3.ContainsKey(item3.Id))
				{
					dictionary3.Add(item3.Id, item3);
				}
			}
		}
		foreach (NavGraphVoxelSimple voxels2 in VoxelsList)
		{
			if (voxels2.DoorLinksIds == null)
			{
				continue;
			}
			if (voxels2.ExfiltrationPoints == null)
			{
				voxels2.ExfiltrationPoints = new List<AIExfiltrationPoint>();
			}
			voxels2.DoorLinks = new List<NavMeshDoorLink>();
			foreach (int doorLinksId in voxels2.DoorLinksIds)
			{
				if (dictionary.TryGetValue(doorLinksId, out var value))
				{
					voxels2.DoorLinks.Add(value);
				}
			}
			foreach (int lootPointsId in voxels2.LootPointsIds)
			{
				if (dictionary2.TryGetValue(lootPointsId, out var value2))
				{
					voxels2.LootPoints.Add(value2);
				}
			}
			foreach (int exfiltrationPointsId in voxels2.ExfiltrationPointsIds)
			{
				if (dictionary3.TryGetValue(exfiltrationPointsId, out var value3))
				{
					voxels2.ExfiltrationPoints.Add(value3);
				}
			}
		}
	}

	public void SetMaxValues(Vector3 max)
	{
		MaxVoxelesValues = max;
		MaxX = (int)MaxVoxelesValues.x;
		MaxY = (int)MaxVoxelesValues.y;
		MaxZ = (int)MaxVoxelesValues.z;
	}

	public bool IsRestored()
	{
		if (VoxelesArray != null)
		{
			return VoxelesArray.Length > 1;
		}
		return false;
	}

	public void Clear()
	{
		VoxelsList = new List<NavGraphVoxelSimple>();
	}

	public void OnDrawGizmosSelected()
	{
		if (!DrawVoxeles)
		{
			return;
		}
		Vector3 position = Camera.current.transform.position;
		float num = DistDrawVoxeles * DistDrawVoxeles;
		foreach (NavGraphVoxelSimple voxels in VoxelsList)
		{
			if (!((voxels.Position - position).sqrMagnitude > num))
			{
				voxels.DrawGizmos(DrawClosestsPoints, DrawDoorLinks);
			}
		}
	}

	public void OptimizeSerialization()
	{
		VoxelesArray = new NavGraphVoxelSimple[0, 0, 0];
	}

	public void RemovePoint(GroupPoint point)
	{
		foreach (NavGraphVoxelSimple voxels in VoxelsList)
		{
			voxels.RemovePoint(point);
		}
	}
}
