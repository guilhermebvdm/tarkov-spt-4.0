using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotUnderbarrelLauncherController : GClass429
{
	public enum ELauncherShootAttemptStatus
	{
		Unsuccessful,
		Successful,
		UnsuccessfulWithDisable
	}

	public enum ESwitchReason
	{
		UseInsteadOfReload,
		ForceSwitchInFight,
		PeaceReload
	}

	public struct Struct11(EHysteresisState lastHysteresisState, bool lastEvaluation)
	{
		public EHysteresisState LastHysteresisState = lastHysteresisState;

		public bool LastEvaluation = lastEvaluation;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class201
	{
		public static readonly Class201 class201_0 = new Class201();

		public static Func<NavGraphVoxelSimple, ushort> func_0;

		public ushort method_0(NavGraphVoxelSimple voxel)
		{
			return voxel.Id;
		}
	}

	[CompilerGenerated]
	public class Class202
	{
		public BotUnderbarrelLauncherController botUnderbarrelLauncherController_0;

		public Action finishCallback;

		public Callback callback_0;

		public void method_0()
		{
			botUnderbarrelLauncherController_0.TryReload(delegate
			{
				botUnderbarrelLauncherController_0.TryDisable(finishCallback);
			});
		}

		public void method_1(IResult _)
		{
			botUnderbarrelLauncherController_0.TryDisable(finishCallback);
		}
	}

	[CompilerGenerated]
	public class Class203
	{
		public BotUnderbarrelLauncherController botUnderbarrelLauncherController_0;

		public Action operationFinishedCallback;

		public void method_0()
		{
			botUnderbarrelLauncherController_0.LastEnableHandsTime = Time.time;
			botUnderbarrelLauncherController_0.ToggleOperationFinished = true;
			botUnderbarrelLauncherController_0.BotOwner_0.AimingManager.SetAimingType(EAimingType.UnderbarrelLauncher);
			operationFinishedCallback?.Invoke();
		}
	}

	[CompilerGenerated]
	public class Class204
	{
		public BotUnderbarrelLauncherController botUnderbarrelLauncherController_0;

		public Action operationFinishedCallback;

		public void method_0()
		{
			botUnderbarrelLauncherController_0.ToggleOperationFinished = true;
			botUnderbarrelLauncherController_0.BotOwner_0.AimingManager.SetAimingType(EAimingType.CommonAiming);
			operationFinishedCallback?.Invoke();
		}
	}

	[CompilerGenerated]
	public class Class205
	{
		public LauncherItemClass launcher;

		public bool method_0(AmmoItemClass ammo)
		{
			if (GClass3124.CanAccept(launcher.Chamber, ammo))
			{
				return ammo.CheckAction(null).Succeeded;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class206
	{
		public BotUnderbarrelLauncherController botUnderbarrelLauncherController_0;

		public EnemyInfo goalEnemy;

		public Func<KeyValuePair<IPlayer, EnemyInfo>, bool> func_0;

		public bool method_0(KeyValuePair<IPlayer, EnemyInfo> enemyInfo)
		{
			if (botUnderbarrelLauncherController_0.NearGoalEnemyVoxelsIdsSet.Contains(botUnderbarrelLauncherController_0.BotOwner_0.CoverSearchInfo.GetVoxelSafe(enemyInfo.Key.Position).Id))
			{
				return enemyInfo.Key.Id != goalEnemy.Person.Id;
			}
			return false;
		}
	}

	[NonSerialized]
	public static EquipmentSlot[] AvailableEquipmentSlots = new EquipmentSlot[4]
	{
		EquipmentSlot.Pockets,
		EquipmentSlot.TacticalVest,
		EquipmentSlot.Backpack,
		EquipmentSlot.SecuredContainer
	};

	[NonSerialized]
	public IList<AmmoItemClass> FoundAmmosBuffer = new List<AmmoItemClass>();

	[NonSerialized]
	public Dictionary<int, Struct11> GoalToOtherEnemiesHysteresisMap = new Dictionary<int, Struct11>();

	[NonSerialized]
	public HashSet<ushort> NearGoalEnemyVoxelsIdsSet = new HashSet<ushort>();

	[NonSerialized]
	public float LastEnableTime;

	[NonSerialized]
	public int SuccessShootAttemptsAfterSwitch;

	[NonSerialized]
	public float NearTargetDistanceTresholdSqr;

	[NonSerialized]
	public float DeviationSqr;

	[NonSerialized]
	public float TimeToCheckSwitchToGrenadeLauncher;

	[NonSerialized]
	public ESwitchReason LastSwitchReason;

	[NonSerialized]
	public bool ToggleOperationFinished = true;

	[NonSerialized]
	public float LastEnableHandsTime;

	[NonSerialized]
	public float LastShootAttemptCheckTime;

	[NonSerialized]
	public int CanUseInsteadOfReloadLastFrame;

	[NonSerialized]
	public bool CanUseInsteadOfReloadLastFrameResult;

	[NonSerialized]
	public const int TIME_FOR_UNSUCCESSFUL_ATTEMPTS = 3;

	[NonSerialized]
	public const float SHOOT_ATTEMT_CHECK_COOLDOWN = 0.25f;

	public bool IsActive => BotOwner_0.WeaponManager.CurrentWeapon.IsUnderBarrelDeviceActive;

	public BotUnderbarrelLauncherController(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		TimeToCheckSwitchToGrenadeLauncher = Time.time + smethod_3(BotOwner_0);
		NearTargetDistanceTresholdSqr = BotOwner_0.Settings.FileSettings.Shoot.DISTANCE_TO_TARGET_NEAR_ENEMY_TRESHOLD * BotOwner_0.Settings.FileSettings.Shoot.DISTANCE_TO_TARGET_NEAR_ENEMY_TRESHOLD;
		DeviationSqr = NearTargetDistanceTresholdSqr - Mathf.Pow(BotOwner_0.Settings.FileSettings.Shoot.DISTANCE_TO_TARGET_NEAR_ENEMY_TRESHOLD - BotOwner_0.Settings.FileSettings.Shoot.DISTANCE_TO_TARGET_NEAR_ENEMY_DEVIATION, 2f);
		method_1();
		BotOwner_0.Memory.OnGoalEnemyChanged += method_2;
	}

	public void ManualUpdate()
	{
	}

	public void TryEnableReloadDisable(Action finishCallback = null)
	{
		LastSwitchReason = ESwitchReason.PeaceReload;
		TryEnable(delegate
		{
			TryReload(delegate
			{
				TryDisable(finishCallback);
			});
		});
	}

	public bool CanUseInsteadOfReload()
	{
		if (IsActive)
		{
			return false;
		}
		if (CanUseInsteadOfReloadLastFrame == Time.frameCount)
		{
			return CanUseInsteadOfReloadLastFrameResult;
		}
		CanUseInsteadOfReloadLastFrame = Time.frameCount;
		if (!TryGetUnderbarrelWeapon(out var underbarrelWeapon))
		{
			CanUseInsteadOfReloadLastFrameResult = false;
			return CanUseInsteadOfReloadLastFrameResult;
		}
		if (!HasAmmoInChamber(underbarrelWeapon))
		{
			CanUseInsteadOfReloadLastFrameResult = false;
			return CanUseInsteadOfReloadLastFrameResult;
		}
		float distanceToEnemy = smethod_2(BotOwner_0, underbarrelWeapon);
		if (!smethod_1(BotOwner_0, distanceToEnemy))
		{
			CanUseInsteadOfReloadLastFrameResult = false;
			return CanUseInsteadOfReloadLastFrameResult;
		}
		if (!BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			CanUseInsteadOfReloadLastFrameResult = false;
			return CanUseInsteadOfReloadLastFrameResult;
		}
		LastSwitchReason = ESwitchReason.UseInsteadOfReload;
		CanUseInsteadOfReloadLastFrameResult = true;
		return CanUseInsteadOfReloadLastFrameResult;
	}

	public bool CheckShootAttemptAndDisableIfNeeded()
	{
		if (Time.time - LastShootAttemptCheckTime < 0.25f)
		{
			return false;
		}
		LastShootAttemptCheckTime = Time.time;
		if (SuccessShootAttemptsAfterSwitch == 0 && !ToggleOperationFinished)
		{
			return false;
		}
		switch (CheckShootAttemptGdRules())
		{
		default:
			return false;
		case ELauncherShootAttemptStatus.Unsuccessful:
			if (Time.time - LastEnableHandsTime >= 3f)
			{
				TryDisable();
			}
			return false;
		case ELauncherShootAttemptStatus.Successful:
			SuccessShootAttemptsAfterSwitch++;
			return true;
		case ELauncherShootAttemptStatus.UnsuccessfulWithDisable:
			TryDisable();
			return false;
		}
	}

	public ELauncherShootAttemptStatus CheckShootAttemptGdRules()
	{
		if (!TryGetUnderbarrelWeapon(out var underbarrelWeapon))
		{
			return ELauncherShootAttemptStatus.UnsuccessfulWithDisable;
		}
		if (SuccessShootAttemptsAfterSwitch >= BotOwner_0.Settings.FileSettings.Shoot.MAX_SUCCESS_GRENADE_LAUNCHER_SHOOT_ATTEMPTS)
		{
			return ELauncherShootAttemptStatus.UnsuccessfulWithDisable;
		}
		if (SuccessShootAttemptsAfterSwitch == 0 && !GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Shoot.SHOOT_PROBABILITY_GRENADE_LAUNCHER))
		{
			return ELauncherShootAttemptStatus.Unsuccessful;
		}
		if (!HasAmmoInChamber(underbarrelWeapon))
		{
			if (AmmoQuery(underbarrelWeapon).Count > 0)
			{
				return ELauncherShootAttemptStatus.Unsuccessful;
			}
			return ELauncherShootAttemptStatus.UnsuccessfulWithDisable;
		}
		float distanceToEnemy = smethod_2(BotOwner_0, underbarrelWeapon);
		if (!smethod_1(BotOwner_0, distanceToEnemy))
		{
			return ELauncherShootAttemptStatus.Unsuccessful;
		}
		if (BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			ESwitchReason lastSwitchReason = LastSwitchReason;
			if (lastSwitchReason != ESwitchReason.UseInsteadOfReload && lastSwitchReason != ESwitchReason.PeaceReload)
			{
				if (method_0())
				{
					return ELauncherShootAttemptStatus.Successful;
				}
				return ELauncherShootAttemptStatus.Unsuccessful;
			}
			return ELauncherShootAttemptStatus.Successful;
		}
		if (Time.time - BotOwner_0.Memory.GoalEnemy.PersonalLastSeenTime > BotOwner_0.Settings.FileSettings.Shoot.MAX_TIME_SEEN_TO_SWITCH_TO_GRENADE_LAUNCHER)
		{
			return ELauncherShootAttemptStatus.Unsuccessful;
		}
		float lastBodyHitDistance = BotOwner_0.Memory.GoalEnemy.GetLastBodyHitDistance();
		if (lastBodyHitDistance > 0f && lastBodyHitDistance < 1f)
		{
			return ELauncherShootAttemptStatus.Unsuccessful;
		}
		return ELauncherShootAttemptStatus.Successful;
	}

	public bool TryEnable(Action operationFinishedCallback = null)
	{
		if (TryGetUnderbarrelWeapon(out var _) && ToggleOperationFinished)
		{
			if (Time.time - LastEnableTime < BotOwner_0.Settings.FileSettings.Shoot.SWITCH_TO_UNDERBARREL_WEAPON_COOLDOWN)
			{
				return false;
			}
			ToggleOperationFinished = false;
			bool num = !BotOwner_0.WeaponManager.ShootController.IsInLauncherMode() & BotOwner_0.WeaponManager.ShootController.ToggleLauncher(delegate
			{
				LastEnableHandsTime = Time.time;
				ToggleOperationFinished = true;
				BotOwner_0.AimingManager.SetAimingType(EAimingType.UnderbarrelLauncher);
				operationFinishedCallback?.Invoke();
			});
			if (num)
			{
				LastEnableTime = Time.time;
				SuccessShootAttemptsAfterSwitch = 0;
				return num;
			}
			ToggleOperationFinished = true;
			return num;
		}
		return false;
	}

	public bool TryDisable(Action operationFinishedCallback = null)
	{
		if (!ToggleOperationFinished)
		{
			return false;
		}
		ToggleOperationFinished = false;
		bool num = BotOwner_0.WeaponManager.ShootController.IsInLauncherMode();
		bool flag = false;
		if (num)
		{
			flag = BotOwner_0.WeaponManager.ShootController.ToggleLauncher(delegate
			{
				ToggleOperationFinished = true;
				BotOwner_0.AimingManager.SetAimingType(EAimingType.CommonAiming);
				operationFinishedCallback?.Invoke();
			});
		}
		bool num2 = num && flag;
		if (!num2)
		{
			ToggleOperationFinished = true;
		}
		return num2;
	}

	public bool TryReload(Callback reloadFinishedCallback = null)
	{
		if (!BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryGetUnderbarrelWeapon(out var underbarrelWeapon))
		{
			return false;
		}
		IList<AmmoItemClass> list = AmmoQuery(underbarrelWeapon);
		if (list.Count != 0 && BotOwner_0.WeaponManager.ShootController.CanStartReload())
		{
			BotOwner_0.WeaponManager.ShootController.ReloadGrenadeLauncher(new AmmoPackReloadingClass(list), reloadFinishedCallback);
			return true;
		}
		BotOwner_0.WeaponManager.ShootController.ReloadMagNotFound();
		return false;
	}

	public bool TryGetUnderbarrelWeapon(out LauncherItemClass underbarrelWeapon)
	{
		underbarrelWeapon = BotOwner_0.WeaponManager.CurrentWeapon.GetUnderbarrelWeapon();
		return underbarrelWeapon?.IsUnderbarrelWeapon ?? false;
	}

	public IList<AmmoItemClass> AmmoQuery(LauncherItemClass launcher)
	{
		FoundAmmosBuffer.Clear();
		BotOwner_0.GetPlayer.InventoryController.GetAcceptableItemsNonAlloc(AvailableEquipmentSlots, FoundAmmosBuffer, (AmmoItemClass ammo) => GClass3124.CanAccept(launcher.Chamber, ammo) && ammo.CheckAction(null).Succeeded);
		return FoundAmmosBuffer;
	}

	public bool NeedToReload()
	{
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryGetUnderbarrelWeapon(out var underbarrelWeapon) && !HasAmmoInChamber(underbarrelWeapon))
		{
			return AmmoQuery(underbarrelWeapon).Count > 0;
		}
		return false;
	}

	public static bool HasAmmoInChamber(LauncherItemClass underbarrelWeapon)
	{
		return underbarrelWeapon.ChamberAmmoCount != 0;
	}

	public bool CanSwitchInFight(BotOwner owner)
	{
		if (!IsActive && TimeToCheckSwitchToGrenadeLauncher < Time.time)
		{
			TimeToCheckSwitchToGrenadeLauncher = Time.time + smethod_3(owner);
			if (!TryGetUnderbarrelWeapon(out var underbarrelWeapon))
			{
				return false;
			}
			if (!HasAmmoInChamber(underbarrelWeapon) && AmmoQuery(underbarrelWeapon).Count == 0)
			{
				return false;
			}
			float distanceToEnemy = smethod_2(owner, underbarrelWeapon);
			if (!smethod_1(BotOwner_0, distanceToEnemy))
			{
				return false;
			}
			if (owner.Memory.GoalEnemy.IsVisible)
			{
				if (method_0())
				{
					return true;
				}
				return false;
			}
			if (Time.time - owner.Memory.GoalEnemy.PersonalLastSeenTime > owner.Settings.FileSettings.Shoot.MAX_TIME_SEEN_TO_SWITCH_TO_GRENADE_LAUNCHER)
			{
				return false;
			}
			method_1();
			LastSwitchReason = ESwitchReason.ForceSwitchInFight;
			return true;
		}
		return false;
	}

	public bool method_0()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Vector3 currPosition = goalEnemy.CurrPosition;
		NearGoalEnemyVoxelsIdsSet.Clear();
		foreach (ushort item in from voxel in goalEnemy.Owner.VoxelesPersonalData.GetVoxelesExtended(currPosition, 1, onlyWithPoints: false)
			select voxel.Id)
		{
			NearGoalEnemyVoxelsIdsSet.Add(item);
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> item2 in BotOwner_0.EnemiesController.EnemyInfos.Where((KeyValuePair<IPlayer, EnemyInfo> enemyInfo) => NearGoalEnemyVoxelsIdsSet.Contains(BotOwner_0.CoverSearchInfo.GetVoxelSafe(enemyInfo.Key.Position).Id) && enemyInfo.Key.Id != goalEnemy.Person.Id))
		{
			int id = item2.Key.Id;
			EHysteresisState eHysteresisState = GClass855.Hysteresis(GClass856.SqrDistance(currPosition, item2.Value.CurrPosition), NearTargetDistanceTresholdSqr, DeviationSqr);
			bool flag;
			if (GoalToOtherEnemiesHysteresisMap.TryGetValue(id, out var value))
			{
				flag = smethod_0(value.LastHysteresisState, value.LastEvaluation, eHysteresisState);
				value.LastHysteresisState = eHysteresisState;
				value.LastEvaluation = flag;
			}
			else
			{
				flag = smethod_0(EHysteresisState.LessThanTreshold, lastEvaluation: false, eHysteresisState);
				GoalToOtherEnemiesHysteresisMap.Add(id, new Struct11(eHysteresisState, flag));
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void method_1()
	{
		LastEnableTime = Time.time - BotOwner_0.Settings.FileSettings.Shoot.SWITCH_TO_UNDERBARREL_WEAPON_COOLDOWN - 1f;
	}

	public void method_2(BotOwner obj)
	{
		GoalToOtherEnemiesHysteresisMap.Clear();
	}

	public static bool smethod_0(EHysteresisState lastState, bool lastEvaluation, EHysteresisState currentState)
	{
		if (lastState < currentState)
		{
			switch (currentState)
			{
			case EHysteresisState.BetweenThresholds:
			case EHysteresisState.LessThanTreshold:
				return true;
			case EHysteresisState.AboveTreshold:
				return false;
			}
		}
		else if (lastState > currentState)
		{
			switch (currentState)
			{
			case EHysteresisState.LessThanTreshold:
				return true;
			case EHysteresisState.AboveTreshold:
			case EHysteresisState.BetweenThresholds:
				return false;
			}
		}
		return lastEvaluation;
	}

	public static bool smethod_1(BotOwner owner, float distanceToEnemy)
	{
		if (owner.Memory.HaveEnemy)
		{
			return owner.Memory.GoalEnemy.Distance >= distanceToEnemy;
		}
		return false;
	}

	public static float smethod_2(BotOwner owner, LauncherItemClass launcher)
	{
		float lOW_DIST_K_FOR_GRENADE_LAUNCHER = owner.Settings.FileSettings.Shoot.LOW_DIST_K_FOR_GRENADE_LAUNCHER;
		owner.GetPlayer.Id.ToString();
		Slot[] chambers = launcher.Chambers;
		int num = 0;
		AmmoItemClass ammoItemClass;
		while (true)
		{
			if (num < chambers.Length)
			{
				ammoItemClass = chambers[num].ContainedItem as AmmoItemClass;
				if (ammoItemClass != null)
				{
					break;
				}
				num++;
				continue;
			}
			return owner.Settings.FileSettings.Shoot.DEFAULT_LOW_DIST_TO_USE_GRENADE_LAUNCHER;
		}
		AmmoTemplate ammoTemplate = ammoItemClass.AmmoTemplate;
		return lOW_DIST_K_FOR_GRENADE_LAUNCHER * ammoTemplate.InitialSpeed * ammoTemplate.FuzeArmTimeSec;
	}

	public static float smethod_3(BotOwner owner)
	{
		return GClass856.Random(owner.Settings.FileSettings.Shoot.MIN_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER, owner.Settings.FileSettings.Shoot.MAX_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER);
	}
}
