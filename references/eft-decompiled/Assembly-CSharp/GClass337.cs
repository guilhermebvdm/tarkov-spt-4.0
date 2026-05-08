using System;
using EFT;
using UnityEngine;

public abstract class GClass337 : BaseBrain
{
	[NonSerialized]
	public GClass435 Gclass435_0;

	public GClass337(BotOwner owner)
		: base(owner)
	{
	}

	public virtual void SubFindBoss(IBossToFollow bossToFollow)
	{
		if (bossToFollow.Player().AIData.BotOwner.Boss.BossLogic is GClass435 boss)
		{
			SetBoss(boss);
		}
		else
		{
			UnityEngine.Debug.LogError("Can't find boss gluhar");
		}
	}

	public virtual void SetBoss(GClass435 gluhar)
	{
		Gclass435_0 = gluhar;
	}
}
