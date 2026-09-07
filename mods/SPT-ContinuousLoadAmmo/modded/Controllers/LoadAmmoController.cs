using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ContinuousLoadAmmo.Models;
using ContinuousLoadAmmo.Patches;
using ContinuousLoadAmmo.Utils;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using UnityEngine;
using static EFT.Player;

namespace ContinuousLoadAmmo.Controllers;

public class LoadAmmoController : IDisposable
{
    private readonly Player _player;
    private readonly MagazinePresetLoader _magazinePresetLoader;
    private MagazineItemClass _magazine;
    private bool _isReachable = true;
    private bool _disposed;

    public event Action<float, int, int> OnStartLoading;
    public event Action<Item> OnCloseInventoryLoading;
    public event Action OnEndLoading;
    public event Action OnPlayerDestroy;

    public static LoadAmmoController Instance { get; private set; }

    public bool IsActive => PlayerInventoryController.Interface19_0 is not null || _magazinePresetLoader.PresetLoaderIsActive;
    public bool IsInventoryOpened => _player.IsInventoryOpened;
    public PlayerInventoryController PlayerInventoryController { get; }

    public LoadAmmoController(Player player)
    {
        Instance = this;
        _player = player;
        if (_player.InventoryController is not PlayerInventoryController playerInvCont)
        {
            throw new InvalidOperationException("Player.InventoryController is not Player.PlayerInventoryController");
        }

        PlayerInventoryController = playerInvCont;
        PlayerInventoryController.SetNextProcessLocked(false);

        PlayerInventoryController.ActiveEventAdded += LoadingStart; // Always CommandStatus.Begin
        InventoryScreenClosePatch.OnInventoryClose += LoadingOutsideInventory;
        UnloadMagazineStartPatch.OnLoadingEnd += LoadingEnd;
        LoadMagazineStartPatch.OnLoadingEnd += LoadingEnd;
        _player.OnHandsControllerChanged += StopLoadingOnHandsChange;
        _player.OnIPlayerDeadOrUnspawn += OnDestroy;

        _magazinePresetLoader = new MagazinePresetLoader(this);
    }

    public bool CanLoadOutsideInventory()
    {
        bool isOnLadder = _player != null && _player.gameObject != null && _player.gameObject.GetComponent("PlayerLadderController") != null;
        return !isOnLadder && !PlayerInventoryController.HasAnyHandsActionNonLinq() && _isReachable;
    }

    public bool IsQuickLoadAvailable(out List<AmmoItemClass> reachableAmmo, out MagazineItemClass foundMagazine, string caliber = null)
    {
        reachableAmmo = null;
        foundMagazine = null;
        return GetReachableAmmoOfCaliber(out reachableAmmo, caliber) && GetMagazineForAmmo(reachableAmmo[0], out foundMagazine);
    }

    public void TryQuickLoadAmmo()
    {
        if (!CanLoadOutsideInventory())
        {
            return;
        }

        if (!IsQuickLoadAvailable(out var reachableAmmo, out var foundMagazine))
        {
            CommonUtils.DisplayNotification(
                "No reachable ammo or magazines found for the current weapon",
                ENotificationIconType.Alert,
                true
            );
            return;
        }

        AmmoItemClass chosenAmmo = null;
        if (ContinuousLoadAmmo.QuickLoadMode.Value == QuickLoadMode.LastBulletMagazine)
        {
            var currentMagazine = _player.LastEquippedWeaponOrKnifeItem.GetCurrentMagazine();
            if (currentMagazine is not null)
            {
                foreach (var currAmmo in reachableAmmo)
                {
                    if (currentMagazine.FirstRealAmmo() is not AmmoItemClass ammoInsideMag
                        || ammoInsideMag.TemplateId != currAmmo.TemplateId)
                    {
                        continue;
                    }

                    // Magazine ammo matched with current reachable ammo
                    chosenAmmo = currAmmo;
                    break;
                }
            }
        }
        // PrioritizeHighestPenetration is false or if no ammo matched from magazine's first ammo, choose first reachable ammo available
        chosenAmmo ??= reachableAmmo[0];
        LoadMagazine(chosenAmmo, foundMagazine);

        CommonUtils.DisplayNotification($"Loading {chosenAmmo.LocalizedShortName()}", ENotificationIconType.Note);
    }

