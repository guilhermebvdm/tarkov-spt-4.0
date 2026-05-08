using System;
using EFT;

public class GClass351 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 6;

	[NonSerialized]
	public const int Int_2 = 5;

	[NonSerialized]
	public const int Int_3 = 12;

	[NonSerialized]
	public const int Int_4 = 2;

	[NonSerialized]
	public const int Int_5 = 3;

	public GClass351(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass94 layer2 = new GClass94(owner, 15);
		method_0(1, layer2, activeOnStart: true);
		GClass93 layer3 = new GClass93(owner, 1);
		method_0(2, layer3, activeOnStart: true);
	}

	public override string ShortName()
	{
		return "RavangeZryachiy";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}
}
