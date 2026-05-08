using System;
using EFT;

public class GClass101 : GClass100
{
	public GClass101(BotOwner bot, int priority, Func<bool> shallUse)
		: base(bot, priority, shallUse)
	{
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return Func_0();
		}
		return false;
	}

	public override string Name()
	{
		return "HoldOrCoverF";
	}
}
