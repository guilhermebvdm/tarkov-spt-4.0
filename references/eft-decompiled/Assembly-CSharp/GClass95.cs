using System;
using EFT;

public abstract class GClass95 : BaseLogicLayerSimpleAbstractClass
{
	public GClass444 GClass444_0
	{
		get
		{
			try
			{
				if (BotOwner_0.Boss.IamBoss)
				{
					return BotOwner_0.Boss.BossLogic as GClass444;
				}
				return BotOwner_0.BotFollower.BossToFollow.GetBossLogic() as GClass444;
			}
			catch (Exception)
			{
			}
			return null;
		}
	}

	public GClass95(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}
}