    public void TryQuickLoadLastPreset()
    {
        if (_magazinePresetLoader.IsPresetAvailableForCurrentWeapon(out var preset))
        {
            _magazinePresetLoader.QuickLoadMagPreset(preset);
            return;
        }

        // Fallback, no preset selected yet through context menu or preset not compatible with weapon
        TryQuickLoadAmmo();
    }

    public void LoadMagazine(AmmoItemClass ammo, MagazineItemClass magazine)
    {
        var loadCount = Mathf.Min(ammo.StackObjectsCount, magazine.MaxCount - magazine.Count);
        _ = LoadMagazineFireAndForgetAsync(ammo, magazine, loadCount);
    }

    // LoadMagazine below is awaited but its IResult was previously discarded — a server-side
    // rejection (eg. Fika: "item is currently being modified") came back as a failed result,
    // not an exception, so it went unnoticed and the player was left with the "Loading X"
    // notification already shown and nothing actually happening.
    private async Task LoadMagazineFireAndForgetAsync(AmmoItemClass ammo, MagazineItemClass magazine, int loadCount)
    {
        try
        {
            var result = await PlayerInventoryController.LoadMagazine(ammo, magazine, loadCount, false);
            if (result.Failed)
            {
                ContinuousLoadAmmo.LogSource.LogWarning($"ContinuousLoadAmmo: LoadMagazine falhou: {result}");
                CommonUtils.DisplayNotification("Failed to load ammo, try again", ENotificationIconType.Alert, true);
            }
        }
        catch (Exception ex)
        {
            ContinuousLoadAmmo.LogSource.LogError($"ContinuousLoadAmmo: LoadMagazine excecao: {ex}");
        }
    }

    /// <returns>false if the server rejected the operation (result.Failed) — caller should stop instead of assuming success</returns>
    public async Task<bool> LoadMagazineAsync(AmmoItemClass ammo, MagazineItemClass magazine, CancellationToken token, int? ammoCount = null)
    {
        var loadCount = ammoCount ?? Mathf.Min(ammo.StackObjectsCount, magazine.MaxCount - magazine.Count);
        while (PlayerInventoryController.Locked)
        {
            token.ThrowIfCancellationRequested();
            await Task.Yield();
        }
        var result = await PlayerInventoryController.LoadMagazine(ammo, magazine, loadCount, false);
        if (result.Failed)
        {
            ContinuousLoadAmmo.LogSource.LogWarning($"ContinuousLoadAmmo: LoadMagazineAsync falhou: {result}");
        }
        return !result.Failed;
    }

    private readonly List<MagazineItemClass> _reachableMagazinesScratch = [];

    /// <summary>
    /// Find reachable magazine for ammo
    /// </summary>
    /// <param name="ammo">Ammo that should be compatible with the magazine</param>
    public bool GetMagazineForAmmo(AmmoItemClass ammo, out MagazineItemClass foundMagazine)
    {
        foundMagazine = null;
        _reachableMagazinesScratch.Clear();
        if (ContinuousLoadAmmo.ReachableOnly.Value)
        {
            // Only get top level container's items for quick load, non-recursive
            PlayerInventoryController.GetAcceptableItemsNonAlloc(
                ReachableSlots,
                _reachableMagazinesScratch,
                (mag) => PlayerInventoryController.Examined(mag) && mag.Count != mag.MaxCount && mag.CheckCompatibility(ammo),
                ContainerPredicate
            );
        }
        else
        {
            // Can be recursive
            GetReachableItems(
                _reachableMagazinesScratch,
                (mag) => PlayerInventoryController.Examined(mag) && mag.Count != mag.MaxCount && mag.CheckCompatibility(ammo)
            );
        }
        if (_reachableMagazinesScratch.Count <= 0) return false;

        // Some magazines can have multiple calibers
        _reachableMagazinesScratch.RemoveAll(mag => mag.HasAmmoWithDifferentCaliber(ammo));

        // Sort by almost full
        _reachableMagazinesScratch.Sort((a, b) => (a.MaxCount - a.Count).CompareTo(b.MaxCount - b.Count));

        // Mag with most amount
        foundMagazine = _reachableMagazinesScratch[0];
        return true;
    }

    private readonly List<AmmoItemClass> _reachableAmmoScratch = [];

