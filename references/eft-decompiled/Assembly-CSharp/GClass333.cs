using System;
using EFT;

public class GClass333 : BaseBrain
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
	public const int Int_4 = 6;

	[NonSerialized]
	public const int Int_5 = 7;

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
	public GClass148 Gclass148_0;

	[NonSerialized]
	public GClass149 Gclass149_0;

	public int Id => Owner.Id;

	public GClass333(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(13, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(11, layer4, activeOnStart: true);
		Gclass148_0 = new GClass148(owner, 62);
		method_0(6, Gclass148_0, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 50);
		method_0(1, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 30);
		method_0(9, layer6, activeOnStart: true);
		Gclass149_0 = new GClass149(owner, 22);
		method_0(7, Gclass149_0, activeOnStart: true);
		GClass46 layer7 = new GClass46(owner, 9);
		method_0(3, layer7, activeOnStart: true);
		GClass86 layer8 = new GClass86(owner, 2);
		method_0(10, layer8, activeOnStart: true);
		GClass133 layer9 = new GClass133(owner, 0);
		method_0(2, layer9, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public void ForceRecalcShootPos()
	{
		Gclass148_0.ForceRecalcShootPos();
	}

	public void SetBossSanitar(GInterface7 bossSanitar)
	{
		Gclass148_0.SetBossData(bossSanitar);
		Gclass149_0.SetBossData(bossSanitar);
	}

	public override string ShortName()
	{
		return "FollowerSanitar";
	}
}
