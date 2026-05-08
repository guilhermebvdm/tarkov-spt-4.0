using System;
using EFT;
using EFT.InventoryLogic;

public class FirearmsAnimator : ObjectInHandsAnimator
{
	public enum EGrenadeFire
	{
		Idle,
		Hold,
		Throw
	}

	public const string JAM_STATE_NAME = "JAM";

	public const string FIRE_STATE_NAME = "FIRE";

	public const string SEMIFIRE_STATE_NAME = "SA FIRE";

	public const string DOUBLE_ACTION_FIRE_STATE_NAME = "DOUBLE_ACTION_FIRE";

	public const string MISFIRE_STATE_NAME = "MISFIRE";

	public const string SOFT_SLIDE_STATE_NAME = "SOFT_SLIDE";

	public const string HARD_SLIDE_STATE_NAME = "HARD_SLIDE";

	public const string FEED_STATE_NAME = "FEED";

	public const string IDLE_STATE_NAME = "IDLE";

	public const string IDLE_PLANT_STATE_NAME = "IDLE PLANT";

	public const string IDLE_UNDERBARREL_NAME = "IDLE WEAPON";

	public const string SPAWN_STATE_NAME = "SPAWN";

	public const string PATROL_STATE_NAME = "PATROL";

	public const string DRY_FIRE_STATE_NAME = "DRY FIRE";

	public const string DRY_FIRE_DISARMED_STATE_NAME = "Hands.DRY FIRE DISARMED";

	public const int HANDS_LAYER_INDEX = 1;

	public const string HANDS_LAYER_NAME = "Hands";

	public const string BOLT_CATCH_LAYER_NAME = "Catch";

	public const string HAMMER_LAYER_NAME = "Hammer";

	public const string ADDITIONAL_HANDS_LAYER_NAME = "Additional_Hands";

	public const string STOCK_LAYER = "Stock";

	public const string MALFUNCTION_LAYER = "Malfunction";

	public const string LHANDS_LAYER = "LActions";

	public int ADDITIONAL_HANDS_LAYER_INDEX;

	public int BOLT_CATCH_LAYER_INDEX;

	public int HAMMER_LAYER_INDEX;

	public int STOCK_LAYER_INDEX;

	public int MALFUNCTION_LAYER_INDEX;

	public new int LACTIONS_LAYER_INDEX;

	[field: NonSerialized]
	public string FullFireStateName { get; set; }

	[field: NonSerialized]
	public string FullSemiFireStateName { get; set; }

	[field: NonSerialized]
	public string FullDoubleActionFireStateName { get; set; }

	[field: NonSerialized]
	public string FullIdleStateName { get; set; }

	public event Action<bool> SetPatronInWeaponVisibleEvent;

	public override void SetAnimatorGetter(Func<IAnimator> getter)
	{
		base.SetAnimatorGetter(getter);
		BOLT_CATCH_LAYER_INDEX = base.Animator.GetLayerIndex("Catch");
		HAMMER_LAYER_INDEX = base.Animator.GetLayerIndex("Hammer");
		ADDITIONAL_HANDS_LAYER_INDEX = base.Animator.GetLayerIndex("Additional_Hands");
		LACTIONS_LAYER_INDEX = base.Animator.GetLayerIndex("LActions");
		STOCK_LAYER_INDEX = base.Animator.GetLayerIndex("Stock");
		MALFUNCTION_LAYER_INDEX = base.Animator.GetLayerIndex("Malfunction");
		LACTIONS_LAYER_INDEX = base.Animator.GetLayerIndex("LActions");
		FullFireStateName = base.Animator.GetLayerName(1) + ".FIRE";
		FullSemiFireStateName = base.Animator.GetLayerName(1) + ".SA FIRE";
		FullDoubleActionFireStateName = base.Animator.GetLayerName(1) + ".DOUBLE_ACTION_FIRE";
		FullIdleStateName = base.Animator.GetLayerName(1) + ".IDLE";
	}

