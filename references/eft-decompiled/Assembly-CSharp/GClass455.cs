using System;
using EFT;
using UnityEngine;

public class GClass455 : BotFollower
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	public GClass455(BotOwner owner)
		: base(owner)
	{
		BotOwner_0.Memory.OnGoalEnemyChanged += method_5;
	}

	public void method_5(BotOwner obj)
	{
		if (base.HaveBoss)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && !goalEnemy.IsVisible)
			{
				method_6(goalEnemy);
			}
		}
	}

	public void method_6(EnemyInfo myEnemy)
	{
		if (!(Float_0 > Time.time))
		{
			Float_0 = Time.time + 20f;
			Bool_0 = myEnemy.Distance < 25f;
		}
	}

	public override bool Dispose()
	{
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_5;
		return base.Dispose();
	}
}
