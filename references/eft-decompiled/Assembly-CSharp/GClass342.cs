using System;
using EFT;

public class GClass342 : BaseBrain
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
	public const int Int_5 = 12;

	public GClass342(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass62 layer3 = new GClass62(owner, 60);
		method_0(1, layer3, activeOnStart: true);
		GClass46 layer4 = new GClass46(owner, 9);
		method_0(3, layer4, activeOnStart: true);
		GClass133 layer5 = new GClass133(owner, 0);
		method_0(2, layer5, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, -1, 55, 76, -1, 76);
	}

	public override string ShortName()
	{
		return "Killa";
	}
}
