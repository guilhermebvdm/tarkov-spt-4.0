using EFT;

public class GClass119 : BaseLogicLayerSimpleAbstractClass
{
	public GClass119(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.BotLay.CanProne)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "inCov");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "inCov");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveGoal;
	}

	public override string Name()
	{
		return "MarksmanTarget";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		if (method_12(5f))
		{
			return new AICoreActionEndStruct("getHitLay");
		}
		return AICoreActionEndStruct_1;
	}
}
