using EFT;

public class GClass196 : GClass177<GClass26>
{
	public GClass196(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.HealAnotherTarget.ManualUpdate();
	}
}
