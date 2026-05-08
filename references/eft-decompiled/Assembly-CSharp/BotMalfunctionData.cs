using System;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotMalfunctionData : GClass429
{
	public enum EInnerMalfunctionState
	{
		None,
		UnknownMalfunction,
		KnownMalfunction,
		MalfunctionFixInProgress,
		MalfunctionFixed
	}

	[NonSerialized]
	public const float END_OPERATION_DURATION = 8.5f;

	[NonSerialized]
	public EInnerMalfunctionState MalfState;

	[NonSerialized]
	public float NextOperationTime;

	[NonSerialized]
	public bool MalfFixed;

	[NonSerialized]
	public bool IgnoreMalfunction;

	[NonSerialized]
	public bool PreFixed;

	[NonSerialized]
	public bool WeaponExamined;

	public BotMalfunctionData(BotOwner owner)
		: base(owner)
	{
	}

	public bool ValidateMalfunction(Weapon.EMalfunctionState malfState)
	{
		if (!IgnoreMalfunction && (BotOwner_0.Settings.FileSettings.Shoot.VALIDATE_MALFUNCTION_CHANCE <= 0 || BotOwner_0.Brain.BaseBrain.HaveLayer<GClass118>()))
		{
			if (!BotOwner_0.Brain.BaseBrain.IsLayerActive<GClass118>())
			{
				return false;
			}
			if (malfState != Weapon.EMalfunctionState.None && method_0() && !MalfFixed)
			{
				if (malfState == Weapon.EMalfunctionState.Jam)
				{
					BotOwner_0.BotTalk.Say(EPhraseTrigger.OnWeaponJammed, sayImmediately: true);
				}
				else
				{
					BotOwner_0.BotTalk.Say(EPhraseTrigger.WeaponBroken, sayImmediately: true);
				}
				MalfFixed = false;
				PreFixed = false;
				WeaponExamined = false;
				NextOperationTime = Time.time + BotOwner_0.Settings.FileSettings.Shoot.DELAY_BEFORE_EXAMINE_MALFUNCTION;
				return true;
			}
			return false;
		}
		IgnoreMalfunction = true;
		return false;
	}

	public bool method_0()
	{
		return GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Shoot.VALIDATE_MALFUNCTION_CHANCE);
	}

	public bool HaveMalfunction()
	{
		if (MalfFixed)
		{
			return true;
		}
		return BotOwner_0.WeaponManager.CurrentWeapon.MalfState.State != Weapon.EMalfunctionState.None;
	}

	public Weapon.EMalfunctionState MalfunctionType()
	{
		if (!(BotOwner_0 == null) && BotOwner_0.WeaponManager.CurrentWeapon != null && BotOwner_0.WeaponManager.CurrentWeapon.MalfState != null)
		{
			return BotOwner_0.WeaponManager.CurrentWeapon.MalfState.State;
		}
		return Weapon.EMalfunctionState.None;
	}

	public bool method_1()
	{
		bool num = BotOwner_0.WeaponManager.ShootController.FirearmsAnimator.CurrentStateNameIs(1, BotOwner_0.WeaponManager.ShootController.FirearmsAnimator.FullIdleStateName);
		bool flag = BotOwner_0.WeaponManager.ShootController.FirearmsAnimator.CurrentStateNameIs(1, "DRY FIRE");
		bool flag2 = BotOwner_0.WeaponManager.ShootController.FirearmsAnimator.CurrentStateNameIs(1, "Hands.DRY FIRE DISARMED");
		return num || flag || flag2;
	}

	public string GetCustomBotInfoData()
	{
		return (BotOwner_0.WeaponManager.CurrentWeapon.MalfState.IsKnownMalfType(BotOwner_0.GetPlayer.ProfileId) ? "1" : "0") + (method_1() ? "1" : "0") + (MalfFixed ? "1" : "0") + "." + (int)BotOwner_0.WeaponManager.CurrentWeapon.MalfState.State + "." + (int)Mathf.Floor(NextOperationTime) + ".";
	}

	public void Update()
	{
		BotOwner_0.StopMove();
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			BotOwner_0.WeaponManager.Reload.CheckReloadLongTime();
		}
		else
		{
			if (NextOperationTime > Time.time)
			{
				return;
			}
			if (PreFixed && Time.time > NextOperationTime && method_1())
			{
				MalfFixed = true;
			}
			if (MalfunctionType() == Weapon.EMalfunctionState.None)
			{
				return;
			}
			if (PreFixed && Time.time > NextOperationTime && method_1())
			{
				MalfFixed = MalfunctionType() == Weapon.EMalfunctionState.None;
				PreFixed = MalfFixed;
				WeaponExamined = MalfFixed;
			}
			bool flag = BotOwner_0.WeaponManager.CurrentWeapon.MalfState.IsKnownMalfType(BotOwner_0.GetPlayer.ProfileId) || WeaponExamined;
			if (Time.time > NextOperationTime && !flag && method_1())
			{
				BotOwner_0.WeaponManager.ShootController.SetAim(value: false);
				BotOwner_0.WeaponManager.ShootController.ExamineWeapon();
				WeaponExamined = true;
				NextOperationTime = Time.time + BotOwner_0.Settings.FileSettings.Shoot.DELAY_BEFORE_FIX_MALFUNCTION;
			}
			else if (!MalfFixed && Time.time > NextOperationTime && method_1())
			{
				if (MalfunctionType() == Weapon.EMalfunctionState.Misfire)
				{
					BotOwner_0.WeaponManager.ShootController.CheckChamber();
				}
				else
				{
					BotOwner_0.WeaponManager.ShootController.CheckChamber();
				}
				NextOperationTime = Time.time + 8.5f;
				PreFixed = true;
				WeaponExamined = false;
			}
		}
	}

	public void Dispose()
	{
	}
}
