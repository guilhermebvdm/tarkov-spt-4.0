using EFT;

public class GClass42 : BaseLogicLayerSimpleAbstractClass
{
	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shoot");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "toEnemy");
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.AssaultBuildingData.IsActive)
		{
			return false;
		}
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalEnemy.Person.AIData.EnvironmentId > 0)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("canSht");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("cantShoot");
	}

	public override string Name()
	{
		return "Assault Building";
	}

	public GClass42(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}
}
