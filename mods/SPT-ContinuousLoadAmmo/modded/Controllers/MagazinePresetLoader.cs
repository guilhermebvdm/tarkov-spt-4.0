using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContinuousLoadAmmo.Patches;
using ContinuousLoadAmmo.Utils;
using EFT;
using EFT.Communications;
using UnityEngine;

namespace ContinuousLoadAmmo.Controllers;

public class MagazinePresetLoader : IDisposable
{
    private readonly LoadAmmoController _loadAmmoController;
    private CancellationTokenSource _loadPresetCancellationSource;

    public bool PresetLoaderIsActive => _loadPresetCancellationSource is not null;

    public MagazinePresetLoader(LoadAmmoController loadAmmoController)
    {
        _loadAmmoController = loadAmmoController;

        OnClickPatch.CancelPresetLoaderOnClick += CancelMagPresetLoading;
        ApplyMagPresetPatch.OnApplyMagPreset += InventoryLoadMagPreset;
    }

    public void QuickLoadMagPreset(MagazineBuildPresetClass preset)
    {
        if (!_loadAmmoController.IsQuickLoadAvailable(out var availableAmmo, out var magazine, preset.GetCaliberReally()))
        {
            CommonUtils.DisplayNotification(
                $"No reachable ammo or magazines found to load for preset: {preset.DisplayText()}",
                ENotificationIconType.Alert,
                true
            );
            return;
        }

        _ = LoadingMagPresetInternalAsync(preset, [magazine], availableAmmo);
    }

    public void CancelMagPresetLoading()
    {
        if (!PresetLoaderIsActive) return;

        try
        {
            _loadPresetCancellationSource?.Cancel();
            _loadPresetCancellationSource?.Dispose();
        }
        catch (Exception ex)
        {
            ContinuousLoadAmmo.LogSource.LogWarning($"ContinuousLoadAmmo: CancelMagPresetLoading: {ex.Message}");
        }
        finally
        {
            _loadPresetCancellationSource = null;
        }
    }

    public bool IsPresetAvailableForCurrentWeapon(out MagazineBuildPresetClass preset)
    {
        var weaponCaliber = _loadAmmoController.GetCurrentWeaponCaliber();
        preset = ProfileMagazinePresetStore.GetMagPreset(weaponCaliber);

        return preset is not null;
    }

    private void StartNewLoadMagPreset(out CancellationToken token)
    {
        CancelMagPresetLoading();
        _loadPresetCancellationSource = new CancellationTokenSource();
        token = _loadPresetCancellationSource.Token;
    }

    private void InventoryLoadMagPreset(MagazineBuildPresetClass preset, List<MagazineItemClass> magazines)
    {
        if (!_loadAmmoController.GetAllAmmoForMagazine(out var availableAmmo, magazines[0]))
        {
            CommonUtils.DisplayNotification(
                $"No reachable ammo or magazines found to load for preset: {preset.DisplayText()}",
                ENotificationIconType.Alert,
                true
            );
            CancelMagPresetLoading();
            return;
        }

        _ = LoadingMagPresetInternalAsync(preset, magazines, availableAmmo);
    }

