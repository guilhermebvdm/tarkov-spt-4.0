using EFT;

public class GClass243 : GClass200<GClass26>
{
	public GClass243(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetPose(1f);
		BotOwner_0.LeaveData.UpdateByNode();
	}
}
