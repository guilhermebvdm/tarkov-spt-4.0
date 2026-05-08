using System;
using EFT;
using UnityEngine;

public class BossKojaniyBrainClass : GClass356
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
	public const int Int_4 = 8;

	[NonSerialized]
	public const int Int_5 = 11;

	[NonSerialized]
	public const int Int_6 = 12;

	[NonSerialized]
	public GClass105 Gclass105_0;

	[NonSerialized]
	public GClass106 Gclass106_0;

	[NonSerialized]
	public GClass135 Gclass135_0;

	public bool IAmReady => Gclass105_0.IAmReady;

	public BossKojaniyBrainClass(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 75);
		method_0(11, layer3, activeOnStart: true);
		Gclass105_0 = new GClass105(owner, 60);
		method_0(1, Gclass105_0, activeOnStart: true);
		GClass39 layer4 = new GClass39(owner, 50);
		method_0(8, layer4, activeOnStart: true);
		Gclass135_0 = new GClass135(owner, 11, tryFinGreenFirst: true, CoverLevel.Sit);
		method_0(2, Gclass135_0, activeOnStart: true);
		Gclass106_0 = new GClass106(owner, 40);
		method_0(3, Gclass106_0, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public void SetBossKojaniy(GInterface6 bossKojaniy)
	{
		Gclass105_0.SetBossKojaniy(bossKojaniy);
	}

	public override void SetCorePosition(Vector3 pos)
	{
		Gclass105_0.SetCorePosition(pos);
		Gclass135_0.SetCorePosition(pos);
	}

	public override string ShortName()
	{
		return "FollowerKojaniy";
	}
}
