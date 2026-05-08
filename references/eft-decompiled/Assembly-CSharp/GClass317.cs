using System;
using EFT;

public class GClass317 : BaseBrain
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
	public const int Int_8 = 11;

	[NonSerialized]
	public const int Int_9 = 12;

	[NonSerialized]
	public GClass147 Gclass147_0;

	[NonSerialized]
	public GClass149 Gclass149_0;

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public GClass317(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		Gclass147_0 = new GClass147(owner, 62);
		method_0(6, Gclass147_0, activeOnStart: true);
		GClass39 layer4 = new GClass39(owner, 50);
		method_0(1, layer4, activeOnStart: true);
		Gclass149_0 = new GClass149(owner, 22);
		method_0(7, Gclass149_0, activeOnStart: true);
		GClass46 layer5 = new GClass46(owner, 9);
		method_0(3, layer5, activeOnStart: true);
		GClass132 layer6 = new GClass132(owner, 2);
		method_0(8, layer6, activeOnStart: true);
		GClass133 layer7 = new GClass133(owner, 0);
		method_0(2, layer7, activeOnStart: true);
	}

	public void ForceRecalcShootPos()
	{
		Gclass147_0.ForceRecalcShootPos();
	}

	public override string ShortName()
	{
		return "BossSanitar";
	}
}
