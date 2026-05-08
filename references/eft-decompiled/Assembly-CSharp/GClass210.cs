using EFT;

public class GClass210 : GClass208
{
	public GClass210(BotOwner bot)
		: base(bot, 30f)
	{
	}

	public override void UpdateNodeByBrain(GClass29 data)
	{
		BotOwner_0.Steering.LookToMovingDirection();
		base.UpdateNodeByBrain(data);
	}
}