    /// <summary>
    /// Find reachable ammo of specified caliber. Used by quick load
    /// </summary>
    /// <param name="reachableAmmo">One of each ammo type found then sorted by Penetration Power descending</param>
    /// <param name="ammoCaliber">Optional, fallbacks to current weapon's caliber</param>
    public bool GetReachableAmmoOfCaliber(out List<AmmoItemClass> reachableAmmo, string ammoCaliber = null)
    {
        _reachableAmmoScratch.Clear();
        reachableAmmo = _reachableAmmoScratch;

        ammoCaliber ??= GetCurrentWeaponCaliber();
        if (ammoCaliber.IsNullOrEmpty())
        {
            return false;
        }

        if (ContinuousLoadAmmo.ReachableOnly.Value)
        {
            // Only get top level container's items for quick load, non-recursive
            PlayerInventoryController.GetAcceptableItemsNonAlloc(
                ReachableSlots,
                reachableAmmo,
                (ammo) => PlayerInventoryController.Examined(ammo) && ammo.Caliber == ammoCaliber,
                ContainerPredicate
            );
        }
        else
        {
            // Can be recursive
            GetReachableItems(reachableAmmo, (ammo) => PlayerInventoryController.Examined(ammo) && ammo.Caliber == ammoCaliber);
        }
        if (reachableAmmo.Count <= 0) return false;

        // Sort penetration power highest to lowest, then stack count ascending
        reachableAmmo.Sort((a, b) =>
            {
                var result = b.PenetrationPower.CompareTo(a.PenetrationPower);
                if (result == 0)
                {
                    result = a.StackObjectsCount.CompareTo(b.StackObjectsCount);
                }
                return result;
            }
        );

        return true;
    }

    private readonly List<AmmoItemClass> _allAmmoScratch = [];

    private static readonly Comparison<AmmoItemClass> _ammoComparison = (a, b) =>
    {
        var result = b.PenetrationPower.CompareTo(a.PenetrationPower);
        if (result == 0)
        {
            result = a.StackObjectsCount.CompareTo(b.StackObjectsCount);
        }
        return result;
    };

    /// <summary>
    /// Find ammo for <paramref name="magazine"/>. Used by loading mag presets in the inventory screen
    /// </summary>
    /// <param name="magazine">Magazine to be checked compatible with</param>
    public bool GetAllAmmoForMagazine(out List<AmmoItemClass> allAmmo, MagazineItemClass magazine)
    {
        _allAmmoScratch.Clear();
        allAmmo = _allAmmoScratch;
        PlayerInventoryController.Inventory.Equipment.GetAcceptableItemsNonAlloc(
            _reachableAll,
            allAmmo,
            (ammo) => PlayerInventoryController.Examined(ammo) && magazine.CheckCompatibility(ammo),
            ContainerPredicate
        );
        if (allAmmo.Count <= 0) return false;

        allAmmo.Sort(_ammoComparison);
        return true;
    }

    public void StopLoading()
    {
        _magazinePresetLoader.CancelMagPresetLoading();
        PlayerInventoryController.StopProcesses();

        if (_stateCoroutine != null && _player != null)
        {
            _player.StopCoroutine(_stateCoroutine);
            _stateCoroutine = null;
        }

        if (_player?.MovementContext != null)
        {
            _player.MovementContext.RemoveStateSpeedLimit(ESpeedLimit.BarbedWire);
            _player.MovementContext.SetPhysicalCondition(EPhysicalCondition.SprintDisabled, false);

            bool hasLoadAmmoAnim = IsLoadAmmoAnimActiveOrPending(_player);
            if (!hasLoadAmmoAnim && _player.HandsIsEmpty && !IsInventoryOpened)
            {
                _player.TrySetLastEquippedWeapon();
            }
        }
    }

    private static bool IsLoadAmmoAnimActiveOrPending(Player player)
    {
        if (player == null) return false;
        if (player.HandsController?.GetType().Name == "LoadAmmoBundleController") return true;

        try
        {
            var animStateType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(t => t.FullName == "Manimal.LoadAmmoAnim.Patches.LoadAmmoAnimState");

            if (animStateType != null)
            {
                var anyMethod = animStateType.GetMethod("AnyIsOurAnimation", BindingFlags.Public | BindingFlags.Static);
                if (anyMethod != null && (bool)anyMethod.Invoke(null, null)) return true;
            }
        }
        catch { }

        return false;
    }

    public string GetMagAmmoCountByLevel()
    {
        if (_magazine is null)
        {
            ContinuousLoadAmmo.LogSource.LogError("Magazine is null while trying to get ammo count");
            return "MAG NULL";
        }

        var skill = Mathf.Max(
            _player.Profile.MagDrillsMastering,
            Mathf.Max(_player.Profile.CheckedMagazineSkillLevel(_magazine.Id), _magazine.CheckOverride)
        );
        // bool @checked = player.InventoryController.CheckedMagazine(StartPatch.Magazine) // Is mag checked?

        return _magazine.GetAmmoCountByLevel(
            _magazine.Count,
            _magazine.MaxCount,
            skill,
            "#ffffff",
            true,
            false,
            "<color={2}>{0}</color>/{1}"
        );
    }

