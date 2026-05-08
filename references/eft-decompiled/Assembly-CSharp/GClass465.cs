using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;

public class GClass465 : GClass429
{
	[NonSerialized]
	public BotGlobalShootData BotGlobalShootData_0;

	[NonSerialized]
	public GClass615 Gclass615_0;

	[NonSerialized]
	public WeaponAIPreset WeaponAIPreset_0;

	[NonSerialized]
	public Dictionary<EWeaponAIPresetType, WeaponAIPreset> Dictionary_0 = new Dictionary<EWeaponAIPresetType, WeaponAIPreset>();

	public WeaponAIPreset WeaponAIPreset
	{
		get
		{
			return WeaponAIPreset_0;
		}
		set
		{
			WeaponAIPreset_0 = value;
		}
	}

	public GClass465(BotOwner owner)
		: base(owner)
	{
	}

	public void PreActivate()
	{
		BotGlobalAimingSettings aiming = BotOwner_0.Settings.FileSettings.Aiming;
		BotGlobalShootData_0 = BotOwner_0.Settings.FileSettings.Shoot;
		Gclass615_0 = BotOwner_0.Settings.Current;
		Dictionary_0.Add(EWeaponAIPresetType.MainWeaponAuto, new WeaponAIPreset(aiming.BASE_SHIEF, aiming.XZ_COEF, method_6, method_1, EWeaponAIPresetType.MainWeaponAuto));
		Dictionary_0.Add(EWeaponAIPresetType.MainWeaponSingle, new WeaponAIPreset(aiming.BASE_SHIEF, aiming.XZ_COEF, method_0, method_1, EWeaponAIPresetType.MainWeaponSingle));
		Dictionary_0.Add(EWeaponAIPresetType.StationaryBullet, new WeaponAIPreset(aiming.BASE_SHIEF_STATIONARY_BULLET, aiming.XZ_COEF_STATIONARY_BULLET, method_4, method_2, EWeaponAIPresetType.StationaryBullet));
		Dictionary_0.Add(EWeaponAIPresetType.StationaryGrenade, new WeaponAIPreset(aiming.BASE_SHIEF_STATIONARY_GRENADE, aiming.XZ_COEF_STATIONARY_GRENADE, method_5, method_3, EWeaponAIPresetType.StationaryGrenade));
		WeaponAIPreset = Dictionary_0[EWeaponAIPresetType.MainWeaponSingle];
	}

	public void UpdateFirearmsController(StationaryWeapon stationaryWeapon, bool isNowAutomatic)
	{
		if (stationaryWeapon != null)
		{
			if (stationaryWeapon.Animation == StationaryWeapon.EStationaryAnimationType.AGS_17)
			{
				WeaponAIPreset = Dictionary_0[EWeaponAIPresetType.StationaryGrenade];
			}
			else
			{
				WeaponAIPreset = Dictionary_0[EWeaponAIPresetType.StationaryBullet];
			}
		}
		else if (isNowAutomatic)
		{
			WeaponAIPreset = Dictionary_0[EWeaponAIPresetType.MainWeaponAuto];
		}
		else
		{
			WeaponAIPreset = Dictionary_0[EWeaponAIPresetType.MainWeaponSingle];
		}
	}

	public float method_0()
	{
		return Gclass615_0.CurrentHoldDownSingleShot;
	}

	public float method_1()
	{
		return BotGlobalShootData_0.WAIT_NEXT_SINGLE_SHOT;
	}

	public float method_2()
	{
		return BotGlobalShootData_0.WAIT_NEXT_STATIONARY_BULLET;
	}

	public float method_3()
	{
		return BotGlobalShootData_0.WAIT_NEXT_STATIONARY_GRENADE;
	}

	public float method_4()
	{
		return Gclass615_0.CurrentHoldDownStationaryBullet;
	}

	public float method_5()
	{
		return Gclass615_0.CurrentHoldDownStationaryGrenade;
	}

	public float method_6()
	{
		return Gclass615_0.CurrentHoldDownAutoFire * GClass856.Random(0.7f, 1.3f);
	}
}
