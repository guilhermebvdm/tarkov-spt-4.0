using EFT;

public class GClass174 : BaseLogicLayerAbstractClass
{
	public GClass174(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.standBy, "one reason");
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.StandBy.StandByType != BotStandByType.none)
		{
			return BotOwner_0.StandBy.StandByType != BotStandByType.active;
		}
		return false;
	}

	public override string Name()
	{
		return "StandBy";
	}

	public override AICoreActionEndStruct EndStandBy()
	{
		return AICoreActionEndStruct_1;
	}
}
