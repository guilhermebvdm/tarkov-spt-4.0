using System;
using EFT;
using UnityEngine;

public class GClass206 : GClass205
{
	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public bool Bool_0;

	public GClass206(BotOwner bot)
		: base(bot)
	{
	}

	public override GClass178 GetAiming(BotOwner bot)
	{
		return new GClass181(bot);
	}

	public override void AimingAndShoot(GClass26 data)
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(2f, 4f);
			Bool_0 = !Bool_0;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Bool_0 || (goalEnemy != null && goalEnemy.CanShoot))
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
	}
}
