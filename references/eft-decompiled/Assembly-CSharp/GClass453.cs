using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass453<T> : GClass429 where T : ABossLogic
{
	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public T Gparam_0;

	public T BossLogic
	{
		[CompilerGenerated]
		get
		{
			return Gparam_0;
		}
		[CompilerGenerated]
		set
		{
			Gparam_0 = value;
		}
	}

	public GClass453(BotOwner owner)
		: base(owner)
	{
	}

	public void FindBoss(Action callbackOnFind = null)
	{
		if (BossLogic == null)
		{
			Action_0 = callbackOnFind;
			if (BotOwner_0.Boss.IamBoss)
			{
				BossLogic = BotOwner_0.Boss.BossLogic as T;
			}
			else if (BotOwner_0.BotFollower.BossToFollow != null)
			{
				method_1(BotOwner_0.BotFollower.BossToFollow);
			}
			else
			{
				BotOwner_0.BotFollower.OnBossFinded += method_0;
			}
		}
	}

	public bool HaveLogic()
	{
		return BossLogic != null;
	}

	public void method_0(IBossToFollow bossToFollow)
	{
		method_1(bossToFollow);
		BotOwner_0.BotFollower.OnBossFinded -= method_0;
	}

	public void method_1(IBossToFollow bossToFollow)
	{
		BossLogic = bossToFollow.Player().AIData.BotOwner.Boss.BossLogic as T;
		if (BossLogic == null)
		{
			Debug.LogError($"Can't find boss {typeof(T)}");
		}
		if (BossLogic != null && Action_0 != null)
		{
			Action_0();
		}
	}

	public void Dispose()
	{
		BotOwner_0.BotFollower.OnBossFinded -= method_0;
	}
}
