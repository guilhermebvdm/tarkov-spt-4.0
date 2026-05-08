using System;
using EFT;
using UnityEngine;

public class GClass363 : GClass356
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
	public const int Int_5 = 12;

	[NonSerialized]
	public GClass106 Gclass106_0;

	[NonSerialized]
	public GClass135 Gclass135_0;

	public GClass363(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass77 layer3 = new GClass77(owner, 60, 35, 20);
		method_0(1, layer3, activeOnStart: true);
		Class99 layer4 = new Class99(owner, 47, withSearch: false);
		method_0(3, layer4, activeOnStart: true);
		Gclass135_0 = new GClass135(owner, 11, tryFinGreenFirst: true);
		method_0(2, Gclass135_0, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}

	public override void SetCorePosition(Vector3 pos)
	{
		Gclass135_0.SetCorePosition(pos);
	}

	public override string ShortName()
	{
		return "PrizrakSt";
	}

	public void SetBoss(GClass447 bossSectantPriestCrossSanitar)
	{
	}
}
