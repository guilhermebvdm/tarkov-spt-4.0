using System;
using EFT;

public class GClass364 : BaseBrain
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

	public GClass364(BotOwner owner)
		: base(owner)
	{
		Owner.BotsController.EventsController.BotsMinotaurLabirint.Init(owner, Owner.BotsController);
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass84 layer3 = new GClass84(owner, 70);
		method_0(7, layer3, activeOnStart: true);
		GClass41 layer4 = new GClass41(owner, 50, 80f);
		method_0(2, layer4, activeOnStart: true);
		GClass139 layer5 = new GClass139(owner, 40);
		method_0(6, layer5, activeOnStart: true);
		GClass46 layer6 = new GClass46(owner, 9);
		method_0(4, layer6, activeOnStart: true);
		GClass133 layer7 = new GClass133(owner, 0);
		method_0(3, layer7, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, -1, 45, 76);
	}

	public override string ShortName()
	{
		return "HelperAgro";
	}
}
