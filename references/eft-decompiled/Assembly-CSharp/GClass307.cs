using System;
using EFT;
using UnityEngine;

public class GClass307 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass307(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 8f;
			BotOwner_0.WeaponManager.Selector.TryChangeWeapon();
		}
	}
}
