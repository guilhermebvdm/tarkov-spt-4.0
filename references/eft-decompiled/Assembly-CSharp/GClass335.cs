using System;
using EFT;

public class GClass335 : BaseBrain
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
	public const int Int_6 = 11;

	public GClass335(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(4, layer, activeOnStart: true);
		GClass166 layer2 = new GClass166(owner, 75);
		method_0(11, layer2, activeOnStart: true);
		GClass169 layer3 = new GClass169(owner, 50);
		method_0(3, layer3, activeOnStart: true);
		GClass171 layer4 = new GClass171(owner, 40);
		method_0(1, layer4, activeOnStart: true);
		GClass173 layer5 = new GClass173(owner, 30);
		method_0(5, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 20);
		method_0(6, layer6, activeOnStart: true);
		GClass172 layer7 = new GClass172(owner, 0);
		method_0(2, layer7, activeOnStart: true);
	}

	public override string ShortName()
	{
		return "Fl_Zraychiy";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}
}
