using System;
using EFT;
using UnityEngine;

public class GClass234 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass234(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		bool flag = method_0() == DoorInteractionStatus.CanRun;
		BotOwner_0.BotLight.TurnOff();
		flag = flag && BotOwner_0.WeaponManager.Stationary.LastDistToWeapon > 3f;
		BotOwner_0.SetPose(1f);
		BotOwner_0.Sprint(flag);
		BotOwner_0.Steering.LookToMovingDirection();
		method_5(BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.OperatorPosition);
	}

	public void method_5(Vector3 weaponOperatorPosition)
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 5f;
			BotOwner_0.GoToPoint(weaponOperatorPosition);
		}
	}
}
