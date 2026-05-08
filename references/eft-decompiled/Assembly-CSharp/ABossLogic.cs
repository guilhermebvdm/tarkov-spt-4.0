using System;
using EFT;

public abstract class ABossLogic : GClass429
{
	[NonSerialized]
	public BotBoss BossLogic;

	public ABossLogic(BotOwner owner, BotBoss bossLogic)
		: base(owner)
	{
		BossLogic = bossLogic;
	}

	public abstract void SetPatrolMode();

	public abstract void Activate();

	public abstract void BossLogicUpdate();

	public virtual void OnDrawGizmosSelected()
	{
	}

	public abstract void Dispose();
}
