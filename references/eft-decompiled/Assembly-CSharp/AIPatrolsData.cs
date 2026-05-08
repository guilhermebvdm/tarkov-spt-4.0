using System.Collections.Generic;
using EFT.Interactive;
using UnityEngine;

public class AIPatrolsData : MonoBehaviour
{
	public List<AILootPoint> ContainerLootPoints;

	public List<AILootPoint> SimpleLootPoints;

	public List<AILootPointsCluster> LootPointClusters;

	public float LootClusterRadius = 20f;

	public int MinLootPointsInCluster = 10;

	public int MinClusterLootScore = 15;

	public List<AIExfiltrationPoint> ExfiltrationPoints;

	public bool DrawAlways;

	public bool UseDrawIndex;

	public void Restore(AICorePointHolder coresHolder)
	{
		if (ContainerLootPoints == null)
		{
			ContainerLootPoints = new List<AILootPoint>();
		}
		foreach (AILootPoint containerLootPoint in ContainerLootPoints)
		{
			containerLootPoint.Restore(coresHolder);
		}
		if (SimpleLootPoints != null)
		{
			foreach (AILootPoint simpleLootPoint in SimpleLootPoints)
			{
				simpleLootPoint.Restore(coresHolder);
			}
		}
		if (ExfiltrationPoints == null)
		{
			ExfiltrationPoints = new List<AIExfiltrationPoint>();
		}
		foreach (AIExfiltrationPoint exfiltrationPoint in ExfiltrationPoints)
		{
			exfiltrationPoint.Restore(coresHolder);
		}
	}

	public void RestoreLoot(GClass1404 lootData, IEnumerable<LootableContainer> containers)
	{
		for (int i = 0; i < lootData.Count; i++)
		{
			LootItemPositionClass lootItemPositionClass = lootData[i];
			if (SimpleLootPoints == null)
			{
				continue;
			}
			foreach (AILootPoint simpleLootPoint in SimpleLootPoints)
			{
				if (lootItemPositionClass.Id.Contains(simpleLootPoint.LootSpawnId))
				{
					simpleLootPoint.RestoreLoot(lootItemPositionClass);
					break;
				}
			}
		}
		foreach (LootableContainer container in containers)
		{
			for (int j = 0; j < ContainerLootPoints.Count; j++)
			{
				AILootPoint aILootPoint = ContainerLootPoints[j];
				if (container.Id == aILootPoint.LootableContainerId)
				{
					aILootPoint.RestoreContainer(container);
					break;
				}
			}
		}
	}

	public void method_0()
	{
		if (ContainerLootPoints == null)
		{
			return;
		}
		int num = 0;
		foreach (AILootPoint containerLootPoint in ContainerLootPoints)
		{
			containerLootPoint.DrawGizmos(num);
			if (UseDrawIndex)
			{
				num++;
			}
		}
		foreach (AIExfiltrationPoint exfiltrationPoint in ExfiltrationPoints)
		{
			exfiltrationPoint.DrawGizmos();
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!DrawAlways)
		{
			method_0();
		}
	}

	public void OnDrawGizmos()
	{
		if (!DrawAlways)
		{
			return;
		}
		method_0();
		foreach (AILootPointsCluster lootPointCluster in LootPointClusters)
		{
			lootPointCluster.DrawGizmos();
		}
	}
}
