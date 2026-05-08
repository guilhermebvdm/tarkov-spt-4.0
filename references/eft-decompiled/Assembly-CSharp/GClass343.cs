using System;
using System.Runtime.CompilerServices;
using EFT;

public class GClass343 : BaseBrain
{
	[CompilerGenerated]
	public class Class235
	{
		public BotOwner mem;

		public void method_0()
		{
			mem.BotTalk.Say(EPhraseTrigger.Toxic, sayImmediately: true);
		}
	}

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 5;

	[NonSerialized]
	public const int Int_4 = 7;

	[NonSerialized]
	public const int Int_5 = 8;

	[NonSerialized]
	public const int Int_6 = 11;

	[NonSerialized]
	public const int Int_7 = 12;

	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public GClass111 Gclass111_0;

	[NonSerialized]
	public GClass110 Gclass110_0;

	public GClass343(BotOwner owner)
		: base(owner)
	{
		Owner.Memory.OnGoalEnemyChanged += method_8;
		Owner.WeaponManager.Melee.OnTryHit += method_6;
		GClass48 layer = new GClass48(owner, 90);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 88);
		method_0(12, layer2, activeOnStart: true);
		GClass114 layer3 = new GClass114(owner, 77);
		method_0(11, layer3, activeOnStart: true);
		Gclass111_0 = new GClass111(owner, 75);
		method_0(7, Gclass111_0, activeOnStart: true);
		Gclass110_0 = new GClass110(owner, 65);
		method_0(3, Gclass110_0, activeOnStart: true);
		GClass116 layer4 = new GClass116(owner, 45);
		method_0(1, layer4, activeOnStart: true);
		GClass133 layer5 = new GClass133(owner, 0);
		method_0(2, layer5, activeOnStart: true);
		Owner.GetPlayer.BeingHitAction += method_9;
	}

	public void method_6(BotOwner obj)
	{
		method_7();
	}

	public void method_7()
	{
		for (int i = 0; i < Owner.BotsGroup.MembersCount; i++)
		{
			if (!GClass856.IsTrue100(70f))
			{
				continue;
			}
			BotOwner mem = Owner.BotsGroup.Member(i);
			if (mem != Owner)
			{
				mem.AITaskManager.RegisterDelayedTask(GClass856.Random(0f, 2f), delegate
				{
					mem.BotTalk.Say(EPhraseTrigger.Toxic, sayImmediately: true);
				});
			}
		}
	}

	public void method_8(BotOwner obj)
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null && !goalEnemy.IsVisible)
		{
			Owner.BotTalk.SetSilence(30f);
		}
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}

	public void method_9(DamageInfoStruct damageInfo, EBodyPart bodyPart, float damageReducedByArmor)
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null && damageInfo.Player == goalEnemy && goalEnemy.Distance > 5f)
		{
			Gclass111_0.UpdateLastFarShotTime();
			Gclass110_0.UpdateLastFarShotTime();
		}
	}

	public override void Dispose()
	{
		Owner.Memory.OnGoalEnemyChanged -= method_8;
		Owner.WeaponManager.Melee.OnTryHit -= method_6;
		Owner.GetPlayer.BeingHitAction -= method_9;
		base.Dispose();
	}

	public override string ShortName()
	{
		return "BossKolontay";
	}
}
