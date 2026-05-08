using EFT;

public class GClass292 : GClass177<GClass26>
{
	public GClass292(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.ItemTaker.ManualUpdate();
	}
}
