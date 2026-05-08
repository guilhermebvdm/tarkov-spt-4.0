using System;
using EFT;

public class GClass320 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	public GClass320(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 180);
		method_0(2, layer, activeOnStart: true);
		GClass164 layer2 = new GClass164(owner, 100);
		method_0(1, layer2, activeOnStart: true);
		GClass165 layer3 = new GClass165(owner, 80);
		method_0(3, layer3, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}

	public override string ShortName()
	{
		return "BossTest";
	}
}
