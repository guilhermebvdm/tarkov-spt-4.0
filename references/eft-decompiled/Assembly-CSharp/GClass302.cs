using EFT;

public class GClass302 : GClass301
{
	public GClass302(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.BotLay.TryLay();
		base.UpdateNodeByBrain(data);
	}
}
