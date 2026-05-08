using System;
using EFT;
using UnityEngine;

public class GClass43 : BaseLogicLayerSimpleAbstractClass
{
	public const float PERIOD_NO_VISION = 1f;

	public const float WAIT_PERIOD_SEC = 900f;

	[NonSerialized]
	public const bool Bool_4 = true;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	public virtual Vector3? TargetPosition
	{
		get
		{
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				return BotOwner_0.Memory.GoalEnemy.EnemyLastPositionReal;
			}
			if (BotOwner_0.Memory.LastEnemy != null)
			{
				return BotOwner_0.Memory.LastEnemy.EnemyLastPositionReal;
			}
			return null;
		}
	}

	public virtual int TargetEnvironmentId
	{
		get
		{
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				return BotOwner_0.Memory.GoalEnemy.Person.AIData.EnvironmentId;
			}
			if (BotOwner_0.Memory.LastEnemy != null)
			{
				return BotOwner_0.Memory.LastEnemy.Person.AIData.EnvironmentId;
			}
			return 0;
		}
	}

	public GClass43(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
			if (aICoreActionResultStruct.HasValue)
			{
				return aICoreActionResultStruct.Value;
			}
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shotNow");
			}
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (method_17())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold1");
			}
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold2");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data = method_14();
		CustomNavigationPoint customNavigationPoint = p(data);
		if (customNavigationPoint != null && customNavigationPoint.EnvironmentId == data.EnvironmentId)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			return customNavigationPoint;
		}
		Vector3 trg = (TargetPosition.HasValue ? TargetPosition.Value : BotOwner_0.Position);
		CustomNavigationPoint customNavigationPoint2 = BotOwner_0.Covers.FindClosestPoint(data.CenterPos, 3f, trg, noRestrictions: false, method_15);
		if (customNavigationPoint2 != null && customNavigationPoint2.EnvironmentId != data.EnvironmentId)
		{
			Debug.LogError($"WRONG closets.EnvironmentId:{customNavigationPoint2.EnvironmentId}");
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint2);
		return customNavigationPoint2;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.Person.AIData.EnvironmentId > 0 && Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal < 900f)
		{
			return true;
		}
		if (BotOwner_0.Memory.LastEnemy != null && BotOwner_0.Memory.LastEnemy.Person.AIData.EnvironmentId > 0 && Time.time - BotOwner_0.Memory.LastEnemy.GroupInfo.EnemyLastSeenTimeReal < 900f)
		{
			return true;
		}
		return false;
	}

	public override string Name()
	{
		return "Enemy Building";
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("cantShoot");
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		return AICoreActionEndStruct;
	}

	public override void ManualUpdate()
	{
		method_13();
		BotOwner_0.AssaultBuildingData.PeriodCheckStart();
		base.ManualUpdate();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime > 25f && method_16())
			{
				BotOwner_0.Memory.CheckIsInCover2();
				return new AICoreActionEndStruct("better");
			}
			if (method_7())
			{
				return new AICoreActionEndStruct("EndHol");
			}
			if (!BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id))
			{
				return new AICoreActionEndStruct("notMy");
			}
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("IsInCover");
			}
			if (goalEnemy != null && goalEnemy.IsVisible)
			{
				if (goalEnemy.CanShoot)
				{
					return new AICoreActionEndStruct("CanShoot");
				}
				if (method_6(out var _))
				{
					return new AICoreActionEndStruct("sfc5");
				}
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("null or spotted");
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return new AICoreActionEndStruct("end");
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		return new AICoreActionEndStruct("end");
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		return new AICoreActionEndStruct("end");
	}

	public void method_13()
	{
		if (Float_4 > Time.time)
		{
			return;
		}
		Float_4 = Time.time + 5f;
		Vector3? targetPosition = TargetPosition;
		if (BotOwner_0.BotRequestController.GetRequestsCount<GClass601>() <= 0 && targetPosition.HasValue)
		{
			int targetEnvironmentId = TargetEnvironmentId;
			BotZoneEntrance closest = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.EntranceInfo.GetClosest(targetPosition.Value, targetEnvironmentId);
			if (closest != null)
			{
				BotOwner_0.BotRequestController.TryActivateThrowGrenadeRequest(closest.PointOutSide, null, out var _);
				Float_4 = Time.time + 25f;
			}
		}
	}

	public CoverSearchData method_14()
	{
		Vector3 vector = TargetPosition ?? BotOwner_0.Position;
		int targetEnvironmentId = TargetEnvironmentId;
		if (targetEnvironmentId > 0)
		{
			Vector3 vector2 = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.EntranceInfo.GetClosest(vector, targetEnvironmentId)?.PointOutSide ?? vector;
			return new CoverSearchData(vector2, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 50f, 64f, CoverSearchType.distToToCenter, new ShootPointClass(vector2), null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f))
			{
				MinSDistToCarePos = 16f,
				CarePositions = { vector2 },
				EnvironmentId = 0
			};
		}
		CoverSearchData coverSearchData;
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			Vector3 partToShoot = BotOwner_0.Memory.GoalEnemy.GetPartToShoot();
			coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 50f, 64f, CoverSearchType.distToToCenter, new ShootPointClass(partToShoot), null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f));
		}
		else
		{
			coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 50f, 64f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f));
		}
		coverSearchData.MinSDistToCarePos = 16f;
		coverSearchData.EnvironmentId = 0;
		return coverSearchData;
	}

	public bool method_15(GroupPoint point)
	{
		return point.IdEnvironment == 0;
	}

	public bool method_16()
	{
		if (Float_3 > Time.time)
		{
			return false;
		}
		Float_3 = Time.time + 2f;
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		CoverSearchData data = method_14();
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (coverPointMain != BotOwner_0.Memory.CurCustomCoverPoint)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
			return true;
		}
		return false;
	}

	public bool method_17()
	{
		CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
		if (curCustomCoverPoint != null && curCustomCoverPoint.EnvironmentId <= 0 && !curCustomCoverPoint.IsSpotted && curCustomCoverPoint.IsFreeById(BotOwner_0.Id))
		{
			return true;
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(null);
		method_16();
		return false;
	}
}
