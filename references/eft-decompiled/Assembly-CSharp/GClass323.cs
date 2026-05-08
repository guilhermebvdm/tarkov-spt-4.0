using System;
using EFT;

public class GClass323 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 2;

	public GClass323(BotOwner owner)
		: base(owner)
	{
		GClass135 layer = new GClass135(owner, 11, tryFinGreenFirst: true, CoverLevel.Sit);
		method_0(2, layer, activeOnStart: true);
	}

	public override string ShortName()
	{
		return "InfectedAssault";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}
}
