using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class BotCurrentEnemiesClass
{
	public int ActiveEnemies;

	[NonSerialized]
	public GClass641.IBotTimer IBotTimer;

	[NonSerialized]
	public BotsGroup BotsGroup_0;

	[NonSerialized]
	public float Float_0 = 10f;

	[CompilerGenerated]
	private Action<int> action_0;

	public event Action<int> OnCurrentCheck
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public BotCurrentEnemiesClass(BotsGroup group)
	{
		Float_0 = LocalBotSettingsProviderClass.Core.ENEMY_TO_BE_CURRENT;
		BotsGroup_0 = group;
	}

	public void GetVision()
	{
		method_1();
	}

	public void LoseVision()
	{
		method_2();
		IBotTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(Float_0 + 1f));
		IBotTimer.OnTimer += method_0;
	}

	public void method_0()
	{
		method_1();
	}

	public void method_1()
	{
		int num = 0;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in BotsGroup_0.Enemies)
		{
			float enemyLastSeenTimeReal = enemy.Value.EnemyLastSeenTimeReal;
			if (Time.time - enemyLastSeenTimeReal < Float_0 && enemyLastSeenTimeReal > 1f)
			{
				num++;
			}
		}
		if (num != ActiveEnemies)
		{
			ActiveEnemies = num;
			if (action_0 != null)
			{
				action_0(ActiveEnemies);
			}
		}
	}

	public void method_2()
	{
		if (IBotTimer != null && IBotTimer.IsActive)
		{
			IBotTimer.Stop();
		}
	}

	public void Dispose()
	{
		method_2();
	}
}
