using EFT;
using UnityEngine;

public class Class105 : BaseLogicLayerSimpleAbstractClass
{
	public Class105(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
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
		if (BotOwner_0.EnemiesController.HavePursuitableEnemy && BotOwner_0.PriorityAxeTarget.IsInPossibleRadius() && BotOwner_0.PriorityAxeTarget.HavePointToGo)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.axeTarget, "axe");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "beRdy");
		}
		HoldFor(3f);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitaxeman");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (goalEnemy.IsVisible || goalEnemy.CanShoot))
		{
			Bool_2 = false;
			return new AICoreActionEndStruct("Visible");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("CauseTime");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!InCover");
		}
		if (!Bool_2 && Time.time - BotOwner_0.Memory.ComeToCoverTime > 10f)
		{
			return new AICoreActionEndStruct("time2leave");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		BotOwner_0.PriorityAxeTarget.FindTarget();
		if (BotOwner_0.EnemiesController.HavePursuitableEnemy)
		{
			return BotOwner_0.PriorityAxeTarget.IsInPossibleRadius();
		}
		return false;
	}

	public override string Name()
	{
		return "Pursuit";
	}

	public override AICoreActionEndStruct EndAxeTarget()
	{
		if (!BotOwner_0.EnemiesController.HavePursuitableEnemy)
		{
			return new AICoreActionEndStruct("HavePursuit");
		}
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("FirstAid");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}
}
