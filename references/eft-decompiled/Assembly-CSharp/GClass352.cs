using System;
using EFT;
using UnityEngine;

public class GClass352 : BaseBrain
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
	public const int Int_4 = 6;

	[NonSerialized]
	public const int Int_5 = 11;

	[NonSerialized]
	public const int Int_6 = 12;

	public GClass352(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass166 layer2 = new GClass166(owner, 75);
		method_0(11, layer2, activeOnStart: true);
		GClass96 layer3 = new GClass96(owner, 15);
		method_0(3, layer3, activeOnStart: true);
		GClass62 layer4 = new GClass62(owner, 10);
		method_0(1, layer4, activeOnStart: true);
		GClass97 layer5 = new GClass97(owner, 0);
		method_0(2, layer5, activeOnStart: true);
		Owner.BotsGroup.OnMemberRemove += method_6;
		AIPlaceInfoHalloweenCircle targetCircle = Owner.BotsGroup.BotGame.BotsController.EventsController.BotHalloweenEvent.TargetCircle;
		if (!(targetCircle == null))
		{
			Transform transform = GClass856.RandomElement(targetCircle.CastPoints);
			Owner.Mover.Teleport(transform.position);
		}
	}

	public void method_6(BotOwner obj)
	{
		Owner.BotsController.EventsController.BotHalloweenEvent.FirstContact();
	}

	public override void Dispose()
	{
		Owner.BotsGroup.OnMemberRemove -= method_6;
		base.Dispose();
	}

	public override string ShortName()
	{
		return "Prst event";
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}
}
