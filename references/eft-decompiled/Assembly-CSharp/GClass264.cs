using EFT;

public class GClass264 : GClass177<GClass26>
{
	public GClass264(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.PlanDropItem.ManualUpdate();
	}
}
