using EFT;

public class GClass47 : GClass46
{
	public GClass47(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (base.ShallUseNow())
		{
			return BotOwner_0.Memory.LastEnemy != null;
		}
		return false;
	}
}
