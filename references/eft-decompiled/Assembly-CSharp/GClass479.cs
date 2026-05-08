using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass479 : BotEnemyChooser
{
	[NonSerialized]
	public EnemyInfo EnemyInfo_0;

	[NonSerialized]
	public GClass466 Gclass466_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	public GClass479([NotNull] BotOwner owner)
		: base(owner)
	{
		BotOwner_0.OnBotStateChange += method_0;
	}

	public override EnemyInfo FindDangerEnemy()
	{
		EnemyInfo_0 = base.FindDangerEnemy();
		return EnemyInfo_0;
	}

	public override float CalcWeight(float dist, EnemyInfo enemyInfo)
	{
		float num = dist;
		if (!enemyInfo.IsVisible)
		{
			num += 1000000f;
		}
		if (enemyInfo == EnemyInfo_0)
		{
			num += 1000000f;
		}
		return num;
	}

	public void method_0(EBotState obj)
	{
		if (obj == EBotState.Active && BotOwner_0.WeaponManager.Selector is GClass466 gclass466_)
		{
			Bool_0 = true;
			Gclass466_0 = gclass466_;
			BotOwner_0.ShootData.OnTriggerPressed += method_1;
		}
	}

	public void method_1()
	{
		method_2();
	}

	public void method_2()
	{
		if (Bool_0 && !(Float_0 > Time.time))
		{
			Float_0 = Time.time + 0.5f;
			if (Gclass466_0.IsGrenadeLauncher)
			{
				BotOwner_0.CalcGoal();
			}
		}
	}

	public override void Dispose()
	{
		Bool_0 = false;
		Gclass466_0 = null;
		BotOwner_0.ShootData.OnTriggerPressed -= method_1;
	}
}
