using EFT;

public class GClass70 : BaseLogicLayerSimpleAbstractClass
{
	public GClass70(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override string Name()
	{
		return "SuppressBTR";
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "SuppressEnemy");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "NoEnemy");
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("NoEnemy");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("HaveEnemy");
	}
}
