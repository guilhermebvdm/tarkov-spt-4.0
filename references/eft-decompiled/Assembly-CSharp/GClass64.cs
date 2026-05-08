using System;
using EFT;
using JetBrains.Annotations;

public class GClass64 : GClass62
{
	[NonSerialized]
	public GClass435 Gclass435_0;

	public GClass64([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public void SetBoss(GClass435 gluhar)
	{
		Gclass435_0 = gluhar;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy && Gclass435_0 != null && Gclass435_0.AssaultFollowersShallAttack)
		{
			return BotOwner_0.BotFollower.HaveBoss;
		}
		return false;
	}

	public override string Name()
	{
		return "GluharKilla";
	}
}
