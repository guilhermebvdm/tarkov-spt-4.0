using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using MagCheckInterrupt.Utils;

namespace MagCheckInterrupt.Components;

/// <summary>
/// Combines a remove and insert magazine operation
/// </summary>
public class SwapReloadOperation(FirearmController controller) : FirearmOperation(controller)
{
    private Slot _weaponMagazineSlot;
    private Callback _finishCallback;
    private AttachModResult _insertMagResult;

    private bool _isMagazineWithBelt;
    private bool _magPulledOutFromWeapon;
    private bool _magPuttedToRig;
    private bool _magAppeared;
    private bool _shellEjected;
    private bool _magInsertedToWeapon;
    private bool _addedAmmoInChamber;
    private bool _ammoRemovedFromChamber;

    /// <summary>
    /// Does both <see cref="RemoveModOperation"/> and <see cref="AttachModOperation"/>
    /// </summary>
    /// <param name="magazine">Removed magazine</param>
    /// <param name="from">Slot magazine was removed from</param>
    public virtual void Start(MagazineItemClass magazine, Slot from, Callback finishCallback)
    {
        LoggerUtil.Debug("SwapReloadOperation::Start");

        _weaponMagazineSlot = from;
        _finishCallback = finishCallback;
        _isMagazineWithBelt = magazine.IsMagazineWithBelt;
        base.Start();
        this.TransitionToReload(false, true);

        FirearmsAnimator_0.SetFire(false);
        FirearmsAnimator_0.SetIsExternalMag(true);
        FirearmsAnimator_0.SetCanReload(true); // True to proceed with the next magazine
        Player_0.MovementContext.SetBlindFire(0);
        Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
        _blockTriggerField(FirearmController_0) = true;

        if (Weapon_0.IsBeltMachineGun)
        {
            FirearmController_0.IsAiming = false;
        }

        if (!Weapon_0.MustBoltBeOpennedForExternalReload)
        {
            _shellEjected = true;
            _ammoRemovedFromChamber = true;
            if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
            {
                FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
            }
        }
        else if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
        {
            FirearmsAnimator_0.SetAmmoInChamber(1f);
            FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
        }

        // isReleased
        if (Weapon_0.IsBoltCatch
            && Weapon_0.ChamberAmmoCount == 1
            && !Weapon_0.ManualBoltCatch
            && !Weapon_0.MustBoltBeOpennedForExternalReload
            && !Weapon_0.MustBoltBeOpennedForInternalReload)
        {
            FirearmsAnimator_0.SetBoltCatch(false);
        }
    }

    public override void Reset()
    {
        LoggerUtil.Debug("SwapReloadOperation::Reset");

        _weaponMagazineSlot = null;
        _insertMagResult = null;
        _finishCallback = null;
        _isMagazineWithBelt = false;
        _magPulledOutFromWeapon = false;
        _magPuttedToRig = false;
        _magAppeared = false;
        _shellEjected = false;
        _magInsertedToWeapon = false;
        _addedAmmoInChamber = false;
        _ammoRemovedFromChamber = false;
        base.Reset();
    }

    public override void OnMagPulledOutFromWeapon()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnMagPulledOutFromWeapon");

        if (_magPulledOutFromWeapon) return;

