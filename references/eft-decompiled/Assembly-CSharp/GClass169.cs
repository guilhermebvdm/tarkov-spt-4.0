using EFT;

public class GClass169 : GClass168
{
	public const float FIGHT_DIST = 15f;

	public GClass169(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "canshootnow");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "can'tshoot");
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		return base.EndShootFromCover();
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalEnemy is GClass541 { CloseZone: not false })
		{
			return true;
		}
		if (BotOwner_0.Memory.GoalEnemy.Distance < 15f)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndCrawlNode()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return base.EndRunToEnemy();
	}

	public override string Name()
	{
		return "CloseZrFight";
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		return new AICoreActionEndStruct("fightCL");
	}
}
