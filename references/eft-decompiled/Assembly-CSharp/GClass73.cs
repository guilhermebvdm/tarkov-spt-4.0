using EFT;

public class GClass73 : BaseLogicLayerSimpleAbstractClass
{
	public GClass73(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.khorovodChristmasEvent, "because");
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}

	public override AICoreActionEndStruct EndKhorovodChristmasEvent()
	{
		if (BotOwner_0.BotsController.EventsController.KhorovodEvent.IsActive && !BotOwner_0.Memory.HaveEnemy)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.BotsController.EventsController.KhorovodEvent.IsActive)
		{
			return false;
		}
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (BotOwner_0.GameEventsData.KhorovodGameEvent.ChristmasTree == null)
		{
			return false;
		}
		int connectionGroupId = BotOwner_0.GameEventsData.KhorovodGameEvent.ChristmasTree.ConnectionGroupId;
		if (connectionGroupId > 0 && BotOwner_0.StartCorePoint.ConnectionGroupId != connectionGroupId)
		{
			return false;
		}
		if (BotOwner_0.GameEventsData.KhorovodGameEvent.ChristmasTree.gameObject.activeInHierarchy)
		{
			return BotOwner_0.GameEventsData.KhorovodGameEvent.ChristmasTree.ProcessKhorovodPlace(BotOwner_0);
		}
		return false;
	}

	public override string Name()
	{
		return "Khorovod";
	}
}