	public void Reload(int currentMagType, int nextMagType, bool isFast)
	{
		SetMagTypeCurrent(currentMagType);
		SetMagTypeNew(nextMagType);
		if (isFast)
		{
			ReloadFast(b: true);
		}
		else
		{
			Reload(b: true);
		}
	}

	public bool IsIdling()
	{
		if (!CurrentStateNameIs(1, "IDLE") && !CurrentStateNameIs(1, FullIdleStateName) && !CurrentStateNameIs(1, "IDLE WEAPON"))
		{
			return CurrentStateNameIs(1, "IDLE PLANT");
		}
		return true;
	}

	public bool IsHandsProcessing()
	{
		return WeaponAnimationSpeedControllerClass.GetBoolUseLeftHand(base.Animator);
	}

	public void SetSpeedParameters(float reload = 1f, float draw = 1f)
	{
		WeaponAnimationSpeedControllerClass.SetSpeedReload(base.Animator, reload);
		WeaponAnimationSpeedControllerClass.SetSpeedDraw(base.Animator, draw);
	}

	public void SetMalfRepairSpeed(float fix = 1f)
	{
		WeaponAnimationSpeedControllerClass.SetSpeedFix(base.Animator, fix);
	}

	public void Idle()
	{
		WeaponAnimationSpeedControllerClass.TriggerIdleTime(base.Animator);
	}

