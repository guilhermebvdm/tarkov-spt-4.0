using System;
using EFT;
using UnityEngine;

public class BotMagazineChecker : GClass429
{
	[NonSerialized]
	public float NextPosibleCheckMagazine;

	public BotMagazineChecker(BotOwner owner)
		: base(owner)
	{
	}

	public void ManualUpdate()
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.CAN_CHECK_MAGAZINE && NextPosibleCheckMagazine < Time.time)
		{
			NextPosibleCheckMagazine = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.CHECK_MAGAZIN_PERIOD);
			BotOwner_0.WeaponManager.ShootController.CheckAmmo();
		}
	}
}
