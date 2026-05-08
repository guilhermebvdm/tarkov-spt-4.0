using System;
using EFT;

public class GClass314 : BaseBrain
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
	public const int Int_5 = 11;

	[NonSerialized]
	public const int Int_6 = 12;

	public GClass314(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass60 layer4 = new GClass60(owner, 60);
		method_0(6, layer4, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 50);
		method_0(1, layer5, activeOnStart: true);
		Class99 layer6 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer6, activeOnStart: true);
		GClass133 layer7 = new GClass133(owner, 0);
		method_0(2, layer7, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}

	public override string ShortName()
	{
		return "BossBully";
	}
}