    private async Task LoadingMagPresetInternalAsync(
        MagazineBuildPresetClass preset,
        List<MagazineItemClass> magazines,
        List<AmmoItemClass> availableAmmo
    )
    {
        try
        {
            StartNewLoadMagPreset(out var token);
            foreach (var magazine in magazines)
            {
                token.ThrowIfCancellationRequested();

                CommonUtils.DisplayNotification($"Loading {preset.DisplayText()}", ENotificationIconType.Note);

                // Bottom
                var bottomCount = 0;
                foreach (var bottom in preset.Bottom)
                {
                    token.ThrowIfCancellationRequested();

                    if (bottom is null) continue;

                    bottomCount = bottom.Count;
                    if (magazine.Count >= bottom.Count) continue;

                    var toLoad = Mathf.Min(bottom.Count, bottom.Count - magazine.Count);
                    if (!await TryLoadPresetStepAsync(availableAmmo, magazine, bottom, toLoad, token))
                    {
                        CancelMagPresetLoading();
                        return;
                    }
                }

                // Loop
                // Track toSkip to resume loading from current count
                var toSkip = magazine.Count - bottomCount;
                var freeLoopSpace = magazine.MaxCount - magazine.Count;
                var topCount = 0;
                foreach (var top in preset.Top)
                {
                    if (top is not null) topCount += top.Count;
                }
                freeLoopSpace -= topCount;

                while (freeLoopSpace > 0)
                {
                    // Guards against preset.Loop being empty (or fully skipped by toSkip every
                    // pass) — without this, freeLoopSpace never decrements and the while spins
                    // forever on the main thread with no await in between.
                    var loadedThisPass = false;

                    foreach (var loop in preset.Loop)
                    {
                        token.ThrowIfCancellationRequested();
                        if (loop is null || freeLoopSpace <= 0) continue;

                        var toLoad = (int)loop.Count;
                        if (toSkip > 0) // Resume loading from current count
                        {
                            // Should skip entire group?
                            if (toSkip >= toLoad)
                            {
                                toSkip -= toLoad;
                                continue;
                            }

                            // Load remaining
                            toLoad -= toSkip;
                            toSkip = 0;
                        }
                        toLoad = Mathf.Min(toLoad, freeLoopSpace);
                        if (!await TryLoadPresetStepAsync(availableAmmo, magazine, loop, toLoad, token))
                        {
                            CancelMagPresetLoading();
                            return;
                        }
                        freeLoopSpace -= toLoad;
                        loadedThisPass = true;
                    }

                    if (!loadedThisPass) break;
                }

                // Top
                foreach (var top in preset.Top)
                {
                    token.ThrowIfCancellationRequested();

                    if (top is null) continue;

                    var toLoad = Mathf.Min(top.Count, magazine.MaxCount - magazine.Count);
                    if (!await TryLoadPresetStepAsync(availableAmmo, magazine, top, toLoad, token))
                    {
                        CancelMagPresetLoading();
                        return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            ContinuousLoadAmmo.LogSource.LogError($"ContinuousLoadAmmo: LoadingMagPresetInternalAsync excecao: {ex}");
            CommonUtils.DisplayNotification("Failed to load magazine preset", ENotificationIconType.Alert, true);
        }

        CancelMagPresetLoading();
    }

    /// <returns>false if the ammo insert was rejected (server failure or no matching ammo without fallback) — caller must stop the preset sequence instead of assuming this step succeeded</returns>
    private async Task<bool> TryLoadPresetStepAsync(
        List<AmmoItemClass> availableAmmo,
        MagazineItemClass magazine,
        MagazineBuildPresetClass.GClass2578 preset,
        int toLoad,
        CancellationToken token
    )
    {
        var matchingAmmo = GetMatchingAmmo(availableAmmo, preset.TemplateId, toLoad);
        if (matchingAmmo is null)
        {
            var missingMessage =
                $"{"Preset missing ammo".Localized()}: {preset.TemplateId.LocalizedShortName()}, Count: {toLoad}";
            CommonUtils.DisplayNotification(missingMessage, ENotificationIconType.Alert, true, ENotificationDurationType.Long);

            if (ContinuousLoadAmmo.MagPresetFallback.Value)
            {
                var fallbackAmmo = GetValidAmmo(availableAmmo);
                if (fallbackAmmo is not null)
                {
                    CommonUtils.DisplayNotification($"Loading {fallbackAmmo.LocalizedShortName()}", ENotificationIconType.Note);
                    return await _loadAmmoController.LoadMagazineAsync(fallbackAmmo, magazine, token);
                }
            }
            CancelMagPresetLoading();
            return false;
        }
        return await _loadAmmoController.LoadMagazineAsync(matchingAmmo, magazine, token, toLoad);
    }

    private static AmmoItemClass GetMatchingAmmo(List<AmmoItemClass> ammo, MongoID templateId, int count)
    {
        foreach (var ammoItem in ammo)
        {
            if (ammoItem.TemplateId != templateId || ammoItem.StackObjectsCount < count) continue;

            return ammoItem;
        }

        return null;
    }

    private static AmmoItemClass GetValidAmmo(List<AmmoItemClass> ammo)
    {
        foreach (var ammoItem in ammo)
        {
            if (ammoItem.StackObjectsCount <= 0) continue;

            return ammoItem;
        }

        return null;
    }

    public void Dispose()
    {
        CancelMagPresetLoading();

        OnClickPatch.CancelPresetLoaderOnClick -= CancelMagPresetLoading;
        ApplyMagPresetPatch.OnApplyMagPreset -= InventoryLoadMagPreset;
    }
}
