using EFT;

public class GClass249 : GClass200<GClass26>
{
	public GClass249(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetPose(1f);
		BotOwner_0.BotFollower.PatrolDataFollower.ManualUpdate();
	}
}