        _magPulledOutFromWeapon = true;
        FirearmsAnimator_0.SetAmmoOnMag(0);
        FirearmsAnimator_0.SetMagInWeapon(false);
        if (FirearmController_0.HasBipod)
        {
            FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
        }
    }

    public override void OnMagPuttedToRig()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnMagPuttedToRig");

        if (_magPuttedToRig) return;

        _magPuttedToRig = true;
        WeaponManagerClass.RemoveMod(_weaponMagazineSlot);

        if (_isMagazineWithBelt)
        {
            _weaponPrefabField(FirearmController_0).UpdateAnimatorHierarchy();
            if (FirearmController_0.HasBipod)
            {
                FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
            }
        }

        // Run insert magazine
        var insertResult = AttachModResult.Run(Player_0.InventoryController, Weapon_0, Player_0.ProfileId);
        if (insertResult.Failed)
        {
            LoggerUtil.Error($"MagCheckReloadOperation::OnMagPuttedToRig Insert mag operation failed: {insertResult.Error}");
            FirearmsAnimator_0.SetCanReload(false);
            _finishCallback.Invoke(insertResult);

            State = EOperationState.Finished;
            FirearmController_0.InitiateOperation<IdlingOperation>().Start(null);
            return;
        }

        _insertMagResult = insertResult.Value;
    }

    public override void OnShellEjectEvent()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnShellEjectEvent");

        if (_shellEjected) return;

        _shellEjected = true;
        if (Weapon_0.MustBoltBeOpennedForExternalReload && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
        {
            WeaponManagerClass.CreatePatronInShellPort(Weapon_0.MalfState.MalfunctionedAmmo, 0);
            WeaponManagerClass.StartSpawnMisfiredCartridge(Player_0.Velocity * 0.66f);
            return;
        }

        foreach (var chamber in Weapon_0.Chambers)
        {
            if (chamber.ContainedItem is not AmmoItemClass { IsUsed: false } ammoItemClass) continue;

            WeaponManagerClass.MoveAmmoFromChamberToShellPort(ammoItemClass.IsUsed, 0);
            WeaponManagerClass.StartSpawnShell(Player_0.Velocity * 0.66f, 0);
            return;
        }

        LoggerUtil.Warning("SwapReloadOperation::OnShellEjectEvent No unused ammo found in chambers?");
    }

    public override void RemoveAmmoFromChamber()
    {
        LoggerUtil.Debug("SwapReloadOperation::RemoveAmmoFromChamber");

        if (_ammoRemovedFromChamber) return;

        _ammoRemovedFromChamber = true;
        if (Weapon_0.MustBoltBeOpennedForExternalReload && Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
        {
            method_2();
            FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
            return;
        }

        foreach (var slot in Weapon_0.Chambers)
        {
            if (slot.ContainedItem is AmmoItemClass { IsUsed: false } ammoInChamber && slot.RemoveItem(false).Succeeded)
            {
                // Below line is missing in GClass2050.RemoveAmmoFromChamber, but is in GClass2016.RemoveAmmoFromChamber
                WeaponManagerClass.ThrowPatronAsLoot(ammoInChamber, Player_0, "SwapReloadOperation.RemoveAmmoFromChamber");
                break;
            }
        }

        FirearmsAnimator_0.SetAmmoInChamber(Weapon_0.ChamberAmmoCount);
    }

    public override void OnMagAppeared()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnMagAppeared");

        if (_magAppeared) return;

        _magAppeared = true;
        InsertMagazine();
        WeaponManagerClass.SetupMod(
            _insertMagResult.MagazineSlot.Slot,
            Singleton<PoolManagerClass>.Instance.CreateItem(_insertMagResult.Magazine, true)
        );

        if (_insertMagResult.Magazine.IsMagazineWithBelt)
        {
            _weaponPrefabField(FirearmController_0).UpdateAnimatorHierarchy();
            if (FirearmController_0.HasBipod)
            {
                FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
            }
        }
    }

    public override void OnMagInsertedToWeapon()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnMagInsertedToWeapon");

        if (_magInsertedToWeapon) return;

        _magInsertedToWeapon = true;
        FirearmsAnimator_0.SetAmmoOnMag(_insertMagResult.MagazineAmmoCount + (_insertMagResult.HasNewAmmo ? 1 : 0));
        FirearmsAnimator_0.SetMagInWeapon(true);
        FirearmsAnimator_0.SetAmmoCompatible(_insertMagResult.AmmoCompatible);

        if (!_insertMagResult.HasNewAmmo
            && (Weapon_0.MalfState.State != Weapon.EMalfunctionState.Misfire
                || !Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId)
                || !_insertMagResult.AmmoCompatible
                || _insertMagResult.Magazine.Count <= 0))
        {
            EndOperation();
        }

        if (FirearmController_0.HasBipod)
        {
            FirearmController_0.FirearmsAnimator.SetBipod(FirearmController_0.BipodState);
        }
    }

    public override void OnOnOffBoltCatchEvent(bool isCaught)
    {
        LoggerUtil.Debug("SwapReloadOperation::OnOnOffBoltCatchEvent");
        FirearmsAnimator_0.SetBoltCatch(isCaught);
    }

    public override void OnAddAmmoInChamber()
    {
        LoggerUtil.Debug("SwapReloadOperation::OnAddAmmoInChamber");

        if (_addedAmmoInChamber) return;

        _addedAmmoInChamber = true;
        FirearmsAnimator_0.SetAmmoOnMag(_insertMagResult.Magazine.Count);
        if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
        {
            method_2();
        }
        if (_insertMagResult.HasNewAmmo)
        {
            WeaponManagerClass.SetRoundIntoWeapon(_insertMagResult.NewAmmo, 0);
        }
        FirearmsAnimator_0.SetAmmoInChamber(_insertMagResult.Weapon.ChamberAmmoCount);
        EndOperation();
    }

    public override void SetInventoryOpened(bool opened)
    {
        LoggerUtil.Debug("SwapReloadOperation::SetInventoryOpened");

        FirearmController_0.InventoryOpened = opened;
        FirearmsAnimator_0.SetInventory(opened);
    }

    public override void HideWeapon(Action onHidden, bool fastDrop, Item nextControllerItem = null)
    {
        LoggerUtil.Debug("SwapReloadOperation::HideWeapon");

        State = EOperationState.Finished;
        FirearmController_0.RecalculateErgonomic();
        FirearmController_0.IsTriggerPressed = false;
        FirearmController_0.IsAiming = false;
        State = EOperationState.Finished; // Idk why BSG set this twice
        FirearmController_0.InitiateOperation<RemoveWeaponOperation>().Start(onHidden, fastDrop, nextControllerItem);
    }

    public override bool CanChangeLightState(FirearmLightStateStruct[] lightsStates)
    {
        return false;
    }

    public override void FastForward()
    {
        LoggerUtil.Debug("SwapReloadOperation::FastForward");

        RemoveAmmoFromChamber();
        OnShellEjectEvent();
        OnMagPulledOutFromWeapon();
        OnMagPuttedToRig();
        OnMagAppeared();
        OnMagInsertedToWeapon();
        OnAddAmmoInChamber();
        if (State != EOperationState.Finished)
        {
            EndOperation();
        }
    }

    private void InsertMagazine()
    {
        LoggerUtil.Debug("SwapReloadOperation::InsertMagazine");

        FirearmsAnimator_0.SetMagTypeNew(_insertMagResult.Magazine.magAnimationIndex);
        FirearmsAnimator_0.SetMagTypeCurrent(_insertMagResult.Magazine.magAnimationIndex);

        // isReleased
        if (Weapon_0.IsBoltCatch
            && Weapon_0.ChamberAmmoCount == 1
            && !_insertMagResult.HasNewAmmo
            && !Weapon_0.ManualBoltCatch
            && !Weapon_0.MustBoltBeOpennedForExternalReload
            && !Weapon_0.MustBoltBeOpennedForInternalReload)
        {
            FirearmsAnimator_0.SetBoltCatch(false);
        }

        // isMalfunction
        if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire
            && Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId)
            && _insertMagResult.Magazine.Count > 0
            && _insertMagResult.AmmoCompatible)
        {
            FirearmsAnimator_0.SetAmmoInChamber(0f);
            FirearmsAnimator_0.SetLayerWeight(FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
        }
    }

    private void EndOperation()
    {
        LoggerUtil.Debug("SwapReloadOperation::EndOperation");

        State = EOperationState.Finished;
        FirearmController_0.RecalculateErgonomic();
        FirearmController_0.InitiateOperation<IdlingOperation>().Start(null);
        _finishCallback?.Succeed();
        FirearmController_0.WeaponModified();
    }

    #region REFLECTION
    private static readonly AccessTools.FieldRef<FirearmController, bool> _blockTriggerField =
        AccessTools.FieldRefAccess<FirearmController, bool>("bool_1");

    private static readonly AccessTools.FieldRef<FirearmController, WeaponPrefab> _weaponPrefabField =
        AccessTools.FieldRefAccess<FirearmController, WeaponPrefab>("weaponPrefab_0");
    #endregion
}
