using EFT;

public class GClass246 : GClass200<GClass26>
{
	public GClass246(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.PriorityAxeTarget.ManualUpdate();
	}
}
