using System;
using EFT;

public class GClass344 : BaseBrain
{
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
	public const int Int_6 = 9;

	[NonSerialized]
	public const int Int_7 = 10;

	[NonSerialized]
	public const int Int_8 = 11;

	[NonSerialized]
	public const int Int_9 = 12;

	[NonSerialized]
	public const int Int_10 = 13;

	[NonSerialized]
	public const int Int_11 = 22;

	[NonSerialized]
	public const int Int_12 = 43;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass115 Gclass115_0;

	[NonSerialized]
	public GClass113 Gclass113_0;

	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	public GClass344(BotOwner owner)
		: base(owner)
	{
		Owner.Memory.OnGoalEnemyChanged += method_6;
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass114 layer3 = new GClass114(owner, 77);
		method_0(11, layer3, activeOnStart: true);
		GClass166 layer4 = new GClass166(owner, 75);
		method_0(8, layer4, activeOnStart: true);
		GClass84 layer5 = new GClass84(owner, 70);
		method_0(10, layer5, activeOnStart: true);
		Gclass115_0 = new GClass115(owner, 60, 300f);
		method_0(1, Gclass115_0, activeOnStart: true);
		Gclass113_0 = new GClass113(owner, 59);
		method_0(13, Gclass113_0, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 30);
		method_0(9, layer6, activeOnStart: true);
		GClass116 layer7 = new GClass116(owner, 9);
		method_0(43, layer7, activeOnStart: true);
		GClass86 layer8 = new GClass86(owner, 2);
		method_0(22, layer8, activeOnStart: true);
		GClass133 layer9 = new GClass133(owner, 0);
		method_0(2, layer9, activeOnStart: true);
		Gclass453_0 = new GClass453<GClass441>(owner);
		Gclass453_0.FindBoss();
		SetProtect(v: false);
	}

	public override string ShortName()
	{
		return "FlKlnAslt";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 67, 45, 74);
	}

	public void method_6(BotOwner obj)
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			method_7(goalEnemy);
		}
	}

	public void method_7(EnemyInfo myEnemy)
	{
		if (myEnemy.IsVisible && myEnemy.HaveSeenPersonal)
		{
			bool protectBoss = myEnemy.Distance < 25f;
			Gclass453_0.BossLogic.MyAssaultCheckProtect(protectBoss);
		}
	}

	public void SetProtect(bool v)
	{
		Gclass115_0.SetProtect(v);
		Gclass113_0.SetProtect(v);
	}
}