    public string GetCurrentWeaponCaliber()
    {
        if (_player.HandsController is FirearmController fc)
        {
            return fc.Weapon.GetWeaponCaliber();
        }

        return string.Empty;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (Instance == this)
        {
            Instance = null;
        }

        _magazinePresetLoader.Dispose();
        if (PlayerInventoryController is not null)
        {
            PlayerInventoryController.StopProcesses();
            PlayerInventoryController.ActiveEventAdded -= LoadingStart;
        }
        if (_player != null)
        {
            if (_stateCoroutine != null)
            {
                _player.StopCoroutine(_stateCoroutine);
                _stateCoroutine = null;
            }
            _player.MovementContext?.RemoveStateSpeedLimit(ESpeedLimit.BarbedWire);
            _player.MovementContext?.SetPhysicalCondition(EPhysicalCondition.SprintDisabled, false);
            InventoryScreenClosePatch.OnInventoryClose -= LoadingOutsideInventory;
            UnloadMagazineStartPatch.OnLoadingEnd -= LoadingEnd;
            LoadMagazineStartPatch.OnLoadingEnd -= LoadingEnd;
            _player.OnHandsControllerChanged -= StopLoadingOnHandsChange;
            _player.OnIPlayerDeadOrUnspawn -= OnDestroy;
        }
        OnPlayerDestroy?.Invoke();
        OnStartLoading = null;
        OnCloseInventoryLoading = null;
        OnEndLoading = null;
        OnPlayerDestroy = null;
    }

    private void LoadingStart(GEventArgs1 eventArgs)
    {
        if (_magazinePresetLoader.PresetLoaderIsActive && eventArgs is GEventArgs7 or GEventArgs8)
        {
            _magazinePresetLoader.CancelMagPresetLoading();
        }

        switch (eventArgs)
        {
            case GEventArgs7 loadEvent:
                if (loadEvent.TargetItem is not MagazineItemClass loadMagazine || loadEvent.Item is not AmmoItemClass ammo)
                {
                    return;
                }

                _magazine = loadMagazine;
                _isReachable = IsAtReachablePlace(_magazine, ammo);
                OnStartLoading?.Invoke(loadEvent.LoadTime, loadEvent.LoadCount, 0);
                break;
            case GEventArgs8 unloadEvent:
                _magazine = unloadEvent.FromItem;
                _isReachable = IsAtReachablePlace(_magazine);
                OnStartLoading?.Invoke(unloadEvent.UnloadTime, unloadEvent.UnloadCount, unloadEvent.StartCount);
                break;
            default:
                return;
        }

        // Started loading from outside the inventory
        if (!_player.IsInventoryOpened)
        {
            LoadingOutsideInventory();
        }
    }

    private Coroutine _stateCoroutine;

    private void LoadingOutsideInventory()
    {
        if (IsActive && CanLoadOutsideInventory())
        {
            SetPlayerState(true);
            OnCloseInventoryLoading?.Invoke(_magazine);
            return;
        }
        StopLoading();
    }

    private void LoadingEnd()
    {
        SetPlayerState(false);
        ResetLoading();
        OnEndLoading?.Invoke();
    }

    private void SetPlayerState(bool startAnim)
    {
        if (_player == null) return;

        if (_stateCoroutine != null)
        {
            _player.StopCoroutine(_stateCoroutine);
            _stateCoroutine = null;
        }

        _stateCoroutine = _player.StartCoroutine(SetPlayerStateRoutine(startAnim));
    }

