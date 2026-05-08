using EFT;

public class GClass288 : GClass177<GClass26>
{
	public GClass288(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.BotTurnAwayLight.IsActive)
		{
			BotOwner_0.BotTurnAwayLight.Update();
		}
	}
}
