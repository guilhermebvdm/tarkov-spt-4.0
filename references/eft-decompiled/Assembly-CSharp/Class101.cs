using System;
using EFT;
using UnityEngine;

public class Class101 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 3f;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public GClass674 Gclass674_0;

	public Class101(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.Memory.GoalTarget.OnGoalTargetChange += method_14;
		BotOwner_0.Memory.GoalTarget.OnZeroGoalSetted += method_13;
	}

	public void method_13()
	{
		if (BotOwner_0.Memory.GoalTarget.HaveMainTarget() && !BotOwner_0.Memory.HaveEnemy)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.Spotted();
		}
	}

	public void method_14(PlaceForCheck prev, PlaceForCheck next)
	{
		Float_4 = Time.time;
		if (prev == null && next != null && BotOwner_0.Memory.GoalTarget.HaveMainTarget() && !BotOwner_0.Memory.HaveEnemy)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.Spotted();
		}
		if (BotOwner_0.Memory.GoalTarget.Position.HasValue)
		{
			BotOwner_0.Mover.GoToPoint(BotOwner_0.Memory.GoalTarget.Position.Value, slowAtTheEnd: false, 1f);
		}
	}

	public override string Name()
	{
		return "InfectedTarget";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (method_16() && BotOwner_0.Memory.GoalTarget.Position.HasValue)
		{
			BotOwner_0.GoToSomePointData.SetPoint(BotOwner_0.Memory.GoalTarget.Position.Value);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "search1");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "hoc");
	}

	public bool method_15()
	{
		if (BotOwner_0.Memory.GoalTarget.HaveZeroTarget())
		{
			return true;
		}
		if (!BotOwner_0.Memory.GoalTarget.HavePlaceTarget())
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalTarget.Type == PlaceForCheckType.danger)
		{
			return (double)Time.time - BotOwner_0.Memory.GoalTarget.CreatedTime < 15.0;
		}
		return false;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.GoalTarget.HaveMainTarget() && Gclass674_0 != null && !Gclass674_0.Disposed)
		{
			BotOwner_0.BotsGroup.AddPointToSearch(Gclass674_0.TargetLastSeenPosition, 1000f, BotOwner_0);
		}
		if (BotOwner_0.Memory.GoalTarget.HaveMainTarget())
		{
			float targetPointSearchRadiusLimit = BotOwner_0.BotsController.EventsController.BotHalloweenWithZombies.TargetPointSearchRadiusLimit;
			if (BotOwner_0.Memory.GoalTarget.Position.HasValue && GClass856.SqrDistance(BotOwner_0.Memory.GoalTarget.Position.Value, BotOwner_0.Position) >= targetPointSearchRadiusLimit * targetPointSearchRadiusLimit)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.Memory.GoalTarget != null && BotOwner_0.Memory.GoalTarget.Position.HasValue && (BotOwner_0.Memory.GoalTarget.Position.Value - BotOwner_0.Position).sqrMagnitude < 16f)
		{
			BotOwner_0.BotsGroup.PointChecked(BotOwner_0.Memory.GoalTarget.GoalTarget);
		}
		if (method_16())
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndGoToPoint();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_12(10f) && !BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("hit&noCover");
		}
		if (method_15())
		{
			return new AICoreActionEndStruct("PlaceCreatRecently");
		}
		if (Time.time < Float_4 + 1f)
		{
			return new AICoreActionEndStruct("GoalTargetChange");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("CauseTime");
		}
		if (BotOwner_0.EnemiesController.HavePursuitableEnemy && !Bool_2)
		{
			return new AICoreActionEndStruct("HavePursuit");
		}
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (method_16())
			{
				return new AICoreActionEndStruct("Search");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("FirstAid");
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public void SetActivePursuit(GClass674 activePursuit)
	{
		if (Gclass674_0 != null && Gclass674_0.Disposed)
		{
			Gclass674_0 = null;
		}
		if (Gclass674_0 == null || Gclass674_0.Disposed || !Gclass674_0.TargetAllowToSpawnCrowd || activePursuit == null || activePursuit.Disposed || activePursuit.TargetAllowToSpawnCrowd)
		{
			Gclass674_0 = activePursuit;
		}
	}

	public bool method_16()
	{
		if (BotOwner_0.Memory.GoalTarget.HaveZeroTarget() && !BotOwner_0.Memory.GoalTarget.HavePlaceTarget() && !BotOwner_0.Memory.GoalTarget.Position.HasValue)
		{
			return false;
		}
		return true;
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.GoalTarget.OnZeroGoalSetted -= method_13;
		BotOwner_0.Memory.GoalTarget.OnGoalTargetChange -= method_14;
		base.Dispose();
	}
}
