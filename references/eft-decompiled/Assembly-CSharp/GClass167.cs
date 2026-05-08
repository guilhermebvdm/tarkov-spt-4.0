using System;
using EFT;

public abstract class GClass167 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public ZyriachyBossLogicClass ZyriachyBossLogicClass;

	public GClass167(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public ZyriachyBossLogicClass method_13()
	{
		if (ZyriachyBossLogicClass != null)
		{
			return ZyriachyBossLogicClass;
		}
		if (BotOwner_0.Boss.IamBoss)
		{
			ZyriachyBossLogicClass = BotOwner_0.Boss.BossLogic as ZyriachyBossLogicClass;
		}
		else
		{
			ZyriachyBossLogicClass = BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.Boss.BossLogic as ZyriachyBossLogicClass;
		}
		return ZyriachyBossLogicClass;
	}
}
