using System;
using EFT;
using UnityEngine;

public class GClass279 : GClass278
{
	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public bool Bool_1;

	public GClass279(BotOwner bot)
		: base(bot)
	{
	}

	public override void Look()
	{
		GClass595 gClass = BotOwner_0.BotRequestController.CurRequest as GClass595;
		if (Float_12 < Time.time)
		{
			Float_12 = Time.time + 1f;
			PlaceForCheck placeForCheck = BotOwner_0.BotsGroup.YoungestFastPlace(BotOwner_0, 20f, 3f);
			Bool_1 = placeForCheck != null;
		}
		if (gClass != null && gClass.Direction.HasValue && !Bool_1)
		{
			BotOwner_0.Steering.LookToDirection(gClass.Direction.Value);
		}
		else
		{
			base.Look();
		}
	}
}
