using EFT;

public class GClass69 : BaseLogicLayerSimpleAbstractClass
{
	public GClass69(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override string Name()
	{
		return "BaseBTR";
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootEnemy");
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
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("HaveEnemy");
		}
		return AICoreActionEndStruct_1;
	}
}
