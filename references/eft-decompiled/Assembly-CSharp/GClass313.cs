using System;
using EFT;

public class GClass313 : BaseBrain
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

	public GClass313(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(4, layer, activeOnStart: true);
		GClass57 layer2 = new GClass57(owner, 30);
		method_0(1, layer2, activeOnStart: true);
		GClass59 layer3 = new GClass59(owner, 20);
		method_0(3, layer3, activeOnStart: true);
		GClass133 layer4 = new GClass133(owner, 1);
		method_0(2, layer4, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, 40);
	}

	public override string ShortName()
	{
		return "BoarSniper";
	}
}
