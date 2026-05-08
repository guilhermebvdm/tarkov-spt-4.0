using System;
using EFT;

public class GClass319 : BaseBrain
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
	public const int Int_7 = 12;

	public GClass319(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass84 layer3 = new GClass84(owner, 70);
		method_0(7, layer3, activeOnStart: true);
		GClass160 layer4 = new GClass160(owner, 60);
		method_0(1, layer4, activeOnStart: true);
		GClass162 layer5 = new GClass162(owner, 50);
		method_0(2, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 40);
		method_0(6, layer6, activeOnStart: true);
		GClass46 layer7 = new GClass46(owner, 9);
		method_0(4, layer7, activeOnStart: true);
		GClass133 layer8 = new GClass133(owner, 0);
		method_0(3, layer8, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, 45, 76);
	}

	public override string ShortName()
	{
		return "Tagilla";
	}
}
