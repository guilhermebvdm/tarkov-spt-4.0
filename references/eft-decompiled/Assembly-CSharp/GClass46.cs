using System;
using EFT;
using UnityEngine;

public class GClass46 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 3f;

	public GClass46(BotOwner bot, int priority)
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
		if (prev == null && next != null && BotOwner_0.Memory.GoalTarget.HaveMainTarget() && !BotOwner_0.Memory.HaveEnemy)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.Spotted();
		}
	}

	public override string Name()
	{
		return "Simple Target";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (method_12(20f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "Hit");
			}
			if (method_15())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "Hit");
			}
			if (BotOwner_0.SmokeGrenade.ShallShoot())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "shootSmoke");
			}
			if (method_16())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "search1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "HoldOrCover");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
		}
		if (method_12(20f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
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

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.GoalTarget.HaveMainTarget())
		{
			if (BotOwner_0.EnemiesController.HavePursuitableEnemy)
			{
				return BotOwner_0.PriorityAxeTarget.IsInPossibleRadius();
			}
			return false;
		}
		return true;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("EndHeal");
		}
		if (BaseLogicLayerSimpleAbstractClass.CheckMedsToStop(BotOwner_0))
		{
			BotOwner_0.Medecine.FirstAid.CancelCurrent();
			return new AICoreActionEndStruct("CancelHeal");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		if (method_12(2f))
		{
			return new AICoreActionEndStruct("Hitted");
		}
		if (method_16())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("!Searc");
	}

	public override AICoreActionEndStruct EndShootToSmoke()
	{
		if (!BotOwner_0.SmokeGrenade.ShallShoot())
		{
			return new AICoreActionEndStruct("EndShootSmo");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return new AICoreActionEndStruct("noGo");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_12(10f) && !BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("hit&noCover");
		}
		if (method_15())
		{
			return new AICoreActionEndStruct("PlaceCreatRecently", val: false);
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

	public bool method_16()
	{
		if (BotOwner_0.Memory.GoalTarget.HaveZeroTarget() && !BotOwner_0.Memory.GoalTarget.HavePlaceTarget())
		{
			return false;
		}
		if (!BotOwner_0.Settings.FileSettings.Mind.SEARCH_TARGET)
		{
			return false;
		}
		if (BotOwner_0.Memory.LastEnemy != null && Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Cover.TIME_TO_MOVE_TO_COVER)
		{
			return false;
		}
		if (method_12(25f))
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
