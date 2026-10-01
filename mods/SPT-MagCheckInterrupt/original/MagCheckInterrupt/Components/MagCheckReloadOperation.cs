using Comfort.Common;
using EFT.InventoryLogic;
using MagCheckInterrupt.Patches;
using MagCheckInterrupt.Utils;
using UnityEngine;

namespace MagCheckInterrupt.Components;

public class MagCheckReloadOperation(FirearmController controller) : UtilityOperation(controller)
{
#if DEBUG
    private static PlayerStateDebug _playerStateDebug;
#endif

    private bool _ammoDetailsShown;
    private bool _reloadCalled;
    private RemoveOperation _swapRemoveOperation;

    // Slow down animation fields
    private float _currentSpeed = 1f;
    private float _targetSpeed = 1f;
    private SpeedState _animSpeedState = SpeedState.Normal;

#if DEBUG
    public new void Start(EUtilityType utilityType)
    {
        base.Start(utilityType);

        LoggerUtil.Debug("MagCheckReloadOperation::Start");
        if (_playerStateDebug == null)
        {
            _playerStateDebug = PlayerStateDebug.Create(Player_0);
        }
        else
        {
            _playerStateDebug.SetPlayer(Player_0);
        }
    }
#endif

    public override void Update(float deltaTime)
    {
        if (State != EOperationState.Ready) return;

        var normalizedTime = FirearmsAnimator_0.GetNormalizedTime(FirearmsAnimator.HANDS_LAYER_INDEX);
        if (!_ammoDetailsShown && normalizedTime > ConfigUtil.ReloadWindowStart.Value)
        {
            // Show ammo details at the start of the reload window
            if (Player_0.FirstPersonPointOfView)
            {
                AmmoDetailsPatch.ShowLastAmmoDetail();
            }

            _ammoDetailsShown = true;
        }

        if (!ConfigUtil.SlowAnimation.Value) return;

        switch (_animSpeedState)
        {
            case SpeedState.Normal:
                if (normalizedTime > ConfigUtil.SlowAnimationStart.Value)
                {
                    _targetSpeed = ConfigUtil.SlowPercentage.Value;
                    _animSpeedState = SpeedState.Slowed;
                }
                break;
            case SpeedState.Slowed:
                if (normalizedTime >= ConfigUtil.SlowAnimationEnd.Value)
                {
                    _targetSpeed = 1f;
                    _animSpeedState = SpeedState.Restored;
                }
                break;
            case SpeedState.Restored:
            default:
                break;
        }

        // Smoothing
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, ConfigUtil.SlowSmoothing.Value * deltaTime);
        FirearmsAnimator_0.SetAnimationSpeed(_currentSpeed); // Set every frame okay?
    }

    public override void Reset()
    {
        _ammoDetailsShown = false;
        _reloadCalled = false;
        _swapRemoveOperation = null;
        _animSpeedState = SpeedState.Normal;
        _currentSpeed = 1f;
        _targetSpeed = 1f;
        Bool_0 = false;
        Bool_1 = false;
        base.Reset();
    }

    public override void OnUtilityOperationStartEvent()
    {
        base.OnUtilityOperationStartEvent();

        if (!Player_0.FirstPersonPointOfView || ConfigUtil.ReloadMode.Value != KeybindsUtil.EReloadMode.Press) return;

        if (KeybindsUtil.AreCheckAndReloadKeysConflicting())
        {
            ReloadConflictPatch.SkipReload();
        }
    }

    public override bool CanStartReload()
    {
        if (State != EOperationState.Ready) return false;

        var normalizedTime = FirearmsAnimator_0.GetNormalizedTime(FirearmsAnimator.HANDS_LAYER_INDEX);
        return normalizedTime > ConfigUtil.ReloadWindowStart.Value && normalizedTime < ConfigUtil.ReloadWindowEnd.Value;
    }

    /// <summary>
    /// Based on <see cref="IdlingOperation.ReloadMag"/>
    /// </summary>
    public override void ReloadMag(MagazineItemClass magazine, ItemAddress itemAddress, Callback finishCallback, Callback startCallback)
    {
        LoggerUtil.Debug("MagCheckReloadOperation::ReloadMag");
        FirearmsAnimator_0.SetAnimationSpeed(1f);

        DisableAimingOnReload();
        SetTriggerPressed(false);
        var reloadResult = ReloadResult.Run(
            Player_0.InventoryController,
            Weapon_0,
            magazine,
            false,
            Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId),
            itemAddress
        );
        if (reloadResult.Failed)
        {
            LoggerUtil.Error($"MagCheckReloadOperation::ReloadMag Failed to reload. Error: {reloadResult.Error}");
            finishCallback?.Invoke(reloadResult);
            return;
        }

        this.TransitionToReload(false, false);
        State = EOperationState.Finished;
        FirearmController_0.InitiateOperation<ReloadOperation>().Start(reloadResult.Value, finishCallback);
        startCallback?.Succeed();
    }

    /// <summary>
    /// Based on <see cref="IdlingOperation.QuickReloadMag"/>
    /// </summary>
    public override void QuickReloadMag(MagazineItemClass magazine, Callback finishCallback, Callback startCallback)
    {
        LoggerUtil.Debug("MagCheckReloadOperation::QuickReloadMag");
        FirearmsAnimator_0.SetAnimationSpeed(1f);

        DisableAimingOnReload();
        SetTriggerPressed(false);
        var reloadResult = ReloadResult.Run(
            Player_0.InventoryController,
            Weapon_0,
            magazine,
            true,
            Weapon_0.MalfState.IsKnownMalfunction(Player_0.ProfileId),
            null
        );
        if (reloadResult.Failed)
        {
            LoggerUtil.Error($"MagCheckReloadOperation::QuickReloadMag Failed to reload. Error: {reloadResult.Error}");
            finishCallback?.Invoke(reloadResult);
            return;
        }

        this.TransitionToReload(true, false);
        State = EOperationState.Finished;
        FirearmController_0.InitiateOperation<ReloadOperation>().Start(reloadResult.Value, finishCallback);
        startCallback?.Succeed();
    }

    /// <summary>
    /// UI Fixes' reload in place feature uses a SwapOperation.
    /// This override handles that swap.
    /// </summary>
    public override void Execute(IInventoryOperation operation, Callback callback)
    {
        if (operation is not IOneItemOperation { Item1: MagazineItemClass } oneItemOperation)
        {
            base.Execute(operation, callback);
            return;
        }

#if DEBUG
        // This operation only runs for weapons with ReloadMode.ExternalMagazine/ExternalMagazineWithInternalReloadSupport,
        // see Patches.RunUtilityOpPatch
        var reloadMode = Weapon_0.ReloadMode;
        if (reloadMode == Weapon.EReloadMode.InternalMagazine || reloadMode == Weapon.EReloadMode.OnlyBarrel)
        {
            callback.Succeed();
            return;
        }
#endif

        // Insert magazine operation
        if (oneItemOperation.To1 is not null && oneItemOperation.To1.IsChildOf(Weapon_0))
        {
            LoggerUtil.Debug("MagCheckReloadOperation::Execute Insert mag operation");
            FirearmsAnimator_0.SetAnimationSpeed(1f);

            if (_swapRemoveOperation is null)
            {
                // Should not be null at this point, start idle operation
                LoggerUtil.Error("MagCheckReloadOperation::Execute Something unexpected happened while swapping magazines!");
                callback.Fail("Remove magazine operation is missing during execution of swap reload"); // Throws

                State = EOperationState.Finished;
                FirearmController_0.InitiateOperation<IdlingOperation>().Start(null);
                return;
            }

            State = EOperationState.Finished;
            FirearmController_0.InitiateOperation<SwapReloadOperation>()
               .Start((MagazineItemClass)_swapRemoveOperation.Item_0, (Slot)_swapRemoveOperation.ItemAddress_0.Container, callback);
            return;
        }

        // Remove magazine operation
        if (oneItemOperation.From1 is not null
            && oneItemOperation.From1.IsChildOf(Weapon_0)
            && oneItemOperation is RemoveOperation removeOp)
        {
            LoggerUtil.Debug("MagCheckReloadOperation::Execute Remove mag operation...skipped");
            _swapRemoveOperation = removeOp;
        }

        callback.Succeed();
    }

    public override void SetInventoryOpened(bool opened)
    {
        FirearmController_0.InventoryOpened = opened;
        FirearmsAnimator_0.SetInventory(opened);
    }

    public override void FastForward()
    {
        // Fika runs FastForward before calling ReloadMag,
        // so we need a packet to set _reloadCalled or else it finishes this operation and ReloadMag won't be called.
        LoggerUtil.Debug("MagCheckReloadOperation::FastForward");
        if (_reloadCalled)
        {
            LoggerUtil.Debug("MagCheckReloadOperation::FastForward Skipped FastForward");
            return;
        }

        FirearmsAnimator_0.SetAnimationSpeed(1f);
        State = EOperationState.Ready;
        OnIdleStartEvent();
    }

    public override void OnIdleStartEvent()
    {
        LoggerUtil.Debug("MagCheckReloadOperation::OnIdleStartEvent");

        base.OnIdleStartEvent();

        if (!Player_0.FirstPersonPointOfView || ConfigUtil.ReloadMode.Value != KeybindsUtil.EReloadMode.Release) return;

        if (KeybindsUtil.AreCheckAndReloadKeysConflicting())
        {
            ReloadConflictPatch.SkipReload();
        }
    }

    public void SetReloadCalled()
    {
        _reloadCalled = true;
    }

    private enum SpeedState : byte
    {
        Normal,
        Slowed,
        Restored,
    }
}
