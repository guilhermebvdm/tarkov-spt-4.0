using EFT;

public class GClass120 : BaseLogicLayerSimpleAbstractClass
{
	public GClass120(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.DangerPointsData.IsPanic)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.panicSitting, "panicSittin");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "HoldOrCover");
	}

	public override string Name()
	{
		return "Panic";
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.DangerPointsData.IsPanic;
	}

	public override AICoreActionEndStruct EndRunAwayGrenade()
	{
		if (!BotOwner_0.DangerPointsData.IsPanic)
		{
			return new AICoreActionEndStruct("IsPanic");
		}
		return AICoreActionEndStruct_1;
	}
}
