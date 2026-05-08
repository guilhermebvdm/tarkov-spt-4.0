using System;
using EFT;

public class GClass322 : BaseBrain
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

	public GClass322(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(6, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass87 layer4 = new GClass87(owner, 65);
		method_0(9, layer4, activeOnStart: true);
		GClass88 layer5 = new GClass88(owner, 50);
		method_0(1, layer5, activeOnStart: true);
		owner.PriorityAxeTarget.AllPursuit = true;
		Class105 layer6 = new Class105(owner, 45);
		method_0(13, layer6, activeOnStart: true);
		GClass139 layer7 = new GClass139(owner, 30);
		method_0(5, layer7, activeOnStart: true);
		Class103 layer8 = new Class103(owner, 4);
		method_0(8, layer8, activeOnStart: true);
		GClass174 layer9 = new GClass174(owner, 3);
		method_0(7, layer9, activeOnStart: true);
		GClass132 layer10 = new GClass132(owner, 2);
		method_0(10, layer10, activeOnStart: true);
		GClass133 layer11 = new GClass133(owner, 1);
		method_0(2, layer11, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, 76);
	}

	public override string ShortName()
	{
		return "CursAssault";
	}
}