    private System.Collections.IEnumerator SetPlayerStateRoutine(bool startAnim)
    {
        if (_player == null || _player.MovementContext == null) yield break;

        _player.MovementContext.SetPhysicalCondition(EPhysicalCondition.SprintDisabled, startAnim);

        if (startAnim)
        {
            // Se o LoadAmmoAnim estiver no controle das mãos ou em transição, não forçamos SetEmptyHands para evitar concorrência
            bool hasLoadAmmoAnim = IsLoadAmmoAnimActiveOrPending(_player);
            if (!hasLoadAmmoAnim)
            {
                _player.TrySaveLastItemInHands();
                // GInterface198 = IHandsController (resultado assíncrono da transição de mãos do EFT)
                _player.SetEmptyHands(new Callback<GInterface198>(result =>
                {
                    if (result.Failed)
                    {
                        ContinuousLoadAmmo.LogSource.LogWarning($"ContinuousLoadAmmo: SetEmptyHands falhou: {result.Error}");
                    }
                }));
            }

            _player.MovementContext.ChangeSpeedLimit(
                ContinuousLoadAmmo.SpeedLimit.Value * _player.MovementContext.MaxSpeed,
                ESpeedLimit.BarbedWire
            );
        }
        else
        {
            // Timing delay on Unity Main Thread
            yield return new WaitForSeconds(0.8f);

            if (_player == null || _player.MovementContext == null) yield break;

            // Check for active MultiSelect load/unload
            if (MultiSelectInterop.MultiSelectLoadSerializerIsActive || _magazinePresetLoader.PresetLoaderIsActive)
            {
                _stateCoroutine = null;
                yield break;
            }

            // Se o LoadAmmoAnim estiver no controle das mãos, deixa ele restaurar a arma
            bool hasLoadAmmoAnim = IsLoadAmmoAnimActiveOrPending(_player);
            if (!hasLoadAmmoAnim && _player.HandsIsEmpty)
            {
                _player.TrySetLastEquippedWeapon();
            }
            _player.MovementContext.RemoveStateSpeedLimit(ESpeedLimit.BarbedWire);
        }
        _stateCoroutine = null;
    }

    private void ResetLoading()
    {
        _isReachable = true;
        _magazine = null;
    }

    private readonly List<Item> _reachablePlaceItemScratch = [];

    /// <summary>
    /// Check if item is reachable, recursively
    /// </summary>
    private bool IsAtReachablePlace(Item item)
    {
        if (item.CurrentAddress is null) return false;

        _reachablePlaceItemScratch.Clear();
        GetReachableItems(_reachablePlaceItemScratch);
        return _reachablePlaceItemScratch.Contains(item) && PlayerInventoryController.Examined(item);
    }

    /// <summary>
    /// Check if item is reachable, recursively
    /// </summary>
    private bool IsAtReachablePlace(Item item, Item item2)
    {
        if (item.CurrentAddress is null || item2.CurrentAddress is null) return false;

        _reachablePlaceItemScratch.Clear();
        GetReachableItems(_reachablePlaceItemScratch);
        return _reachablePlaceItemScratch.Contains(item)
               && PlayerInventoryController.Examined(item)
               && _reachablePlaceItemScratch.Contains(item2)
               && PlayerInventoryController.Examined(item2);
    }

    private void GetReachableItems<TItem>(List<TItem> preAllocatedList, Predicate<TItem> predicate = null) where TItem : Item
    {
        PlayerInventoryController.Inventory.Equipment.GetAcceptableItemsNonAlloc(
            ReachableSlots,
            preAllocatedList,
            predicate,
            ContainerPredicate
        );
    }

    /// <summary>
    /// Do not pull ammo inside magazines/ammo boxes and only searched containers
    /// </summary>
    private bool ContainerPredicate(GClass3248 container)
    {
        return container is not IAmmoContainer
               && (container is not SearchableItemItemClass searchable
                   || PlayerInventoryController.SearchController.IsSearched(searchable));
    }

    private void StopLoadingOnHandsChange(AbstractHandsController oldHands, AbstractHandsController newHands)
    {
        if (!IsActive) return;

        // Do not stop loading if hands changed to EmptyHands or LoadAmmoAnim's custom controller
        if (newHands is not (null or EmptyHandsController) && !IsLoadAmmoAnimActiveOrPending(_player))
        {
            StopLoading();
        }
    }

    private void OnDestroy(IPlayer player)
    {
        Dispose();
    }

    private static EquipmentSlot[] ReachableSlots => ContinuousLoadAmmo.ReachableOnly.Value ? _reachableOnly : _reachableAll;

    private static readonly EquipmentSlot[] _reachableOnly =
    [
        EquipmentSlot.Pockets,
        EquipmentSlot.TacticalVest,
        EquipmentSlot.ArmBand,
        EquipmentSlot.SecuredContainer,
    ];

    private static readonly EquipmentSlot[] _reachableAll =
    [
        EquipmentSlot.Pockets,
        EquipmentSlot.TacticalVest,
        EquipmentSlot.ArmBand,
        EquipmentSlot.SecuredContainer,
        EquipmentSlot.Backpack,
    ];
}
