using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass117 : BaseLogicLayerSimpleAbstractClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class221
	{
		public static readonly Class221 class221_0 = new Class221();

		public static Func<GroupPoint, bool> func_0;

		public static Func<GroupPoint, bool> func_1;

		public bool method_0(GroupPoint point)
		{
			if (!point.IsSpotted)
			{
				return point.CoverType == CoverType.Wall;
			}
			return false;
		}

		public bool method_1(GroupPoint point)
		{
			if (!point.IsSpotted)
			{
				return point.CoverType == CoverType.Wall;
			}
			return false;
		}
	}

	[NonSerialized]
	public const int Int_1 = 5;

	[NonSerialized]
	public const float Float_3 = 120f;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	public bool Bool_8;

	[NonSerialized]
	public float Float_5 = float.MaxValue;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public bool Bool_9;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_1;

	[NonSerialized]
	public AICorePoint AicorePoint_0;

	[NonSerialized]
	public float Float_7 = 120f;

	public AILootPointsCluster AILootPointsCluster_0 => BotOwner_0.PatrollingData.LootData.TargetLootCluster;

	public GClass117(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public bool method_13()
	{
		if (Time.time > BotOwner_0.PatrollingData.LootData.BeginLootClusterTime + 120f && BotOwner_0.PatrollingData.LootData.ClusterLootingNow())
		{
			return true;
		}
		if (AILootPointsCluster_0 != null)
		{
			return AILootPointsCluster_0.Looted;
		}
		return false;
	}

	public BotOwner GetLeader(BotOwner target)
	{
		if (target.BotsGroup.BossGroup != null)
		{
			return target.BotsGroup.BossGroup.Boss;
		}
		if (target.BotsGroup.BossGroup == null)
		{
			return target;
		}
		BotOwner result = target.BotsGroup.Member(0);
		int num = 0;
		while (true)
		{
			if (num < target.BotsGroup.MembersCount)
			{
				BotOwner botOwner = target.BotsGroup.Member(num);
				if (num % 5 == 0)
				{
					result = botOwner;
				}
				if (botOwner == target)
				{
					break;
				}
				num++;
				continue;
			}
			return target;
		}
		return result;
	}

	public BotOwner GetLeader()
	{
		return GetLeader(BotOwner_0);
	}

	public bool IAmLeader()
	{
		if (!BotOwner_0.Boss.IamBoss)
		{
			return GetLeader() == BotOwner_0;
		}
		return true;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Bool_7 = false;
		Bool_6 = false;
		if (Bool_9)
		{
			BotOwner_0.GoToSomePointData.SetPoint(AILootPointsCluster_0.CenterPosition);
			Bool_9 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "Stucked");
		}
		Bool_9 = false;
		if (BotOwner_0.Memory.HaveEnemy)
		{
			HoldFor(UnityEngine.Random.Range(5f, 10f));
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "enemyAmbush");
		}
		if (!BotOwner_0.PatrollingData.LootData.AllGroupWantLootCluster() && AILootPointsCluster_0 != null && !AILootPointsCluster_0.Looted)
		{
			Bool_6 = true;
			HoldFor(UnityEngine.Random.Range(5f, 10f));
			method_14();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "waitGroup");
		}
		if (AILootPointsCluster_0 == null || method_13())
		{
			if (AILootPointsCluster_0 != null && method_13() && BotOwner_0.PatrollingData.LootData.ClusterLootingNow() && !AILootPointsCluster_0.Looted && Bool_5)
			{
				BotOwner_0.PatrollingData.LootData.StopLootCluster();
				AILootPointsCluster_0.MarkLooted();
			}
			if (IAmLeader())
			{
				Bool_5 = false;
				List<AILootPointsCluster> list = method_17(BotOwner_0.Settings.FileSettings.Patrol.LOOT_PATROL_POTENTIAL_CLUSTERS_AMOUNT);
				if (list.Count > 0)
				{
					BotOwner_0.PatrollingData.LootData.StopLootCluster();
					AILootPointsCluster randomItem = GClass855.GetRandomItem(list);
					BotOwner_0.PatrollingData.LootData.SetTargetLootCluster(randomItem);
					if (AILootPointsCluster_0 != null)
					{
						AILootPointsCluster_0.ReserveForGroup(BotOwner_0.BotsGroup);
					}
				}
			}
			else
			{
				BotOwner_0.PatrollingData.LootData.StopLootCluster();
				BotOwner leader = GetLeader();
				if (leader.PatrollingData.LootData.TargetLootCluster == null || leader.PatrollingData.LootData.TargetLootCluster.Looted)
				{
					HoldFor(1f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "waitLeader");
				}
				Bool_5 = false;
				BotOwner_0.PatrollingData.LootData.SetTargetLootCluster(leader.PatrollingData.LootData.TargetLootCluster);
			}
		}
		if (AILootPointsCluster_0 != null)
		{
			if (!method_21(1.2f) && !Bool_5)
			{
				if (method_21(4f))
				{
					BotOwner_0.GoToSomePointData.SetPoint(AILootPointsCluster_0.CenterPosition);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "GoToLootClstrNr");
				}
				if (Bool_8)
				{
					if (BotOwner_0.Memory.IsInCover)
					{
						BotMemoryClass memory = BotOwner_0.Memory;
						float? secToBeSpotted = 1f;
						memory.Spotted(byHit: false, null, secToBeSpotted);
					}
					BotOwner_0.GoToSomePointData.SetPoint(AILootPointsCluster_0.CenterPosition);
					Float_5 = float.MaxValue;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "GoToLootClstr1");
				}
				Bool_8 = true;
				Float_5 = float.MaxValue;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPointTactical, "GoToLootClstr2");
			}
			Bool_5 = true;
			if (!BotOwner_0.PatrollingData.LootData.HaveActions(AILootPointsCluster_0.CenterPosition, AILootPointsCluster_0.SquareRadius * 2f, 3) && !BotOwner_0.PatrollingData.LootData.HaveActions(BotOwner_0.Position, AILootPointsCluster_0.SquareRadius * 2f, 4))
			{
				if (BotOwner_0.Memory.IsInCover)
				{
					BotMemoryClass memory2 = BotOwner_0.Memory;
					float? secToBeSpotted = 1f;
					memory2.Spotted(byHit: false, null, secToBeSpotted);
				}
				HoldFor(UnityEngine.Random.Range(5f, 10f));
				Bool_7 = true;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "holdNoLoot");
			}
			if (!BotOwner_0.PatrollingData.LootData.ClusterLootingNow())
			{
				BotOwner_0.PatrollingData.LootData.BeginLootCluster();
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToLootPointNode, "Looting");
		}
		HoldFor(UnityEngine.Random.Range(5f, 10f));
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "hold");
	}

	public void method_14()
	{
		if (BotOwner_0.PatrollingData.LootData.AllGroupWantLootCluster())
		{
			return;
		}
		BotOwner leader = GetLeader(BotOwner_0);
		if (leader == null)
		{
			return;
		}
		if (leader.PatrollingData.LootData.TargetLootCluster == null || leader.PatrollingData.LootData.TargetLootCluster.Looted)
		{
			List<AILootPointsCluster> list = method_17(BotOwner_0.Settings.FileSettings.Patrol.LOOT_PATROL_POTENTIAL_CLUSTERS_AMOUNT);
			if (list.Count == 0)
			{
				return;
			}
			AILootPointsCluster randomItem = GClass855.GetRandomItem(list);
			leader.PatrollingData.LootData.SetTargetLootCluster(randomItem);
		}
		for (int i = 1; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			leader = GetLeader(botOwner);
			botOwner.PatrollingData.LootData.SetTargetLootCluster(leader.PatrollingData.LootData.TargetLootCluster);
		}
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.PatrollingData.LootData.TargetLootCluster == null)
		{
			return base.FindPoint(data, p, checkCurrent);
		}
		if (!Bool_6 && !Bool_7)
		{
			data.CenterPos = BotOwner_0.PatrollingData.LootData.TargetLootCluster.CenterPosition;
			CustomNavigationPoint_0 = base.FindPoint(data, p, checkCurrent);
			return CustomNavigationPoint_0;
		}
		CustomNavigationPoint_0 = null;
		data.CenterPos = BotOwner_0.Position;
		return base.FindPoint(data, p, checkCurrent);
	}

	public CustomNavigationPoint FindPointFromFullMapPatrolLayer(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_1 != null && !CustomNavigationPoint_1.IsSpotted)
		{
			return CustomNavigationPoint_1;
		}
		if (AicorePoint_0 == null)
		{
			AICorePointHolder aICorePointsHolder = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AICorePointsHolder;
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, (GroupPoint point) => !point.IsSpotted && point.CoverType == CoverType.Wall);
			if (closestPoint != null)
			{
				AicorePoint_0 = aICorePointsHolder.GetCorePoint(closestPoint.GroupPoint.CorePointId);
			}
		}
		if (AicorePoint_0 != null)
		{
			if (AicorePoint_0.ConnectionsAtNet.Count > 0)
			{
				AICorePoint aICorePoint = GClass856.RandomElement(AicorePoint_0.ConnectionsAtNet);
				if (aICorePoint != null)
				{
					AicorePoint_0 = aICorePoint;
				}
			}
			float b = 20f;
			float x = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			float z = GClass856.Random(10f, b) * (float)GClass856.RandomSing();
			Vector3 vector = new Vector3(x, 0f, z);
			CustomNavigationPoint closestPoint2 = BotOwner_0.Covers.GetClosestPoint(AicorePoint_0.Position + vector, (GroupPoint point) => !point.IsSpotted && point.CoverType == CoverType.Wall);
			if (closestPoint2 != null)
			{
				if (CustomNavigationPoint_1 == null || CustomNavigationPoint_1.Id != closestPoint2.Id)
				{
					Float_7 = Time.time;
				}
				CustomNavigationPoint_1 = closestPoint2;
				return CustomNavigationPoint_1;
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public bool method_15(AILootPointsCluster cluster)
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_LOOT_BOSS_CLUSTER)
		{
			return !cluster.CheckClusterInBossZone(BotOwner_0.BotsGroup);
		}
		return true;
	}

	public AILootPointsCluster method_16()
	{
		List<AILootPointsCluster> lootPointClusters = Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.LootPointClusters;
		int num = -1;
		double num2 = 0.0;
		for (int i = 0; i < lootPointClusters.Count; i++)
		{
			AILootPointsCluster aILootPointsCluster = lootPointClusters[i];
			if (!method_18(aILootPointsCluster) && method_15(aILootPointsCluster))
			{
				float num3 = aILootPointsCluster.Evaluate(BotOwner_0);
				if ((double)num3 > num2)
				{
					num2 = num3;
					num = i;
				}
			}
		}
		if (num < lootPointClusters.Count && num >= 0)
		{
			return lootPointClusters[num];
		}
		return null;
	}

	public List<AILootPointsCluster> method_17(int amount)
	{
		List<AILootPointsCluster> list = new List<AILootPointsCluster>();
		List<AILootPointsCluster> lootPointClusters = Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.LootPointClusters;
		if (amount == 1)
		{
			list.Add(method_16());
			return list;
		}
		Dictionary<float, AILootPointsCluster> dictionary = new Dictionary<float, AILootPointsCluster>();
		for (int i = 0; i < lootPointClusters.Count; i++)
		{
			AILootPointsCluster aILootPointsCluster = lootPointClusters[i];
			if (!method_18(aILootPointsCluster) && method_15(aILootPointsCluster))
			{
				float key = aILootPointsCluster.Evaluate(BotOwner_0);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, aILootPointsCluster);
				}
			}
		}
		List<float> list2 = dictionary.Keys.ToList();
		list2.Sort();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			list.Add(dictionary[list2[num]]);
			if (list.Count >= amount)
			{
				break;
			}
		}
		if (list.Count < amount)
		{
			list.Clear();
			list.Add(method_16());
			return list;
		}
		return list;
	}

	public bool method_18(AILootPointsCluster cluster)
	{
		if (cluster.ConnectionGroup == BotOwner_0.StartCorePoint.ConnectionGroupId && !cluster.Looted && (cluster.ReservedForGroup == null || cluster.ReservedForGroup == BotOwner_0.BotsGroup))
		{
			return false;
		}
		return true;
	}

	public bool method_19()
	{
		if (BotOwner_0.KeepZoneOnSpawn)
		{
			return GClass1891.PastTime < BotOwner_0.Settings.FileSettings.Mind.KEEP_ZONE_ON_SPAWN_TIME_SEC;
		}
		return false;
	}

	public bool method_20()
	{
		if (Time.time < Float_4)
		{
			return Bool_4;
		}
		List<AILootPointsCluster> lootPointClusters = Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.LootPointClusters;
		if (lootPointClusters == null)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < lootPointClusters.Count)
			{
				AILootPointsCluster aILootPointsCluster = lootPointClusters[num];
				if (!aILootPointsCluster.Looted && (aILootPointsCluster.ReservedForGroup == null || aILootPointsCluster.ReservedForGroup == BotOwner_0.BotsGroup) && !(aILootPointsCluster.EvaluatedCostRuntime < 0.1f) && (BotOwner_0.Settings.FileSettings.Mind.CAN_LOOT_BOSS_CLUSTER || !aILootPointsCluster.CheckClusterInBossZone(BotOwner_0.BotsGroup)))
				{
					break;
				}
				num++;
				continue;
			}
			Bool_4 = false;
			Float_4 = Time.time + 10f;
			return false;
		}
		Bool_4 = true;
		Float_4 = Time.time + 10f;
		return true;
	}

	public bool method_21(float sqrRadiusMultiplier = 1f)
	{
		if (AILootPointsCluster_0 == null)
		{
			return false;
		}
		return GClass856.SqrDistance(BotOwner_0.Position, AILootPointsCluster_0.CenterPosition) < AILootPointsCluster_0.SquareRadius * sqrRadiusMultiplier;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (Bool_9)
		{
			float num = 0f;
			if (CustomNavigationPoint_0 != null)
			{
				num = (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude;
			}
			if (num > Float_5)
			{
				Float_6 = Time.time;
			}
			else if (Time.time > Float_6 + 120f)
			{
				if (BotOwner_0.Mover.CurrentState == EBotMoverState.NearDoor)
				{
					Float_6 = Time.time;
					return AICoreActionEndStruct_1;
				}
				Bool_9 = false;
				return new AICoreActionEndStruct("TryTactical");
			}
		}
		if (method_21(0.25f))
		{
			Bool_9 = false;
			return new AICoreActionEndStruct("InCluster");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToLootPointNode()
	{
		if (method_13())
		{
			if (!BotOwner_0.PatrollingData.LootData.HaveActions())
			{
				return new AICoreActionEndStruct("noLootD");
			}
			return AICoreActionEndStruct_1;
		}
		if (BotOwner_0.PatrollingData.LootData.AllGroupWantLootCluster() && !Bool_5)
		{
			return new AICoreActionEndStruct("wantLoot");
		}
		if (!BotOwner_0.PatrollingData.LootData.HaveActions(AILootPointsCluster_0.SquareRadius * 2f, 4))
		{
			return new AICoreActionEndStruct("noLootD");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		if (CustomNavigationPoint_0 != null)
		{
			float sqrMagnitude = (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude;
			if (sqrMagnitude < Float_5)
			{
				Float_5 = sqrMagnitude;
				Float_6 = Time.time;
			}
			if (BotOwner_0.Memory.IsInCover && sqrMagnitude < 5f)
			{
				Float_5 = float.MaxValue;
				Bool_9 = true;
				return new AICoreActionEndStruct("TargetReached");
			}
		}
		if (Time.time > Float_6 + 40f)
		{
			if (BotOwner_0.Mover.CurrentState == EBotMoverState.NearDoor)
			{
				Float_6 = Time.time;
				return AICoreActionEndStruct_1;
			}
			Bool_9 = true;
			return new AICoreActionEndStruct("Stuck");
		}
		if (method_21(1.25f))
		{
			return new AICoreActionEndStruct("InCluster");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_13())
		{
			return new AICoreActionEndStruct("TimeExp");
		}
		if (BotOwner_0.PatrollingData.LootData.AllGroupWantLootCluster() && !Bool_5 && AILootPointsCluster_0 != null)
		{
			return new AICoreActionEndStruct("WantLoot");
		}
		if (Bool_6 && BotOwner_0.PatrollingData.LootData.TargetLootCluster != null && BotOwner_0.PatrollingData.LootData.TargetLootCluster.Looted)
		{
			return new AICoreActionEndStruct("Looted");
		}
		if ((method_7() || !Bool_2) && AILootPointsCluster_0 != null && BotOwner_0.PatrollingData.LootData.HaveActions(AILootPointsCluster_0.SquareRadius * 2f, 4))
		{
			return new AICoreActionEndStruct("haveLoot");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		if (method_19())
		{
			return false;
		}
		return method_20();
	}

	public override string Name()
	{
		return "LootPatrol";
	}
}
