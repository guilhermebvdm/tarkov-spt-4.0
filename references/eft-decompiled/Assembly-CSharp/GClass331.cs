using System;
using EFT;

public class GClass331 : BaseBrain
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
	public const int Int_6 = 8;

	[NonSerialized]
	public const int Int_7 = 9;

	[NonSerialized]
	public const int Int_8 = 11;

	[NonSerialized]
	public string String_0;

	public GClass331(BotOwner owner, bool closePatrol)
		: base(owner)
	{
		GClass49 layer = new GClass49(owner, 120);
		method_0(3, layer, activeOnStart: true);
		GClass83 layer2 = new GClass83(owner, 80);
		method_0(8, layer2, activeOnStart: true);
		GClass118 layer3 = new GClass118(owner, 78);
		method_0(6, layer3, activeOnStart: true);
		GClass166 layer4 = new GClass166(owner, 75);
		method_0(11, layer4, activeOnStart: true);
		GClass84 layer5 = new GClass84(owner, 74);
		method_0(7, layer5, activeOnStart: true);
		GClass82 layer6 = new GClass82(owner, 70);
		method_0(5, layer6, activeOnStart: true);
		GClass58 layer7 = new GClass58(owner, 50);
		method_0(1, layer7, activeOnStart: true);
		GClass51 layer8;
		if (closePatrol)
		{
			String_0 = "FlBoarCl";
			layer8 = new GClass52(owner, 20);
		}
		else
		{
			String_0 = "FlBoarSt";
			layer8 = new GClass53(owner, 20);
		}
		method_0(2, layer8, activeOnStart: true);
		GClass135 layer9 = new GClass135(owner, 5, tryFinGreenFirst: false);
		method_0(9, layer9, activeOnStart: true);
	}

	public override string ShortName()
	{
		return String_0;
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}
}
