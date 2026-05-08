using System;
using EFT;

public class BossKnightBrainClass : GClass326
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 4;

	[NonSerialized]
	public const int Int_4 = 5;

	[NonSerialized]
	public const int Int_5 = 6;

	[NonSerialized]
	public const int Int_6 = 7;

	[NonSerialized]
	public const int Int_7 = 8;

	[NonSerialized]
	public const int Int_8 = 11;

	[NonSerialized]
	public const int Int_9 = 12;

	[NonSerialized]
	public const int Int_10 = 14;

	[NonSerialized]
	public GClass80 Gclass80_0;

	public BossKnightBrainClass(BotOwner owner)
		: base(owner)
	{
		method_6();
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass42 layer4 = new GClass42(owner, 72);
		method_0(14, layer4, activeOnStart: true);
		GClass43 layer5 = new GClass43(owner, 70);
		method_0(8, layer5, activeOnStart: true);
		Gclass80_0 = new GClass80(owner, 62);
		method_0(1, Gclass80_0, activeOnStart: true);
		GClass39 layer6 = new GClass39(owner, 50);
		method_0(2, layer6, activeOnStart: true);
		GClass139 layer7 = new GClass139(owner, 30);
		method_0(6, layer7, activeOnStart: true);
		GClass79 layer8 = new GClass79(owner, 0, ECoverPatrolControls.distances, 30f, 100f, (ECoverPointSpecial)0);
		method_0(3, layer8, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public void ForceRecalcShootPos()
	{
		Gclass80_0.ForceRecalcShootPos();
	}

	public override string ShortName()
	{
		return "Knight";
	}
}
