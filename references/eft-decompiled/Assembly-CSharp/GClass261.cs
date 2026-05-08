using EFT;

public class GClass261 : GClass177<GClass26>
{
	public GClass261(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.EatDrinkData.ManualUpdate();
	}
}
