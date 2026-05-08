using System;
using EFT;

public class GClass345 : BaseBrain
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
	public const int Int_10 = 22;

	[NonSerialized]
	public const int Int_11 = 43;

	public GClass345(BotOwner owner)
		: base(owner)
	{
		Owner.Memory.OnGoalEnemyChanged += method_6;
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass114 layer3 = new GClass114(owner, 77);
		method_0(11, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(10, layer4, activeOnStart: true);
		GClass112 layer5 = new GClass112(owner, 60);
		method_0(1, layer5, activeOnStart: true);
		GClass166 layer6 = new GClass166(owner, 50);
		method_0(8, layer6, activeOnStart: true);
		GClass92 layer7 = new GClass92(owner, 46);
		method_0(3, layer7, activeOnStart: true);
		GClass139 layer8 = new GClass139(owner, 30);
		method_0(9, layer8, activeOnStart: true);
		GClass116 layer9 = new GClass116(owner, 9);
		method_0(43, layer9, activeOnStart: true);
		GClass86 layer10 = new GClass86(owner, 2);
		method_0(22, layer10, activeOnStart: true);
		GClass133 layer11 = new GClass133(owner, 0);
		method_0(2, layer11, activeOnStart: true);
	}

	public void method_6(BotOwner obj)
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null && !goalEnemy.IsVisible)
		{
			Owner.BotTalk.SetSilence(30f);
		}
	}

	public override string ShortName()
	{
		return "KolonSec";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 67, 48, 74);
	}

	public override void Dispose()
	{
		Owner.Memory.OnGoalEnemyChanged -= method_6;
		base.Dispose();
	}
}