	public void SetIsExternalMag(bool isExternalMag)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_ISEXTERNALMAG))
		{
			WeaponAnimationSpeedControllerClass.SetIsExternalMag(base.Animator, isExternalMag);
		}
	}

	public void SetMagFull(bool b)
	{
		WeaponAnimationSpeedControllerClass.SetMagFull(base.Animator, b);
	}

	public void SetFire(bool fire)
	{
		WeaponAnimationSpeedControllerClass.SetFire(base.Animator, fire);
		ResetLeftHand();
	}

	public void SetSprint(bool sprint)
	{
		WeaponAnimationSpeedControllerClass.SetSprint(base.Animator, sprint);
	}

	public void SetGrenadeFire(EGrenadeFire fire)
	{
		WeaponAnimationSpeedControllerClass.SetGrenadeFire(base.Animator, (int)fire);
		ResetLeftHand();
	}

	public void SetAlternativeFire(bool fire)
	{
		WeaponAnimationSpeedControllerClass.SetAltFire(base.Animator, fire);
		ResetLeftHand();
	}

	public void SetGrenadeAltFire(EGrenadeFire fire)
	{
		WeaponAnimationSpeedControllerClass.SetGrenadeAltFire(base.Animator, (int)fire);
		ResetLeftHand();
	}

	public void SetMagInWeapon(bool ok)
	{
		WeaponAnimationSpeedControllerClass.SetMagInWeapon(base.Animator, ok);
		WeaponAnimationSpeedControllerClass.SetMagInWeaponFloat(base.Animator, ok);
	}

	public void SetBoltActionReload(bool boltActionReload)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_BOLTACTIONRELOAD))
		{
			WeaponAnimationSpeedControllerClass.SetBoltActionReload(base.Animator, boltActionReload);
		}
	}

	public void SetBoltCatch(bool active)
	{
		if (base.Animator != null)
		{
			if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_BOLTCATCH))
			{
				WeaponAnimationSpeedControllerClass.SetBoltCatch(base.Animator, active);
			}
			if (HasParameter(WeaponAnimationSpeedControllerClass.FLOAT_IDLEPOSITION))
			{
				WeaponAnimationSpeedControllerClass.SetIdlePosition(base.Animator, active ? 1 : 0);
			}
			if (BOLT_CATCH_LAYER_INDEX >= 0)
			{
				base.Animator.SetLayerWeight(BOLT_CATCH_LAYER_INDEX, active ? 1 : 0);
			}
		}
	}

	public bool GetBoltCatch()
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_BOLTCATCH))
		{
			return WeaponAnimationSpeedControllerClass.GetBoolBoltCatch(base.Animator);
		}
		return false;
	}

	public void SetCanReload(bool canReload)
	{
		WeaponAnimationSpeedControllerClass.SetCanReload(base.Animator, canReload);
	}

	public void SetAimAngle(float playerPitch)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.FLOAT_AIM_ANGLE))
		{
			WeaponAnimationSpeedControllerClass.SetAim_angle(base.Animator, playerPitch);
		}
	}

	public void SetAiming(bool isAiming)
	{
		float aimingFloat = (isAiming ? 1f : 0f);
		WeaponAnimationSpeedControllerClass.SetAimingFloat(base.Animator, aimingFloat);
	}

	public void SetAimingFloat(float aiming)
	{
		WeaponAnimationSpeedControllerClass.SetAimingFloat(base.Animator, aiming);
	}

	public void SetAimingIn(bool aimingIn)
	{
		if (aimingIn)
		{
			WeaponAnimationSpeedControllerClass.SetAimingInTrigger(base.Animator);
		}
		else
		{
			WeaponAnimationSpeedControllerClass.ResetAimingInTrigger(base.Animator);
		}
	}

	public void SetAimingOut(bool aimingOut)
	{
		if (aimingOut)
		{
			WeaponAnimationSpeedControllerClass.SetAimingOutTrigger(base.Animator);
		}
		else
		{
			WeaponAnimationSpeedControllerClass.ResetAimingOutTrigger(base.Animator);
		}
	}

	public void SetPrevAimingFloat(bool isAiming)
	{
		WeaponAnimationSpeedControllerClass.SetPrevAimingFloat(base.Animator, isAiming ? 1f : 0f);
	}

	public void Malfunction(int val)
	{
		WeaponAnimationSpeedControllerClass.SetMalfunction(base.Animator, val);
		if (val != -1)
		{
			WeaponAnimationSpeedControllerClass.SetMalfunctionType(base.Animator, val);
		}
	}

	public void MalfunctionRepair(bool val)
	{
		WeaponAnimationSpeedControllerClass.SetMalfunctionRepair(base.Animator, val);
	}

	public void MisfireSlideUnknown(bool val)
	{
		WeaponAnimationSpeedControllerClass.SetMisfireSlideUnknown(base.Animator, val);
	}

	public void Rechamber(bool val)
	{
		WeaponAnimationSpeedControllerClass.SetRechamber(base.Animator, val);
	}

	public void SetAmmoOnMag(int count)
	{
		WeaponAnimationSpeedControllerClass.SetAmmoInMag(base.Animator, count);
	}

	public void SetMasteringReloadAborted(bool value)
	{
		WeaponAnimationSpeedControllerClass.SetMasteringReloadAborted(base.Animator, value);
	}

	public void SetAmmoCountForRemove(int count)
	{
		WeaponAnimationSpeedControllerClass.SetAmmoCountForRemove(base.Animator, count);
	}

	public void SetCamoraIndex(int camoraIndex)
	{
		WeaponAnimationSpeedControllerClass.SetCamoraIndex(base.Animator, camoraIndex);
	}

	public void SetCamoraFireIndex(int camoraFireIndex)
	{
		WeaponAnimationSpeedControllerClass.SetCamoraFireIndex(base.Animator, camoraFireIndex);
	}

	public void SetDoubleAction(float doubleAction)
	{
		WeaponAnimationSpeedControllerClass.SetDoubleAction(base.Animator, doubleAction);
	}

	public void SetCamoraIndexForLoadAmmo(int camoraIndex)
	{
		WeaponAnimationSpeedControllerClass.SetCamoraIndexForLoadAmmo(base.Animator, camoraIndex);
	}

	public void SetCamoraIndexWithShellForRemove(int camoraIdnex)
	{
		WeaponAnimationSpeedControllerClass.SetCamoraIndexWithShellForRemove(base.Animator, camoraIdnex);
	}

	public void SetCamoraIndexForUnloadAmmo(int camoraIndex)
	{
		WeaponAnimationSpeedControllerClass.SetCamoraIndexForUnloadAmmo(base.Animator, camoraIndex);
	}

	public void SetAmmoInChamber(float count)
	{
		WeaponAnimationSpeedControllerClass.SetAmmoInChamber(base.Animator, count);
	}

	public void SetShellsInWeapon(int count)
	{
		WeaponAnimationSpeedControllerClass.SetShellsInWeapon(base.Animator, count);
	}

	public void SetUnderbarrelSightingRange(float value)
	{
		WeaponAnimationSpeedControllerClass.SetUnderbarrelSightingRange(base.Animator, value);
	}

	public int GetSightingRange()
	{
		return WeaponAnimationSpeedControllerClass.GetSightingRange(base.Animator);
	}

	public void SetFireMode(Weapon.EFireMode fireMode, bool skipAnimation = false)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetFireMode(base.Animator, (int)fireMode);
		if (!skipAnimation)
		{
			WeaponAnimationSpeedControllerClass.TriggerFiremodeSwitch(base.Animator);
		}
	}

	public void TriggerFiremodeCheck()
	{
		WeaponAnimationSpeedControllerClass.TriggerFiremodeCheck(base.Animator);
	}

	public void SetMagTypeCurrent(int magType)
	{
		WeaponAnimationSpeedControllerClass.SetRelTypeOld(base.Animator, magType);
	}

	public void SetUniqueAnimationModId(int id)
	{
		WeaponAnimationSpeedControllerClass.SetModAnimationUniqueId(base.Animator, id);
	}

	public void SetMagTypeNew(int magType)
	{
		WeaponAnimationSpeedControllerClass.SetRelTypeNew(base.Animator, magType);
	}

	public void SetWeaponLevel(float weaponLevel)
	{
		WeaponAnimationSpeedControllerClass.SetWeaponLevel(base.Animator, weaponLevel);
	}

	public void SetPickup(bool p)
	{
	}

	public void SetInteract(bool p, int actionIndex)
	{
	}

	public void WatchClock(EPlayerSide playerSide)
	{
		if (playerSide != EPlayerSide.Savage)
		{
			WeaponAnimationSpeedControllerClass.SetUseLeftHand(base.Animator, useLeftHand: true);
			WeaponAnimationSpeedControllerClass.SetLActionIndex(base.Animator, (playerSide == EPlayerSide.Bear) ? 301 : 302);
		}
	}

	public void Fold(bool folded)
	{
		if (STOCK_LAYER_INDEX >= 0 && STOCK_LAYER_INDEX < AnimatorLayersCount)
		{
			base.Animator.SetLayerWeight(STOCK_LAYER_INDEX, folded ? 1 : 0);
		}
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_STOCKFOLDED))
		{
			WeaponAnimationSpeedControllerClass.SetStockFolded(base.Animator, folded);
		}
	}

	public void SetAmmoCompatible(bool compatible)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_INCOMPATIBLEAMMO))
		{
			WeaponAnimationSpeedControllerClass.SetIncompatibleAmmo(base.Animator, !compatible);
		}
	}

	public void SetAnimationVariant(int animationVariant)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.INT_ANIMATIONVARIANT))
		{
			WeaponAnimationSpeedControllerClass.SetAnimationVariant(base.Animator, animationVariant);
		}
	}

	public void SetNextLimb(bool value)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.BOOL_NEXT_LIMB))
		{
			WeaponAnimationSpeedControllerClass.SetNextLimb(base.Animator, value);
		}
	}

	public bool HasNextLimb()
	{
		return HasParameter(WeaponAnimationSpeedControllerClass.BOOL_NEXT_LIMB);
	}

	public void SetUseTimeMultiplier(float speed)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.FLOAT_USETIMEMULTIPLIER))
		{
			WeaponAnimationSpeedControllerClass.SetUseTimeMultiplier(base.Animator, speed);
		}
	}

	public void SetLooting(bool b)
	{
		if (b)
		{
			WeaponAnimationSpeedControllerClass.SetUseLeftHand(base.Animator, useLeftHand: true);
		}
	}

	public void ReloadFast(bool b)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.TriggerReloadFast(base.Animator);
	}

	public void RollToZeroCamora(bool roll)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetRollToZeroCamora(base.Animator, roll);
	}

	public void SetRollCylinder(bool roll)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetRollCylinder(base.Animator, roll);
	}

	public void LoadOneTrigger(bool loadOne)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetLoadOne(base.Animator, loadOne);
	}

	public void HammerArmed()
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.TriggerHammerArmed(base.Animator);
	}

	public void SetChamberIndexForLoadUnloadAmmo(float chamberIndex)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetChamberIndexForLoadAmmo(base.Animator, chamberIndex);
	}

	public void SetChamberIndexWithShell(float chamberIndex)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.SetChamberIndexWithShell(base.Animator, chamberIndex);
	}

	public void Reload(bool b)
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.TriggerReload(base.Animator);
	}

	public void ResetReload()
	{
		ResetLeftHand();
		WeaponAnimationSpeedControllerClass.ResetTriggerReload(base.Animator);
	}

	public void SetButterFingers()
	{
	}

	public void TriggerFold()
	{
		WeaponAnimationSpeedControllerClass.TriggerStockSwitch(base.Animator);
	}

	public void LookTrigger()
	{
		WeaponAnimationSpeedControllerClass.TriggerLook(base.Animator);
	}

	public void InsertMagInInventoryMode()
	{
		WeaponAnimationSpeedControllerClass.TriggerMagInFromInv(base.Animator);
	}

	public void ResetInsertMagInInventoryMode()
	{
		WeaponAnimationSpeedControllerClass.ResetTriggerMagInFromInv(base.Animator);
	}

	public void ResetGestureTrigger()
	{
		WeaponAnimationSpeedControllerClass.ResetTriggerGesture(base.Animator);
	}

	public void ResetHandReadyTrigger()
	{
		WeaponAnimationSpeedControllerClass.ResetTriggerHandReady(base.Animator);
	}

	public void PullOutMagInInventoryMode()
	{
		WeaponAnimationSpeedControllerClass.TriggerMagOutFromInv(base.Animator);
	}

	public void SetupMod(bool modSet)
	{
		WeaponAnimationSpeedControllerClass.SetModSet(base.Animator, modSet);
	}

	public void CheckAmmo()
	{
		WeaponAnimationSpeedControllerClass.TriggerCheckAmmo(base.Animator);
	}

	public void CheckChamber()
	{
		WeaponAnimationSpeedControllerClass.TriggerCheckChamber(base.Animator);
	}

	public void ResetCheckChamberTrigger()
	{
		WeaponAnimationSpeedControllerClass.ResetTriggerCheckChamber(base.Animator);
	}

	public void SetLauncher(bool isLauncherEnabled)
	{
		WeaponAnimationSpeedControllerClass.SetLauncherReady(base.Animator, isLauncherEnabled);
		SetLauncherId(-1);
	}

	public void SetLauncherId(int launcherId)
	{
		WeaponAnimationSpeedControllerClass.SetLauncherID(base.Animator, launcherId);
	}

	public void Discharge(bool discharge)
	{
		WeaponAnimationSpeedControllerClass.SetDischarge(base.Animator, discharge);
	}

	public void ModToggleTrigger()
	{
		WeaponAnimationSpeedControllerClass.TriggerModSwitch(base.Animator);
	}

	public void SetBipod(bool open)
	{
		WeaponAnimationSpeedControllerClass.SetBipod(base.Animator, open);
	}

	public void SetMounted(bool mounted)
	{
		WeaponAnimationSpeedControllerClass.SetMounted(base.Animator, mounted);
	}

	public void SetHammerArmed(bool isTriggerArmed)
	{
		WeaponAnimationSpeedControllerClass.SetArmed(base.Animator, isTriggerArmed);
		if (HAMMER_LAYER_INDEX >= 0)
		{
			base.Animator.SetLayerWeight(HAMMER_LAYER_INDEX, (!isTriggerArmed) ? 1 : 0);
		}
	}

	public void SetPatronInWeaponVisible(bool visible)
	{
		if (this.SetPatronInWeaponVisibleEvent != null)
		{
			this.SetPatronInWeaponVisibleEvent(visible);
		}
	}
}
