using System;
using EFT;

public class GClass311 : BaseBrain
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
	public const int Int_8 = 9;

	[NonSerialized]
	public const int Int_9 = 10;

	[NonSerialized]
	public const int Int_10 = 11;

	[NonSerialized]
	public const int Int_11 = 12;

	[NonSerialized]
	public const int Int_12 = 13;

	[NonSerialized]
	public const int Int_13 = 14;

	public GClass311(BotOwner owner, bool withPursuit)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass87 layer3 = new GClass87(owner, 75);
		method_0(10, layer3, activeOnStart: true);
		GClass166 layer4 = new GClass166(owner, 73);
		method_0(11, layer4, activeOnStart: true);
		GClass84 layer5 = new GClass84(owner, 70);
		method_0(9, layer5, activeOnStart: true);
		GClass45 layer6 = new GClass45(owner, 61);
		method_0(14, layer6, activeOnStart: true);
		GClass142 layer7 = new GClass142(owner, 60);
		method_0(1, layer7, activeOnStart: true);
		GClass39 layer8 = new GClass39(owner, 50);
		method_0(6, layer8, activeOnStart: true);
		GClass139 layer9 = new GClass139(owner, 30);
		method_0(4, layer9, activeOnStart: true);
		if (withPursuit)
		{
			Class105 layer10 = new Class105(owner, 25);
			method_0(13, layer10, activeOnStart: true);
		}
		Class99 layer11 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer11, activeOnStart: true);
		GClass132 layer12 = new GClass132(owner, 3);
		method_0(8, layer12, activeOnStart: true);
		GClass86 layer13 = new GClass86(owner, 2);
		method_0(7, layer13, activeOnStart: true);
		GClass133 layer14 = new GClass133(owner, 0);
		method_0(2, layer14, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, 75, 55, 76, 47, 76);
	}

	public override string ShortName()
	{
		return "PMC";
	}
}
