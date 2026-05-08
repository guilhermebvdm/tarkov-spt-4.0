using EFT;

public class GClass166 : BaseLogicLayerSimpleAbstractClass
{
	public GClass166(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.warnPlayer, "WPD");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.WarnData.WarnPlayerRequest != null;
	}

	public override AICoreActionEndStruct EndWarnPlayer()
	{
		if (!BotOwner_0.WarnData.WarnPlayerRequest.CanProceed())
		{
			BotOwner_0.WarnData.StopWarn();
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override string Name()
	{
		return "Warn";
	}
}
