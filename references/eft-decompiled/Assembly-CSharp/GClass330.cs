using System;
using EFT;

public class GClass330 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public const int Int_1 = 7;

	[NonSerialized]
	public const int Int_2 = 5;

	[NonSerialized]
	public const int Int_3 = 12;

	[NonSerialized]
	public const int Int_4 = 3;

	[NonSerialized]
	public const int Int_5 = 6;

	[NonSerialized]
	public const int Int_6 = 8;

	[NonSerialized]
	public const int Int_7 = 11;

	[NonSerialized]
	public const int Int_8 = 13;

	public GClass330(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		GClass42 layer4 = new GClass42(owner, 72);
		method_0(13, layer4, activeOnStart: true);
		GClass43 layer5 = new GClass43(owner, 70);
		method_0(7, layer5, activeOnStart: true);
		GClass78 layer6 = new GClass78(owner, 55);
		method_0(8, layer6, activeOnStart: true);
		GClass77 layer7 = new GClass77(owner, 50);
		method_0(2, layer7, activeOnStart: true);
		GClass139 layer8 = new GClass139(owner, 30);
		method_0(6, layer8, activeOnStart: true);
		GClass79 layer9 = new GClass79(owner, 0, ECoverPatrolControls.byY, 30f, 100f, ECoverPointSpecial.noSnipePatrol);
		method_0(3, layer9, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
	}

	public override string ShortName()
	{
		return "BirdEye";
	}
}
