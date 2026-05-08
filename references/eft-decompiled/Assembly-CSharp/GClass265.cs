using EFT;

public class GClass265 : GClass177<GClass26>
{
	public GClass265(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.ItemTaker.ManualUpdate();
	}
}
