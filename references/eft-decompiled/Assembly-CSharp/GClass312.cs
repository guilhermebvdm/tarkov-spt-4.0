using System;
using EFT;

public class GClass312 : BaseBrain
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
	public const int Int_4 = 11;

	public GClass312(BotOwner owner)
		: base(owner)
	{
		GClass49 layer = new GClass49(owner, 100);
		method_0(3, layer, activeOnStart: true);
		GClass166 layer2 = new GClass166(owner, 75);
		method_0(11, layer2, activeOnStart: true);
		GClass118 layer3 = new GClass118(owner, 60);
		method_0(4, layer3, activeOnStart: true);
		GClass56 layer4 = new GClass56(owner, 50);
		method_0(1, layer4, activeOnStart: true);
		GClass54 layer5 = new GClass54(owner, 0);
		method_0(2, layer5, activeOnStart: true);
	}

	public override string ShortName()
	{
		return "BossBoar";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, 76);
	}
}
