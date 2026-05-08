using EFT;

public class GClass132 : BaseLogicLayerSimpleAbstractClass
{
	public GClass132(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.DeadBodyWork.ShallUse)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.deadBody, "Dead Body");
		}
		if (BotOwner_0.PlanDropItem.WantDropItem())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.botDropItem, "Drop Item");
		}
		if (BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.botTakeItem, "Take Item");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "Stub logic");
	}

	public override bool ShallUseNow()
	{
		BotOwner_0.DeadBodyWork.UpdateCheck();
		BotOwner_0.ItemTaker.RefreshClosestItems();
		if (!BotOwner_0.DeadBodyWork.ShallUse && !BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return BotOwner_0.PlanDropItem.WantDropItem();
		}
		return true;
	}

	public override string Name()
	{
		return "Utility peace";
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDeadBody()
	{
		if (BotOwner_0.DeadBodyWork.ShallUse)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndTakeItem()
	{
		if (BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDropItem()
	{
		if (BotOwner_0.PlanDropItem.WantDropItem())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}
}
