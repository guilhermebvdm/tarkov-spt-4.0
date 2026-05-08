using System;
using EFT;
using UnityEngine;

public class Class99 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public bool Bool_4;

	public Class99(BotOwner bot, int priority, bool withSearch)
		: base(bot, priority)
	{
		Bool_4 = withSearch;
	}

	public override string Name()
	{
		return "AdvAssaultTarget";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (method_13())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "search1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "HoldOrCover");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.GoalTarget.HaveMainTarget();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("EndHeal");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		if (method_13())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("!Searc");
	}

	public override AICoreActionEndStruct EndAxeTarget()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootToSmoke()
	{
		if (!BotOwner_0.SmokeGrenade.ShallShoot())
		{
			return new AICoreActionEndStruct("EndShootSmo");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (method_13())
			{
				return new AICoreActionEndStruct("Search");
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("!Cover");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("FirstAid");
	}

	public bool method_13()
	{
		if (!Bool_4)
		{
			return false;
		}
		if (!BotOwner_0.Memory.AttackImmediately && BotOwner_0.Memory.LastEnemy != null && (Time.time - BotOwner_0.Memory.LastEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Cover.TIME_TO_MOVE_TO_COVER || BotOwner_0.BotsGroup.IsLastPositionOld))
		{
			return false;
		}
		return true;
	}
}
