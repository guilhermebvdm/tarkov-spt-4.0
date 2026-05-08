using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass134(BotOwner bot, int priority) : GClass133(bot, priority)
{
	[Serializable]
	[CompilerGenerated]
	public class Class216
	{
		public static readonly Class216 class216_0 = new Class216();

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
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public AICorePoint AicorePoint_0;

	[NonSerialized]
	public float Float_4 = 25f;

	[NonSerialized]
	public float Float_5 = 7f;

	[NonSerialized]
	public float Float_6 = 15f;

	[NonSerialized]
	public float Float_7 = 120f;

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (method_13(out var aiCoreActionResult))
		{
			return aiCoreActionResult;
		}
		if (BotOwner_0.Memory.IsInCover && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			HoldFor(GClass856.Random(20f, 35f));
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hld");
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPointTactical, "gld");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted)
		{
			return CustomNavigationPoint_0;
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
				if (CustomNavigationPoint_0 == null || CustomNavigationPoint_0.Id != closestPoint2.Id)
				{
					Float_7 = Time.time;
				}
				CustomNavigationPoint_0 = closestPoint2;
				return CustomNavigationPoint_0;
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return "Full map patrol";
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_16(out var reason))
		{
			return new AICoreActionEndStruct(reason);
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("VisibleCanS");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (Time.time - BotOwner_0.Memory.ComeToCoverTime > Float_6)
		{
			Float_6 = GClass856.Random(Float_5, Float_4);
			if (CustomNavigationPoint_0 != null && Time.time - Float_7 > 120f)
			{
				CustomNavigationPoint_0.Spotted(1f);
				CustomNavigationPoint_0 = null;
			}
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("longCover");
		}
		return AICoreActionEndStruct_1;
	}
}
