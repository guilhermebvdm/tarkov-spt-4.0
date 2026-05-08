using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class BotWeaponManager : GClass429
{
	[NonSerialized]
	public GClass465 PresetCollection;

	[NonSerialized]
	public Dictionary<EquipmentSlot, BotWeaponInfo> Info = new Dictionary<EquipmentSlot, BotWeaponInfo>();

	[NonSerialized]
	public BotWeaponInfo CurrentWeaponInfo;

	[NonSerialized]
	public GClass25 PeriodCheckAutoFire;

	[NonSerialized]
	public const float BLOCK_AUTO_FIRE_PERIOD = 60f;

	[NonSerialized]
	public bool CanBeAutomatic;

	[NonSerialized]
	public bool IsScatteringFixed;

	public float CriticalBulletCount;

	[NonSerialized]
	public float BlockAutoFireUntil;

	[NonSerialized]
	public bool Disposed;

	public WeaponAIPreset WeaponAIPreset => PresetCollection.WeaponAIPreset;

	[field: NonSerialized]
	public BotWeaponSelector Selector { get; set; }

	[field: NonSerialized]
	public BotMeleeWeaponData Melee { get; set; }

	[field: NonSerialized]
	public BotStationaryWeaponData Stationary { get; set; }

	[field: NonSerialized]
	public BotGrenadeController Grenades { get; set; }

	public float Single_0 => BotOwner_0.Settings.FileSettings.Shoot.DITANCE_TO_OFF_AUTO_FIRE;

	public float Single_1 => BotOwner_0.Settings.FileSettings.Shoot.DITANCE_TO_ON_AUTO_FIRE;

	[field: NonSerialized]
	public BotUnderbarrelLauncherController UnderbarrelLauncherController { get; }

	[field: NonSerialized]
	public BotMalfunctionData Malfunctions { get; set; }

	public bool IsMelee => Melee.MeleeWeaponEquipped;

	[field: NonSerialized]
	public IFirearmHandsController ShootController { get; set; }

	public BotReload Reload => CurrentWeaponInfo.Reload;

	public BotWeaponInfo PistolWeaponInfo
	{
		get
		{
			if (Info.TryGetValue(EquipmentSlot.Holster, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public BotWeaponInfo MainWeaponInfo
	{
		get
		{
			if (Info.TryGetValue(EquipmentSlot.FirstPrimaryWeapon, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public BotWeaponInfo SecondWeaponInfo
	{
		get
		{
			if (Info.TryGetValue(EquipmentSlot.SecondPrimaryWeapon, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public BotWeaponInfo MeleeWeaponInfo
	{
		get
		{
			if (Info.TryGetValue(EquipmentSlot.Scabbard, out var value))
			{
				return value;
			}
			return null;
		}
	}

	[field: NonSerialized]
	public bool IsReady { get; set; }

	public bool HaveBullets => (float)CurrentWeaponInfo.Reload.BulletCount > CriticalBulletCount;

	public Weapon CurrentWeapon
	{
		get
		{
			if (ShootController != null && ShootController.Item != null)
			{
				return ShootController.Item;
			}
			return null;
		}
	}

	public bool IsNowAutomatic => CurrentWeaponInfo.IsNowAutomatic;

	[field: NonSerialized]
	public float AmbushDistance { get; set; } = 100f;

	[field: NonSerialized]
	public bool IsCloseWeapon { get; set; }

	public bool IsWeaponReady => Selector.IsWeaponReady;

	public event Action<IFirearmHandsController> WeaponHandsChangedEvent;

	public void method_0(EquipmentSlot slot)
	{
		if (Info != null && Info.TryGetValue(slot, out var value))
		{
			CurrentWeaponInfo = value;
		}
	}

	public BotWeaponManager(BotOwner owner)
		: base(owner)
	{
		PeriodCheckAutoFire = new GClass25(3f, method_6);
		IsCloseWeapon = false;
		PresetCollection = new GClass465(owner);
		Melee = new BotMeleeWeaponData(owner);
		Stationary = new BotStationaryWeaponData(owner);
		Grenades = new BotGrenadeController(owner);
		Malfunctions = new BotMalfunctionData(owner);
		UnderbarrelLauncherController = new BotUnderbarrelLauncherController(owner);
		switch (owner.Profile.Info.Settings.Role)
		{
		default:
			Selector = new BotWeaponSelector(owner);
			break;
		case WildSpawnType.infectedAssault:
		case WildSpawnType.infectedPmc:
		case WildSpawnType.infectedCivil:
		case WildSpawnType.infectedLaborant:
			Selector = new GClass467(owner);
			break;
		case WildSpawnType.followerBigPipe:
			Selector = new GClass466(owner);
			break;
		}
		BotWeaponSelector selector = Selector;
		selector.OnActiveEquipmentSlotChanged = (Action<EquipmentSlot>)Delegate.Combine(selector.OnActiveEquipmentSlotChanged, new Action<EquipmentSlot>(method_0));
	}

	public virtual void Activate()
	{
		Melee.Activate();
		Selector.Activate();
		Stationary.Activate();
		Grenades.Activate();
		UnderbarrelLauncherController.Activate();
		BotOwner_0.GetPlayer.GetPlayer.BeingHitAction += method_2;
		BotOwner_0.Memory.OnBulletNear += method_1;
	}

	public void method_1(BotOwner arg1, IPlayer source)
	{
		if (source != null)
		{
			method_3(source.Position);
		}
	}

	public void method_2(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (arg1.Player != null)
		{
			method_3(arg1.Player.iPlayer.Position);
		}
	}

	public void method_3(Vector3 from)
	{
		if (!(Single_0 <= 0.1f) && (from - BotOwner_0.Position).sqrMagnitude > Single_0 * Single_0)
		{
			BlockAutoFireUntil = Time.time + 60f;
			CurrentWeaponInfo.ChangeFireMode(Weapon.EFireMode.single);
		}
	}

	public void PreActivate()
	{
		PresetCollection.PreActivate();
		BotOwner_0.GetPlayer.HandsController.FastForwardCurrentState();
		Selector.TakeMainWeapon();
	}

	public void StationaryTaken(IFirearmHandsController fireArms, StationaryWeapon stationaryWeapon)
	{
		method_4(fireArms, stationaryWeapon);
	}

	public virtual void ManualUpdate()
	{
		PeriodCheckAutoFire.Update();
		Selector.ManualUpdate();
		Grenades.UpdateCheck();
		Grenades.CheckPeriodTime();
		Reload.ManualUpdate();
		UnderbarrelLauncherController.ManualUpdate();
	}

	public void CheckCurMainWeapon()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		bool num = getPlayer.HandsController.Item != Selector.SecondPrimaryWeaponItem && getPlayer.HandsController.Item != Selector.FirstPrimaryWeaponItem;
		if (ShootController.Item != Selector.SecondPrimaryWeaponItem)
		{
			_ = ShootController.Item != Selector.FirstPrimaryWeaponItem;
		}
		else
			_ = 0;
		if (!num)
		{
			method_4((IFirearmHandsController)getPlayer.HandsController, null);
		}
	}

	public void UpdateWeaponsList()
	{
		Selector.UpdateWeaponsList();
	}

	public void CheckWeaponReady()
	{
		if (ShootController != null && ShootController.FirearmsAnimator != null && string.IsNullOrEmpty(ShootController.FirearmsAnimator.FullIdleStateName))
		{
			Selector.IsWeaponReady = true;
		}
	}

	public bool InIdleState()
	{
		if (ShootController.FirearmsAnimator == null)
		{
			return false;
		}
		return ShootController.FirearmsAnimator.Animator.GetCurrentAnimatorStateInfo(1).IsName(ShootController.FirearmsAnimator.FullIdleStateName);
	}

	public void UpdateHandsController(IHandsController handsController, out bool allFine)
	{
		allFine = false;
		if (handsController is IFirearmHandsController firearmHandsController)
		{
			BotOwner_0.WeaponManager.method_4(firearmHandsController, null);
			BotOwner_0.WeaponManager.Melee.UpdateKnifeController(null);
			allFine = true;
		}
		else if (handsController is IKnifeController knifeController)
		{
			if (!Info.ContainsKey(EquipmentSlot.Scabbard))
			{
				CurrentWeaponInfo = new BotWeaponInfo(BotOwner_0, knifeController.Item as Weapon, EquipmentSlot.Scabbard, method_5);
				Info.Add(EquipmentSlot.Scabbard, CurrentWeaponInfo);
			}
			BotOwner_0.WeaponManager.Melee.UpdateKnifeController(knifeController);
			allFine = true;
		}
	}

	public void method_4(IFirearmHandsController firearmHandsController, [CanBeNull] StationaryWeapon stationaryWeapon)
	{
		if (Disposed)
		{
			return;
		}
		if (ShootController != null)
		{
			ShootController.Item.OnMalfunctionValidate -= Malfunctions.ValidateMalfunction;
		}
		ShootController = firearmHandsController;
		ShootController.Item.OnMalfunctionValidate += Malfunctions.ValidateMalfunction;
		if (BotOwner_0 == null)
		{
			return;
		}
		BotOwner_0.LookSensor?.Init();
		IsReady = true;
		if (ShootController == null)
		{
			return;
		}
		Weapon item = ShootController.Item;
		if (item != null)
		{
			IsCloseWeapon = item is PistolItemClass || item is ShotgunItemClass || item is RevolverItemClass;
			AmbushDistance = (IsCloseWeapon ? BotOwner_0.Settings.FileSettings.Mind.PISTOL_SHOTGUN_AMBUSH_DIST : BotOwner_0.Settings.FileSettings.Mind.STANDART_AMBUSH_DIST);
			Selector.IsWeaponReady = true;
			bool flag = false;
			if (BotOwner_0.Settings.FileSettings.Mind.CAN_USE_LONG_COVER_POINTS)
			{
				flag = item is AssaultRifleItemClass || item is ShotgunItemClass || item is MarksmanRifleItemClass || item is SniperRifleItemClass;
			}
			if (flag)
			{
				BotOwner_0.Covers.SetLong();
			}
			else
			{
				BotOwner_0.Covers.SetClose();
			}
			BotOwner_0.AimingManager.SetWeapon(item);
			BotOwner_0.BotPersonalStats.SetWeapon(item);
			if (!Info.ContainsKey(Selector.EquipmentSlot))
			{
				CurrentWeaponInfo = new BotWeaponInfo(BotOwner_0, item, Selector.EquipmentSlot, method_5);
				Info.Add(Selector.EquipmentSlot, CurrentWeaponInfo);
			}
			BotOwner_0.BotLight.FindLight();
			PresetCollection.UpdateFirearmsController(stationaryWeapon, IsNowAutomatic);
		}
		BotOwner_0.AIData.CalcPower();
		this.WeaponHandsChangedEvent?.Invoke(ShootController);
	}

	public bool method_5(Weapon.EFireMode obj)
	{
		return ShootController.ChangeFireMode(obj);
	}

	public void method_6()
	{
		if (ShootController != null)
		{
			Weapon item = ShootController.Item;
			method_7(item);
		}
	}

	public void method_7(Weapon weapon)
	{
		if (weapon == null)
		{
			return;
		}
		CanBeAutomatic = weapon.WeapFireType.Contains(Weapon.EFireMode.fullauto);
		if (!CanBeAutomatic)
		{
			return;
		}
		bool flag = BlockAutoFireUntil < Time.time;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (flag)
		{
			if (Single_0 > 0f && goalEnemy != null && goalEnemy.Distance > Single_0)
			{
				flag = false;
			}
		}
		else if (Single_0 > 0f && goalEnemy != null && goalEnemy.Distance < Single_1)
		{
			flag = true;
		}
		if (flag)
		{
			if (!CurrentWeaponInfo.IsNowAutomatic && weapon.FireMode.FireMode != Weapon.EFireMode.fullauto && (float)GClass856.RandomInclude(0, 100) <= BotOwner_0.Settings.FileSettings.Shoot.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100)
			{
				CurrentWeaponInfo.ChangeFireMode(Weapon.EFireMode.fullauto);
				if (!IsScatteringFixed)
				{
					IsScatteringFixed = true;
					BotOwner_0.Settings.FileSettings.Core.ScatteringPerMeter *= BotOwner_0.Settings.FileSettings.Shoot.AUTOMATIC_FIRE_SCATTERING_COEF;
				}
			}
		}
		else if (CurrentWeaponInfo.IsNowAutomatic && weapon.FireMode.FireMode == Weapon.EFireMode.fullauto)
		{
			CurrentWeaponInfo.ChangeFireMode(Weapon.EFireMode.single);
			BlockAutoFireUntil = Time.time + 60f;
		}
	}

	public void Dispose()
	{
		BotOwner_0.GetPlayer.GetPlayer.BeingHitAction -= method_2;
		BotOwner_0.Memory.OnBulletNear -= method_1;
		Selector?.Dispose();
		Selector = null;
		Melee?.Dispose();
		Melee = null;
		Stationary?.Dispose();
		Stationary = null;
		Grenades?.Dispose();
		Grenades = null;
		Malfunctions?.Dispose();
		if (ShootController != null && ShootController.Item != null && Malfunctions != null)
		{
			ShootController.Item.OnMalfunctionValidate -= Malfunctions.ValidateMalfunction;
		}
		Malfunctions = null;
		Info?.Clear();
		Info = null;
		ShootController = null;
		Disposed = true;
	}

	public void TryReloadWeaponOrUnderbarrelLauncher()
	{
		if (UnderbarrelLauncherController.IsActive)
		{
			if (UnderbarrelLauncherController.NeedToReload())
			{
				UnderbarrelLauncherController.TryReload();
			}
		}
		else if (!HaveBullets)
		{
			Reload.TryReload();
		}
	}

	public bool CanChangeHands()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		if (!(getPlayer == null) && !(getPlayer.HandsController == null))
		{
			if (!getPlayer.HealthController.IsAlive)
			{
				return false;
			}
			if (getPlayer.InventoryController.IsChangingWeapon)
			{
				return false;
			}
			if (getPlayer.IsInventoryOpened)
			{
				return false;
			}
			if (!getPlayer.StateIsSuitableForHandInput)
			{
				return false;
			}
			if (getPlayer.HandsController.IsInInteractionStrictCheck())
			{
				return false;
			}
			if (!getPlayer.HandsController.CanRemove())
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
