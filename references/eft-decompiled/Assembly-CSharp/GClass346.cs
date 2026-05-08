using System;
using EFT;

public class GClass346 : BaseBrain
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

	public GClass346(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(4, layer, activeOnStart: true);
		GClass40 layer2 = new GClass40(owner, 30);
		method_0(1, layer2, activeOnStart: true);
		GClass119 layer3 = new GClass119(owner, 20);
		method_0(3, layer3, activeOnStart: true);
		GClass174 layer4 = new GClass174(owner, 3);
		method_0(5, layer4, activeOnStart: true);
		GClass133 layer5 = new GClass133(owner, 1);
		method_0(2, layer5, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, 40, 26);
	}

	public override string ShortName()
	{
		return "Marksman";
	}
}
