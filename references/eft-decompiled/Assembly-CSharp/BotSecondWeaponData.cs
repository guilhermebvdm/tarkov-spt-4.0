using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotSecondWeaponData : GClass429
{
	[NonSerialized]
	public const float START_TIME_OFFSET = 90f;

	[NonSerialized]
	public const float DELAY_BEFORE_ACTION = 1f;

	[NonSerialized]
	public const float DELAY_ACTION_TIME = 2f;

	[NonSerialized]
	public float NextPossibleUseTime;

	[NonSerialized]
	public bool ShallStart;

	[NonSerialized]
	public float AmmoActionTime = float.MaxValue;

	[NonSerialized]
	public float ReturnToMainTime = float.MaxValue;

	[NonSerialized]
	public IPlayer LookTo;

	[NonSerialized]
	public bool CanUse;

	public bool HaveSecondWeapon
	{
		get
		{
			if (!(BotOwner_0 == null) && BotOwner_0.WeaponManager != null)
			{
				return BotOwner_0.WeaponManager.Selector.CanChangeToSecondWeapons;
			}
			return false;
		}
	}

	public BotSecondWeaponData(BotOwner owner)
		: base(owner)
	{
		CanUse = BotOwner_0.Settings.FileSettings.Patrol.CAN_WATCH_SECOND_WEAPON;
	}

	public void StartDo(GClass413 closest)
	{
		if (CanUse)
		{
			BotOwner_0.WeaponManager.UpdateWeaponsList();
			if (!(NextPossibleUseTime > Time.time) && HaveSecondWeapon)
			{
				ShallStart = true;
				LookTo = closest.GetAnother(BotOwner_0);
			}
		}
	}

	public bool HaveActions()
	{
		return ShallStart;
	}

	public void ManualUpdate()
	{
		if (!CanUse)
		{
			return;
		}
		if (AmmoActionTime < Time.time)
		{
			AmmoActionTime = float.MaxValue;
			if (GClass856.IsTrue100(50f))
			{
				BotOwner_0.WeaponManager.ShootController.CheckAmmo();
			}
			else
			{
				BotOwner_0.WeaponManager.ShootController.ExamineWeapon();
			}
			ReturnToMainTime = Time.time + 2f;
		}
		if (ReturnToMainTime < Time.time)
		{
			if (BotOwner_0.WeaponManager.Selector.ChangeToMain())
			{
				AmmoActionTime = float.MaxValue;
				ReturnToMainTime = float.MaxValue;
				ShallStart = false;
			}
			else
			{
				BotOwner_0.WeaponManager.CheckWeaponReady();
			}
		}
		if (LookTo != null && !(BotOwner_0 == null))
		{
			Vector3 point = LookTo.Position + BotOwner.STAY_HEIGHT;
			BotOwner_0.Steering.LookToPoint(point);
			BotOwner_0.StopMove();
			if (NextPossibleUseTime < Time.time)
			{
				Apply();
				method_0();
			}
		}
		else
		{
			AmmoActionTime = float.MaxValue;
			ReturnToMainTime = float.MaxValue;
			ShallStart = false;
		}
	}

	public void Activate()
	{
		NextPossibleUseTime = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.WATCH_SECOND_WEAPON_PERIOD) + 90f;
	}

	public void method_0()
	{
		NextPossibleUseTime = Time.time + GClass856.GreateRandom(BotOwner_0.Settings.FileSettings.Patrol.WATCH_SECOND_WEAPON_PERIOD);
	}

	public void Apply()
	{
		if (!CanUse)
		{
			return;
		}
		if (!HaveSecondWeapon)
		{
			ShallStart = false;
			return;
		}
		BotOwner_0.WeaponManager.Selector.ChangeToSecond(delegate(Result<IHandsController> x)
		{
			if (x.Succeed)
			{
				AmmoActionTime = Time.time + 1f;
			}
		});
	}

	[CompilerGenerated]
	public void method_1(Result<IHandsController> x)
	{
		if (x.Succeed)
		{
			AmmoActionTime = Time.time + 1f;
		}
	}
}
