using System;
using EFT;

public class GClass355 : BaseBrain
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
	public const int Int_8 = 9;

	[NonSerialized]
	public const int Int_9 = 10;

	[NonSerialized]
	public const int Int_10 = 11;

	[NonSerialized]
	public const int Int_11 = 12;

	[NonSerialized]
	public const int Int_12 = 14;

	[NonSerialized]
	public const int Int_13 = 15;

	[NonSerialized]
	public const int Int_14 = 16;

	[NonSerialized]
	public const int Int_15 = 17;

	[NonSerialized]
	public const int Int_16 = 18;

	[NonSerialized]
	public const int Int_17 = 19;

	[NonSerialized]
	public const float Float_0 = 20f;

	[NonSerialized]
	public const int Int_18 = 21;

	public GClass355(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(6, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass87 layer3 = new GClass87(owner, 70);
		method_0(9, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 65);
		method_0(11, layer4, activeOnStart: true);
		GClass45 layer5 = new GClass45(owner, 59);
		method_0(21, layer5, activeOnStart: true);
		bool activeOnStart = Owner.Profile.Info.Settings.BotDifficulty == BotDifficulty.hard;
		GClass41 layer6 = new GClass41(owner, 58);
		method_0(18, layer6, activeOnStart);
		GClass39 layer7 = new GClass39(owner, 55);
		method_0(1, layer7, activeOnStart: true);
		GClass139 layer8 = new GClass139(owner, 30);
		method_0(5, layer8, activeOnStart: true);
		Class105 layer9 = new Class105(owner, 25);
		method_0(16, layer9, activeOnStart: true);
		GClass46 layer10 = new GClass46(owner, 20);
		method_0(3, layer10, activeOnStart: true);
		Class103 layer11 = new Class103(owner, 10);
		method_0(8, layer11, activeOnStart: true);
		GClass174 layer12 = new GClass174(owner, 8);
		method_0(7, layer12, activeOnStart: true);
		GClass132 layer13 = new GClass132(owner, 6);
		method_0(10, layer13, activeOnStart: true);
		GClass117 layer14 = new GClass117(owner, 4);
		method_0(19, layer14, activeOnStart: true);
		GClass133 layer15 = new GClass133(owner, 2);
		method_0(2, layer15, activeOnStart: true);
		Owner.BotsGroup.OnMemberRemove += method_6;
	}

	public void method_6(BotOwner obj)
	{
		Owner.BotsGroup.OnMemberRemove -= method_6;
		if (GClass856.IsTrue100(20f))
		{
			method_1(18);
		}
	}

	public override void Dispose()
	{
		Owner.BotsGroup.OnMemberRemove -= method_6;
		base.Dispose();
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, 55, 50, 76, 52, 76);
	}

	public override string ShortName()
	{
		return "Assault";
	}
}
