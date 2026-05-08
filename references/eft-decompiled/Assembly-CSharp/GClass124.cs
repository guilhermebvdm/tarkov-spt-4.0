using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass124(BotOwner bot, int priority, GClass48 avoidLayerToCheck) : GClass122(bot, priority, avoidLayerToCheck)
{
	[CompilerGenerated]
	public class Class224
	{
		public EnemyInfo enemy;

		public bool method_0(GroupPoint arg)
		{
			if (enemy == null)
			{
				return true;
			}
			if ((arg.Position - enemy.CurrPosition).sqrMagnitude > 400f)
			{
				return true;
			}
			return false;
		}
	}

	[NonSerialized]
	public float Float_7 = -9999f;

	[NonSerialized]
	public float Float_8;

	public const float SDIST_TO_STOP_ON_HEAR = 2025f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public HashSet<CustomNavigationPoint> HashSet_0 = new HashSet<CustomNavigationPoint>();

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo enemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		AIMinePoint aIMinePoint = BotOwner_0.MinesData.FindClosestsUnplanted(BotOwner_0.Position);
		if (!BotOwner_0.MinesData.GetFirstFragGrenade(out var _))
		{
			Bool_4 = true;
			HoldFor(1f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "n887");
		}
		if (aIMinePoint == null)
		{
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "n865");
		}
		float num = BotOwner_0.SDistTo(aIMinePoint.Position);
		BotOwner_0.MinesData.SetActive(aIMinePoint);
		if (num < 2.25f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.plantMine, "fdgh65");
		}
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(aIMinePoint.Position, delegate(GroupPoint arg)
		{
			if (enemy == null)
			{
				return true;
			}
			return (arg.Position - enemy.CurrPosition).sqrMagnitude > 400f;
		}, printErrorLogsIfFail: false, 30);
		if (HashSet_0.Count > 15)
		{
			HashSet_0.Clear();
		}
		if (closestPoint != null && !HashSet_0.Contains(closestPoint))
		{
			float sqrMagnitude = (closestPoint.Position - aIMinePoint.Position).sqrMagnitude;
			float sqrMagnitude2 = (BotOwner_0.Position - aIMinePoint.Position).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				CustomNavigationPoint_0 = closestPoint;
				HashSet_0.Add(CustomNavigationPoint_0);
				BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPointTactical, "tcvr5");
			}
		}
		CustomNavigationPoint_0 = null;
		BotOwner_0.GoToSomePointData.SetPoint(aIMinePoint.Position);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPointTactical, "uity5");
	}

	public override AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint == null)
		{
			return new AICoreActionEndStruct("noCov");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && (BotOwner_0.Position - BotOwner_0.Memory.CurCustomCoverPoint.Position).sqrMagnitude < 4f)
		{
			return new AICoreActionEndStruct("toclose");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndMoveStealthy()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		if (method_14())
		{
			return new AICoreActionEndStruct("wnr4");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (Time.time - Float_7 > 20f)
		{
			return new AICoreActionEndStruct("n9v3");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Bool_2)
		{
			if (Float_2 < Time.time)
			{
				Bool_2 = false;
				return new AICoreActionEndStruct("finisHld");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("notHld");
	}

	public override string Name()
	{
		return "PartMineAll";
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.MinesData.HaveUnplanted)
		{
			return false;
		}
		if (Bool_4)
		{
			return false;
		}
		if (Float_8 > Time.time)
		{
			return false;
		}
		if (BotOwner_0.Memory.IsUnderFire)
		{
			Float_8 = Time.time + 30f;
			return false;
		}
		if (!BotOwner_0.Memory.HaveGoal)
		{
			return true;
		}
		if (!method_15())
		{
			return false;
		}
		GoalTargetClass goalTarget = BotOwner_0.Memory.GoalTarget;
		if (!goalTarget.Position.HasValue)
		{
			return true;
		}
		Vector3 vector = goalTarget.Position.Value - BotOwner_0.Position;
		float num = 2025f;
		if (vector.sqrMagnitude < num)
		{
			Float_8 = Time.time + 30f;
			return false;
		}
		return true;
	}
}
