using EFT;

public class GClass106 : GClass102
{
	public GClass106(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveGoal;
	}

	public override string Name()
	{
		return "Kojaniy Target";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return AICoreActionEndStruct_1;
		}
		if ((!goalEnemy.IsVisible || !goalEnemy.CanShoot) && BotOwner_0.Memory.IsInCover && !CanSearchEnemy())
		{
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("enemy.Visib");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("enemynull");
	}

	public override bool ShallAttack()
	{
		return false;
	}
}
