using System;
using EFT;

public class GClass339 : GClass337
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
	public const int Int_10 = 15;

	[NonSerialized]
	public const int Int_11 = 43;

	[NonSerialized]
	public GClass67 Gclass67_0;

	[NonSerialized]
	public GClass90 Gclass90_0;

	[NonSerialized]
	public GClass64 Gclass64_0;

	public GClass339(BotOwner owner)
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
		Gclass67_0 = new GClass67(owner, 65);
		method_0(1, Gclass67_0, activeOnStart: true);
		Gclass64_0 = new GClass64(owner, 55);
		method_0(8, Gclass64_0, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 50);
		method_0(7, layer5, activeOnStart: true);
		GClass101 layer6 = new GClass101(owner, 47, Gclass67_0.ShallGoNearBoss);
		method_0(15, layer6, activeOnStart: true);
		GClass139 layer7 = new GClass139(owner, 30);
		method_0(9, layer7, activeOnStart: true);
		Gclass90_0 = new GClass90(owner, 14);
		method_0(3, Gclass90_0, activeOnStart: true);
		GClass46 layer8 = new GClass46(owner, 9);
		method_0(43, layer8, activeOnStart: true);
		GClass133 layer9 = new GClass133(owner, 0);
		method_0(2, layer9, activeOnStart: true);
		method_6();
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 53, 76);
	}

	public override string ShortName()
	{
		return "FollowerGluharAssault";
	}

	public override void SetBoss(GClass435 gluhar)
	{
		base.SetBoss(gluhar);
		Gclass67_0.SetBoss(gluhar);
		Gclass90_0.SetBoss(gluhar);
		Gclass64_0.SetBoss(gluhar);
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
