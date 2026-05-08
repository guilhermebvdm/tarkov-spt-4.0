using EFT;

public class GClass204 : GClass200<GClass26>
{
	public GClass204(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.PlayerFollowData.UpdateFromNode();
	}
}
