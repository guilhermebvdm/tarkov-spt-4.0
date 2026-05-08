using EFT;

public class GClass89 : BaseLogicLayerSimpleAbstractClass
{
	public GClass89(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.GiftData.CalcGiftsOnStart();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.botTakeItem, "Take Item");
		}
		BotOwner_0.Gesture.TryGestus(EInteraction.ComeWithMeGesture, withAimingDelay: false);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.doGiftChristmasEvent, "DoGift");
	}

	public override bool ShallUseNow()
	{
		BotOwner_0.GiftData.UpdateGiftTarget();
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			if (!BotOwner_0.GiftData.HaveUngiftedPlayerNear())
			{
				return BotOwner_0.ItemTaker.HaveItemToTake();
			}
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndDoGiftChristmasEvent()
	{
		if (BotOwner_0.GiftData.HaveUngiftedPlayerNear() && !BotOwner_0.ItemTaker.HaveItemToTake())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override string Name()
	{
		return "Gifter";
	}
}
