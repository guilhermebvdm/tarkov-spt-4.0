using System;
using EFT;
using UnityEngine;

public class GClass135 : BaseLogicLayerSimpleAbstractClass
{
	public const float SDIST_TREE_CLOSE = 900f;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3 = 400f;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public CoverLevel CoverLevel_0;

	public GClass135(BotOwner bot, int priority, bool tryFinGreenFirst, CoverLevel minGreenPointCoverLevel = CoverLevel.Lay, bool usePeriodForceReched = false)
		: base(bot, priority)
	{
		CoverLevel_0 = minGreenPointCoverLevel;
		Bool_5 = usePeriodForceReched;
		Bool_6 = tryFinGreenFirst;
	}

	public void SetCorePosition(Vector3 corePosition)
	{
		if (Vector3_0.sqrMagnitude < 0.1f)
		{
			Vector3_0 = (BotOwner_0.BotFollower.HaveBoss ? BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover : BotOwner_0.Position);
		}
		GClass369.DebugDrawArc(BotOwner_0.Position, corePosition, 30f, 40f, Color.yellow);
		Bool_4 = (corePosition - Vector3_0).sqrMagnitude > 1f;
		Vector3_0 = corePosition;
		if (BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			float sqrMagnitude = (Vector3_0 - BotOwner_0.Memory.CurCustomCoverPoint.Position).sqrMagnitude;
			Bool_4 = sqrMagnitude > 100f;
			BotOwner_0.Memory.Spotted(byHit: false);
		}
	}

