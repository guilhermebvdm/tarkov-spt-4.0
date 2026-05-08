using EFT;

public class GClass218 : GClass200<GClass26>
{
	public GClass218(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.PatrollingData.MoveByReservWay.Update();
	}
}
