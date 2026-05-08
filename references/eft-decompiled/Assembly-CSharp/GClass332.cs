using System;
using EFT;

public class GClass332 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 7;

	[NonSerialized]
	public const int Int_2 = 2;

	[NonSerialized]
	public const int Int_3 = 3;

	[NonSerialized]
	public const int Int_4 = 4;

	[NonSerialized]
	public const int Int_5 = 5;

	[NonSerialized]
	public const int Int_6 = 6;

	[NonSerialized]
	public const int Int_7 = 8;

	[NonSerialized]
	public const int Int_8 = 11;

	[NonSerialized]
	public const int Int_9 = 12;

	public GClass332(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(6, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(5, layer4, activeOnStart: true);
		GClass143 layer5 = new GClass143(owner, 60);
		method_0(1, layer5, activeOnStart: true);
		GClass39 layer6 = new GClass39(owner, 50);
		method_0(7, layer6, activeOnStart: true);
		GClass139 layer7 = new GClass139(owner, 30);
		method_0(4, layer7, activeOnStart: true);
		Class99 layer8 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer8, activeOnStart: true);
		GClass86 layer9 = new GClass86(owner, 2);
		method_0(8, layer9, activeOnStart: true);
		GClass133 layer10 = new GClass133(owner, 0);
		method_0(2, layer10, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public override string ShortName()
	{
		return "FollowerBully";
	}
}
