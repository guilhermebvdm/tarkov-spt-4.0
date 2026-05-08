using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JsonType;
using UnityEngine;

[Serializable]
public class AILootPointsCluster
{
	[NonSerialized]
	public List<AILootPoint> LootPoints = new List<AILootPoint>();

	[SerializeField]
	public int _centralPointId;

	[SerializeField]
	public Vector3 _centerPosition;

	[NonSerialized]
	public Vector3 PointsCoordSum = Vector3.zero;

	[SerializeField]
	public int _clusterId;

	[SerializeField]
	public float _squareRadius;

	[SerializeField]
	public float _radius;

	[NonSerialized]
	public float EvaluatedCostRuntime_1;

	[NonSerialized]
	public static int NextClusterId;

	[NonSerialized]
	public const float SEPARATE_CLUSTERS_Y_DIST = 6f;

	[NonSerialized]
	public BotsGroup ReservedForGroup_1;

	[SerializeField]
	public int _connectionGroup = -1;

	[SerializeField]
	public BotZone _botZone;

	public int CentralPointId => _centralPointId;

	public Vector3 CenterPosition => _centerPosition;

	[field: NonSerialized]
	public bool CenterRebalanced { get; set; }

	public int PointsCount => LootPoints.Count;

	public int ClusterId => _clusterId;

	public float SquareRadius => _squareRadius;

	public float Radius => _radius;

	public float EvaluatedCostRuntime => EvaluatedCostRuntime_1;

	[field: NonSerialized]
	public bool Inited { get; set; }

	[field: NonSerialized]
	public bool Looted { get; set; }

	public BotsGroup ReservedForGroup => ReservedForGroup_1;

	public int ConnectionGroup => _connectionGroup;

	public BotZone BotZone => _botZone;

	public AILootPointsCluster(AILootPoint centralPoint, float radius)
	{
		PointsCoordSum = Vector3.zero;
		NextClusterId++;
		_clusterId = NextClusterId;
		_squareRadius = radius * radius;
		_radius = radius;
		_centralPointId = centralPoint.Id;
		LootPoints.Add(centralPoint);
		_centerPosition = centralPoint.Position;
		_connectionGroup = centralPoint.CorePointInGame.ConnectionGroupId;
		if (_connectionGroup <= 0)
		{
			Debug.LogError("loot cluster init error");
		}
	}

	public AILootPointsCluster(int centralPointId, Vector3 centerPosition, float radius, int connectionGroup)
	{
		PointsCoordSum = Vector3.zero;
		NextClusterId++;
		_clusterId = NextClusterId;
		_squareRadius = radius * radius;
		_radius = radius;
		_centralPointId = centralPointId;
		_centerPosition = centerPosition;
		_connectionGroup = connectionGroup;
		if (_connectionGroup <= 0)
		{
			Debug.LogError("loot cluster init error");
		}
	}

	public void LinkToBotZone(BotZone botZone)
	{
		_botZone = botZone;
	}

	public void CollectActualSpawnedLoot(List<LootItemPositionClass> allLoot)
	{
		EvaluatedCostRuntime_1 = 0f;
		for (int i = 0; i < allLoot.Count; i++)
		{
			LootItemPositionClass lootItemPositionClass = allLoot[i];
			if (GClass856.SqrDistance(lootItemPositionClass.Position, _centerPosition) <= _squareRadius)
			{
				EvaluatedCostRuntime_1 += method_0(lootItemPositionClass.Item);
			}
		}
	}

	public float method_0(Item item)
	{
		if (!item.CanBePickedUp)
		{
			return 0f;
		}
		return (float)(Singleton<HandbookClass>.Instance.GetBasePrice(item.TemplateId) * (double)item.StackObjectsCount);
	}

	public bool IsPointInClusterArea(AILootPoint pointToCheck)
	{
		if (Mathf.Abs(_centerPosition.y - pointToCheck.Position.y) > 6f)
		{
			return false;
		}
		return GClass856.SqrDistance(pointToCheck.Position, _centerPosition) < SquareRadius;
	}

	public float Evaluate(BotOwner owner)
	{
		float num = EvaluatedCostRuntime_1;
		float num2 = Vector3.Distance(owner.Position, _centerPosition);
		int num3 = 8;
		float num4 = 0.9f;
		for (int i = 0; (float)i < num2; i += num3)
		{
			num *= num4;
		}
		return num;
	}

	public int CalcRarityScore()
	{
		int num = 0;
		foreach (AILootPoint lootPoint in LootPoints)
		{
			if (lootPoint.LootSpawn != null)
			{
				switch (lootPoint.LootSpawn.Rarity)
				{
				case ELootRarity.Common:
					num++;
					break;
				case ELootRarity.Rare:
					num += 5;
					break;
				case ELootRarity.Superrare:
					num += 10;
					break;
				case ELootRarity.Not_exist:
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else if ((bool)lootPoint.LootableContainer)
			{
				int num2 = 0;
				GClass3709[] trees = lootPoint.LootableContainer.Trees;
				foreach (GClass3709 gClass in trees)
				{
					num2 += gClass.Quantity;
				}
				num += (int)((float)num2 * 0.5f);
			}
		}
		return num;
	}

	public void RebalanceCluster()
	{
		Vector3 b = PointsCoordSum / LootPoints.Count;
		float num = float.MaxValue;
		AILootPoint aILootPoint = null;
		for (int i = 0; i < LootPoints.Count; i++)
		{
			AILootPoint aILootPoint2 = LootPoints[i];
			float num2 = GClass856.SqrDistance(aILootPoint2.Position, b);
			if (num2 < num)
			{
				num = num2;
				aILootPoint = aILootPoint2;
			}
		}
		if (aILootPoint != null && _centralPointId != aILootPoint.Id)
		{
			_centralPointId = aILootPoint.Id;
			_centerPosition = aILootPoint.Position;
			CenterRebalanced = true;
		}
	}

	public void MarkLooted()
	{
		Looted = true;
	}

	public void AddPoint(AILootPoint lootPoint)
	{
		if (ConnectionGroup > 0 && lootPoint.CorePointInGame.ConnectionGroupId != ConnectionGroup)
		{
			Debug.LogError($"Wrong set connection group;{lootPoint.CorePointInGame.ConnectionGroupId} != {ConnectionGroup} ");
		}
		LootPoints.Add(lootPoint);
		lootPoint.ClusterId = _clusterId;
		PointsCoordSum += lootPoint.Position;
	}

	public void ReserveForGroup(BotsGroup group)
	{
		if (group.BossGroup != null)
		{
			ReservedForGroup_1 = group;
		}
		else
		{
			ReservedForGroup_1 = null;
		}
	}

	public bool CheckClusterInBossZone(BotsGroup group)
	{
		BotsController botsController = Singleton<IBotGame>.Instance?.BotsController;
		if (!(_botZone == null) && botsController != null)
		{
			foreach (BotOwner botOwner in botsController.Bots.BotOwners)
			{
				if (botOwner.Boss.IamBoss && botOwner.SpawnBotZone == _botZone && botOwner.BotsGroup != group)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void DrawGizmos()
	{
		Gizmos.DrawSphere(_centerPosition, Radius * 0.2f);
	}
}
