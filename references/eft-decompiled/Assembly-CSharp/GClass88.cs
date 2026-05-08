using EFT;

public class GClass88 : GClass87
{
	public GClass88(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			if (BotOwner_0.CalledData != null)
			{
				return BotOwner_0.CalledData.Target != null;
			}
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return "GroupForce";
	}
}
