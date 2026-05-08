using System;
using EFT;

public class GClass325 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 3;

	[NonSerialized]
	public const int Int_1 = 4;

	[NonSerialized]
	public const int Int_2 = 5;

	[NonSerialized]
	public const int Int_3 = 14;

	public GClass325(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		Class100 layer2 = new Class100(owner, 72);
		method_0(14, layer2, activeOnStart: true);
		GClass74 layer3 = new GClass74(owner, 10);
		method_0(3, layer3, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public override string ShortName()
	{
		return "Obdolbs";
	}
}
