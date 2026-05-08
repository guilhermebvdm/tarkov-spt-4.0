using System;
using EFT;

public class GClass340 : GClass337
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
	public const int Int_5 = 9;

	[NonSerialized]
	public const int Int_6 = 10;

	[NonSerialized]
	public const int Int_7 = 11;

	[NonSerialized]
	public const int Int_8 = 12;

	[NonSerialized]
	public const int Int_9 = 22;

	[NonSerialized]
	public const int Int_10 = 43;

	public GClass340(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(10, layer4, activeOnStart: true);
		GClass144 layer5 = new GClass144(owner, 60);
		method_0(1, layer5, activeOnStart: true);
		GClass39 layer6 = new GClass39(owner, 50);
		method_0(7, layer6, activeOnStart: true);
		GClass91 layer7 = new GClass91(owner, 46);
		method_0(3, layer7, activeOnStart: true);
		GClass139 layer8 = new GClass139(owner, 30);
		method_0(9, layer8, activeOnStart: true);
		GClass46 layer9 = new GClass46(owner, 9);
		method_0(43, layer9, activeOnStart: true);
		GClass86 layer10 = new GClass86(owner, 2);
		method_0(22, layer10, activeOnStart: true);
		GClass133 layer11 = new GClass133(owner, 0);
		method_0(2, layer11, activeOnStart: true);
		method_6();
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public override string ShortName()
	{
		return "FollowerGluharProtect";
	}

	public override void SetBoss(GClass435 gluhar)
	{
		base.SetBoss(gluhar);
	}

	public void method_6()
	{
		if (Gclass435_0 == null)
		{
			if (Owner.BotFollower.BossToFollow != null)
			{
				SubFindBoss(Owner.BotFollower.BossToFollow);
			}
			else
			{
				Owner.BotFollower.OnBossFinded += method_7;
			}
		}
	}

	public void method_7(IBossToFollow bossToFollow)
	{
		SubFindBoss(bossToFollow);
		Owner.BotFollower.OnBossFinded -= method_7;
	}
}
