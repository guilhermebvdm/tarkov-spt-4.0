using EFT;

public class GClass202 : GClass200<GClass26>
{
	public GClass202(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.DeadBodyWork.ManualUpdate();
	}
}
