using System;
using EFT;

public class GClass338 : GClass337
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
	public const int Int_6 = 12;

	[NonSerialized]
	public const int Int_7 = 13;

	public GClass338(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 90);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 88);
		method_0(12, layer2, activeOnStart: true);
		GClass66 layer3 = new GClass66(owner, 65);
		method_0(1, layer3, activeOnStart: true);
		GClass39 layer4 = new GClass39(owner, 50);
		method_0(7, layer4, activeOnStart: true);
		GClass144 layer5 = new GClass144(owner, 60);
		method_0(8, layer5, activeOnStart: true);
		GClass133 layer6 = new GClass133(owner, 0);
		method_0(2, layer6, activeOnStart: true);
		Class99 layer7 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer7, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}

	public override string ShortName()
	{
		return "BossGluhar";
	}
}
