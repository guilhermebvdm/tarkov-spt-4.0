using System;
using EFT;
using UnityEngine;

public class BotFlashGrenade : GClass429
{
	[NonSerialized]
	public float FlashEndTimne;

	[NonSerialized]
	public float ShootInFlashEndTime;

	[NonSerialized]
	public bool IsFlashed_1;

	[NonSerialized]
	public bool ShallRun;

	[NonSerialized]
	public Vector3? LastTargetToShoot;

	[NonSerialized]
	public BotLastBlindEffectModifierClass LastBlindEffectModif;

	[NonSerialized]
	public BotLastBlindEffectModifierClass LastBlindEffectModif2;

	[NonSerialized]
	public BotLastBlindEffectModifierClass LastSoundEffectModif;

	public bool IsFlashed
	{
		get
		{
			if (!IsFlashed_1)
			{
				return false;
			}
			IsFlashed_1 = FlashEndTimne > Time.time;
			if (!IsFlashed_1)
			{
				method_0();
			}
			return IsFlashed_1;
		}
	}

	public Vector3? PlaceToShoot
	{
		get
		{
			if (LastTargetToShoot.HasValue)
			{
				return LastTargetToShoot;
			}
			if (BotOwner_0.Memory.GoalTarget.HavePlaceTarget())
			{
				return BotOwner_0.Memory.GoalTarget.GoalTarget.Position + Vector3.up * 1.6f;
			}
			return null;
		}
	}

	public BotFlashGrenade(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		ShallRun = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Grenade.CHANCE_RUN_FLASHED_100);
	}

	public bool ShallShoot()
	{
		if (ShallRun)
		{
			return false;
		}
		if (!IsFlashed)
		{
			return false;
		}
		if (ShootInFlashEndTime > Time.time)
		{
			return false;
		}
		Vector3? placeToShoot = PlaceToShoot;
		if (!placeToShoot.HasValue)
		{
			return false;
		}
		if ((BotOwner_0.Position - placeToShoot.Value).sqrMagnitude < BotOwner_0.Settings.FileSettings.Grenade.MAX_FLASHED_DIST_TO_SHOOT_SQRT)
		{
			if (Physics.Linecast(BotOwner_0.WeaponRoot.position, placeToShoot.Value, out var _, LayerMaskClass.HighPolyWithTerrainMaskAI))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void AddSoundEffect(float time, Vector3 position)
	{
		BotOwner_0.BotsGroup.AddPointToSearch(position, 140f, BotOwner_0);
		if (LastSoundEffectModif != null)
		{
			BotOwner_0.Settings.Current.Dismiss(LastSoundEffectModif);
		}
		LastSoundEffectModif = new BotLastBlindEffectModifierClass(1f, 1f, 1f, 1f, 1f, 1f, BotOwner_0.Settings.FileSettings.Change.STUN_HEARING, 1f, 1f);
		BotOwner_0.Settings.Current.Apply(LastSoundEffectModif, time);
	}

	public void AddBlindEffect(float time, Vector3 position)
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			BotOwner_0.Memory.GoalEnemy.SetVisible(value: false);
			LastTargetToShoot = BotOwner_0.Memory.GoalEnemy.GetPartToShoot();
		}
		else
		{
			LastTargetToShoot = null;
		}
		BotOwner_0.BotsGroup.AddPointToSearch(position, 140f, BotOwner_0);
		if (BotOwner_0.NightVision.UsingNow)
		{
			time *= BotOwner_0.Settings.FileSettings.Grenade.FLASH_MODIF_IS_NIGHTVISION;
		}
		float duractionSec = time;
		float duractionSec2 = time * 0.7f;
		if (IsFlashed_1)
		{
			method_0();
		}
		else
		{
			LastBlindEffectModif = method_3();
			LastBlindEffectModif2 = method_3();
			BotOwner_0.Settings.Current.Apply(LastBlindEffectModif, duractionSec);
			BotOwner_0.Settings.Current.Apply(LastBlindEffectModif2, duractionSec2);
		}
		FlashEndTimne = time + Time.time;
		ShootInFlashEndTime = Time.time + BotOwner_0.Settings.FileSettings.Grenade.TIME_SHOOT_TO_FLASH;
		IsFlashed_1 = true;
	}

	public void method_0()
	{
		if (LastBlindEffectModif != null)
		{
			BotOwner_0.Settings.Current.Dismiss(LastBlindEffectModif);
			LastBlindEffectModif = null;
		}
		if (LastBlindEffectModif2 != null)
		{
			BotOwner_0.Settings.Current.Dismiss(LastBlindEffectModif2);
			LastBlindEffectModif2 = null;
		}
	}

	public void method_1(IEffect obj)
	{
	}

	public void method_2(IEffect obj)
	{
	}

	public BotLastBlindEffectModifierClass method_3()
	{
		return new BotLastBlindEffectModifierClass(BotOwner_0.Settings.FileSettings.Change.FLASH_PRECICING, BotOwner_0.Settings.FileSettings.Change.FLASH_ACCURATY, BotOwner_0.Settings.FileSettings.Change.FLASH_LAY_CHANCE, BotOwner_0.Settings.FileSettings.Change.FLASH_VISION_DIST, BotOwner_0.Settings.FileSettings.Change.FLASH_GAIN_SIGHT, BotOwner_0.Settings.FileSettings.Change.FLASH_SCATTERING, BotOwner_0.Settings.FileSettings.Change.FLASH_HEARING, BotOwner_0.Settings.FileSettings.Change.FLASH_SCATTERING, 1f);
	}

	public void Dispose()
	{
	}
}