	public override void ManualUpdate()
	{
		BotOwner_0.PatrollingData.TryToTalk(bigDelay: true);
		base.ManualUpdate();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 == null)
		{
			CustomNavigationPoint_0 = GetPoint();
		}
		return CustomNavigationPoint_0;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			method_16();
			if (BotOwner_0.Memory.CurCustomCoverPoint != CustomNavigationPoint_0 && CustomNavigationPoint_0 != null)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
				if (BotOwner_0.CanSprintPlayer)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "!=");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "CntSprint");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "no better");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Vector3 vector = Vector3.up * 0.3f;
		Gizmos.DrawLine(Vector3_0 + vector, BotOwner_0.Position + vector);
		if (CustomNavigationPoint_0 != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(CustomNavigationPoint_0.Position + vector, BotOwner_0.Position + vector);
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(BotOwner_0.Memory.CurCustomCoverPoint.Position + vector, BotOwner_0.Position + vector);
		}
	}

	public override string Name()
	{
		return "StayAtPos";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("nedHeal");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFollowerPatrolItem()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		method_16();
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("nedHeal");
		}
		if (IsPointGood(BotOwner_0.Memory.CurCustomCoverPoint))
		{
			return AICoreActionEndStruct_1;
		}
		if (CustomNavigationPoint_0 == BotOwner_0.Memory.CurCustomCoverPoint)
		{
			return AICoreActionEndStruct_1;
		}
		if (PointBetter(CustomNavigationPoint_0, BotOwner_0.Memory.CurCustomCoverPoint) == CustomNavigationPoint_0)
		{
			return new AICoreActionEndStruct("bettePoint");
		}
		return AICoreActionEndStruct_1;
	}

	public virtual bool IsPointGood(CustomNavigationPoint cachedPoint)
	{
		if (cachedPoint == null)
		{
			return false;
		}
		return (cachedPoint.Position - Vector3_0).sqrMagnitude < Float_3;
	}

	public virtual CustomNavigationPoint PointBetter(CustomNavigationPoint b, CustomNavigationPoint a)
	{
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}
		float sqrMagnitude = (a.Position - Vector3_0).sqrMagnitude;
		float sqrMagnitude2 = (b.Position - Vector3_0).sqrMagnitude;
		if (sqrMagnitude < sqrMagnitude2)
		{
			return a;
		}
		return b;
	}

	public virtual CustomNavigationPoint GetPoint()
	{
		if (!Bool_4 && CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		CustomNavigationPoint customNavigationPoint = method_13();
		BotOwner_0.Memory.SetCoverPoints(customNavigationPoint);
		return customNavigationPoint;
	}

	public CustomNavigationPoint method_13()
	{
		if (Vector3_0.sqrMagnitude <= 0.1f)
		{
			try
			{
				Vector3_0 = (BotOwner_0.BotFollower.HaveBoss ? BotOwner_0.BotFollower.BossToFollow.PositionOrTargetCover : BotOwner_0.Position);
			}
			catch (Exception)
			{
				Vector3_0 = BotOwner_0.Position;
			}
		}
		Vector3? vector = BotOwner_0.Covers.ClosestFriendCoverPoint(Vector3_0);
		float minDistToFriend = BotOwner_0.Settings.FileSettings.Cover.CHECK_CLOSEST_FRIEND_DIST * BotOwner_0.Settings.FileSettings.Cover.CHECK_CLOSEST_FRIEND_DIST;
		CoverPointMaster.GStruct10? friendClose = null;
		if (vector.HasValue)
		{
			friendClose = new CoverPointMaster.GStruct10
			{
				MinDistToFriend = minDistToFriend,
				CoverPos = vector.Value
			};
		}
		if (Bool_6)
		{
			if (method_15(friendClose, out var point))
			{
				return point;
			}
			if (method_14(out var cachedPoint))
			{
				return cachedPoint;
			}
		}
		else
		{
			if (method_14(out var cachedPoint2))
			{
				return cachedPoint2;
			}
			if (method_15(friendClose, out var point2))
			{
				return point2;
			}
		}
		return BotOwner_0.BotsGroup.CoverPointMaster.GetFreeClosePoint(Vector3_0, BotOwner_0.Covers, -1f);
	}

	public bool method_14(out CustomNavigationPoint cachedPoint)
	{
		CustomNavigationPoint customNavigationPoint = BotOwner_0.Covers.FindHidePoint(Vector3_0, 50f);
		if (customNavigationPoint != null)
		{
			cachedPoint = customNavigationPoint;
			return true;
		}
		CoverSearchData coverSearchData = new CoverSearchData(Vector3_0, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 30f, 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f), PointsArrayType.byShootType, useSelfFindPoint: false);
		coverSearchData.ArrayType = PointsArrayType.both;
		coverSearchData.UseSelfFindPoint = false;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		if (CustomNavigationPoint_0 == null && coverPointMain != null)
		{
			CustomNavigationPoint_0 = coverPointMain;
			cachedPoint = CustomNavigationPoint_0;
			return true;
		}
		CustomNavigationPoint customNavigationPoint2 = PointBetter(CustomNavigationPoint_0, coverPointMain);
		if (customNavigationPoint2 != null)
		{
			cachedPoint = customNavigationPoint2;
			return true;
		}
		CustomNavigationPoint freeClosePoint = BotOwner_0.BotsGroup.CoverPointMaster.GetFreeClosePoint(Vector3_0, BotOwner_0.Covers, -1f);
		cachedPoint = freeClosePoint;
		return cachedPoint != null;
	}

	public bool method_15(CoverPointMaster.GStruct10? friendClose, out CustomNavigationPoint point)
	{
		if (Bool_6)
		{
			float sDist;
			CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(Vector3_0, BotOwner_0, friendClose, null, CoverLevel_0, out sDist);
			if (customNavigationPoint != null && sDist < 900f)
			{
				point = customNavigationPoint;
				return true;
			}
		}
		point = null;
		return false;
	}

	public void method_16()
	{
		if (Bool_5 && Float_5 < Time.time)
		{
			Float_5 = Time.time + 10f;
			Bool_4 = true;
		}
		if (Float_4 < Time.time)
		{
			Float_4 = Time.time + 1f;
			CustomNavigationPoint_0 = GetPoint();
		}
	}
}
