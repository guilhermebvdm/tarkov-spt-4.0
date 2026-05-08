using EFT;
using UnityEngine;

public class GClass60 : BaseLogicLayerSimpleAbstractClass
{
	public GClass60(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "basiclogic");
	}

	public virtual bool CanUse()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (BotOwner_0.Boss.Followers.Count == 0)
			{
				return false;
			}
			if (goalEnemy.Person.Side == EPlayerSide.Savage && !BotOwner_0.Memory.IsUnderFire && (BotOwner_0.Memory.LastDamageData == null || !(Time.time - BotOwner_0.Memory.LastDamageData.TimeDamage < BotOwner_0.Settings.FileSettings.Boss.WAIT_NO_ATTACK_SAVAGE)) && Time.time - goalEnemy.AddTime < BotOwner_0.Settings.FileSettings.Boss.WAIT_NO_ATTACK_SAVAGE)
			{
				return false;
			}
		}
		return true;
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
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.Distance < 10f)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("CLOSEANDVIS");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return CanUse();
		}
		return false;
	}

	public override string Name()
	{
		return "Bully Layer";
	}
}
