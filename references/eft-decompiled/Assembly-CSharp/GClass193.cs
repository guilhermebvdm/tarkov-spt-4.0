using System;
using EFT;
using UnityEngine;

public class GClass193 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public BotHalloweenEvent BotHalloweenEvent_0 => BotOwner_0.BotsController.EventsController.BotHalloweenEvent;

	public GClass193(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		BotOwner_0.SetPose(1f);
		if (!BotOwner_0.WeaponManager.IsMelee && BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
		{
			BotOwner_0.WeaponManager.Selector.ChangeToMelee();
		}
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 2f;
			BotOwner_0.GetPlayer.SetAnimatorLayerWeight(17, 1);
			BotHalloweenEvent_0.UpdateFromBoss();
		}
	}
}
