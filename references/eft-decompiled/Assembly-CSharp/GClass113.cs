using System;
using EFT;

public class GClass113(BotOwner bot, int priority) : GClass112(bot, priority)
{
	[NonSerialized]
	public bool Bool_5 = true;

	public override bool ShallUseNow()
	{
		if (Bool_5)
		{
			return base.ShallUseNow();
		}
		return false;
	}

	public override string Name()
	{
		return "KolontayAP";
	}

	public void SetProtect(bool protectBoss)
	{
		Bool_5 = protectBoss;
	}
}
