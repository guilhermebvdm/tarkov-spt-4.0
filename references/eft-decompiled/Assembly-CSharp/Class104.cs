using System;
using EFT;
using UnityEngine;

public class Class104 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 40000f;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_4;

	public Class104(BotOwner bot, int priority, bool withSearch)
		: base(bot, priority)
	{
		Bool_4 = withSearch;
		BotOwner_0.Memory.GoalTarget.OnGoalTargetChange += method_13;
	}

	public void method_13(PlaceForCheck p1, PlaceForCheck p2)
	{
		Float_4 = float.MaxValue;
	}

	public override string Name()
	{
		return "PmcPveTarget";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do && !BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (method_14())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "search1");
			}
			float num = UnityEngine.Random.Range(30f, 150f);
			Float_4 = Time.time + num - 0.1f;
			HoldFor(num);
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
		if (BotOwner_0.Memory.GoalTarget.HaveMainTarget() && BotOwner_0.Memory.GoalTarget.IsDanger && BotOwner_0.Memory.GoalTarget.Position.HasValue && GClass856.SqrDistance(BotOwner_0.Memory.GoalTarget.Position.Value, BotOwner_0.Position) < 40000f && !(Time.time >= Float_4))
		{
			return true;
		}
		return Time.time > Float_4 + 160f;
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
		if (method_14())
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
			if (method_14())
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

	public override AICoreActionEndStruct EndGoToLootPointNode()
	{
		return AICoreActionEndStruct;
	}

	public bool method_14()
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
