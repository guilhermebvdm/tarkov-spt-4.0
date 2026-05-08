using System;
using EFT;
using UnityEngine;

public abstract class GClass38 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public bool Bool_4 = true;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public float Float_3 = 120f;

	public float Single_0 => BotOwner_0.Settings.FileSettings.Mind.DIST_TO_HIDE_ASSAULT;

	public GClass38(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass25_0 = new GClass25(3f, method_13);
	}

	public void method_13()
	{
		if (!(Time.time - BotOwner_0.Memory.UnderFireTime < Float_3) && Time.time - BotOwner_0.Memory.LastTimeHit >= Float_3)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.Distance <= Single_0)
			{
				Bool_4 = false;
			}
			else
			{
				Bool_4 = true;
			}
		}
		else
		{
			Bool_4 = false;
		}
	}
}
