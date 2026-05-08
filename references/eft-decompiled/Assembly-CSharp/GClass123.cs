using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass123(BotOwner bot, int priority, GClass48 avoidLayerToCheck) : GClass122(bot, priority, avoidLayerToCheck)
{
	[CompilerGenerated]
	public class Class223
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
	public float Float_7;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public float Float_8 = 4f;

	[NonSerialized]
	public const bool Bool_7 = true;

	public const float WAIT_DANGER_PLACE = 30f;

	public const float WAIT_SIMPLE_PLACE = 10f;

	public const float SDIST_TO_STOP_ON_HEAR = 100f;

	public const float SDIST_TO_STOP_ON_HEAR_DANGER = 900f;

	public const float SDIST_ENEMY_GO_FAR = 6400f;

	[NonSerialized]
	public float Float_9 = -9999f;

	[NonSerialized]
	public float Float_10;

	public const float PERIOD_STOP_ON_HEAR = 30f;

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo enemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		if (!BotOwner_0.MinesData.GetFirstFragGrenade(out var _))
		{
			Bool_4 = true;
			HoldFor(1f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "n887");
		}
		AIMinePoint closestsFromCache = BotOwner_0.MinesData.GetClosestsFromCache(BotOwner_0.Position);
		if (closestsFromCache == null)
		{
			BotOwner_0.MinesData.ClearCache();
			if (BotOwner_0.Memory.IsInCover)
			{
				HoldFor(5f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "f4");
			}
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, delegate(GroupPoint arg)
			{
				if (enemy == null)
				{
					return true;
				}
				return (arg.Position - enemy.CurrPosition).sqrMagnitude > 400f;
			}, printErrorLogsIfFail: false, 30);
			if (closestPoint != null)
			{
				BotOwner_0.Memory.SetCoverPoints(closestPoint);
				GClass369.DebugDrawArc(BotOwner_0.Position, closestPoint.Position, 5f, 5f, Color.yellow);
				Bool_6 = false;
				return method_13("posiblePoint", closestPoint);
			}
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "u84");
		}
		float num = BotOwner_0.SDistTo(closestsFromCache.Position);
		BotOwner_0.MinesData.SetActive(closestsFromCache);
		if (num < 2.25f)
		{
			Bool_5 = false;
			Float_7 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.plantMine, "putMine");
		}
		CustomNavigationPoint closestPoint2 = BotOwner_0.Covers.GetClosestPoint(closestsFromCache.Position, delegate(GroupPoint arg)
		{
			if (enemy == null)
			{
				return true;
			}
			return (arg.Position - enemy.CurrPosition).sqrMagnitude > 400f;
		});
		Float_9 = Time.time;
		float num2 = BotOwner_0.SDistTo(closestPoint2.Position);
		float num3 = BotOwner_0.Memory.GoalEnemy.Distance * BotOwner_0.Memory.GoalEnemy.Distance;
		if (Bool_5)
		{
			BotOwner_0.GoToSomePointData.SetPoint(closestsFromCache.Position);
			Bool_5 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPointTactical, "lstMStlt_" + num.ToString("0") + ":" + num2.ToString("0"));
		}
		if (num3 < num2 && !Bool_6)
		{
			Bool_6 = true;
			BotOwner_0.Memory.SetCoverPoints(closestPoint2);
			return method_13("arEnmy_" + num3.ToString("0") + ":" + num2.ToString("0"), closestPoint2);
		}
		if (num < num2)
		{
			BotOwner_0.GoToSomePointData.SetPoint(closestsFromCache.Position);
			Bool_5 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPointTactical, "toMinePoint_" + num.ToString("0") + ":" + num2.ToString("0"));
		}
		BotOwner_0.Memory.Spotted(byHit: false);
		BotOwner_0.Memory.SetCoverPoints(closestPoint2);
		GClass369.DebugDrawArc(BotOwner_0.Position, closestPoint2.Position, 5f, 5f, Color.yellow);
		return method_13("onlon", closestPoint2);
	}

	public override AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndMoveStealthy()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		if (method_14())
		{
			return new AICoreActionEndStruct("wntR1");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (Time.time - Float_9 > 20f)
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
		return "PartisanMine";
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
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (Bool_4)
		{
			return false;
		}
		if (Float_10 > Time.time)
		{
			return false;
		}
		if (!method_15())
		{
			return false;
		}
		if (BotOwner_0.Memory.IsUnderFire)
		{
			Float_10 = Time.time + 30f;
			return false;
		}
		if ((BotOwner_0.MinesData.LastCacheCenter - BotOwner_0.Memory.GoalEnemy.CurrPosition).sqrMagnitude > 6400f)
		{
			BotOwner_0.MinesData.ClearCache();
		}
		if (!BotOwner_0.MinesData.HaveAtCache())
		{
			return false;
		}
		if (!BotOwner_0.Memory.HaveGoal)
		{
			return true;
		}
		GoalTargetClass goalTarget = BotOwner_0.Memory.GoalTarget;
		if (!goalTarget.Position.HasValue)
		{
			return true;
		}
		Vector3 vector = goalTarget.Position.Value - BotOwner_0.Position;
		float num;
		float num2;
		if (goalTarget.IsDanger)
		{
			num = 900f;
			num2 = 30f;
		}
		else
		{
			num = 100f;
			num2 = 10f;
		}
		if ((double)Time.time - goalTarget.CreatedTime > (double)num2)
		{
			return true;
		}
		if (vector.sqrMagnitude < num)
		{
			Float_10 = Time.time + 30f;
			return false;
		}
		return true;
	}
}
