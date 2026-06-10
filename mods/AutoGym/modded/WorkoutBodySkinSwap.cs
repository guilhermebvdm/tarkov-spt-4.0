using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace AutoGym;

internal static class WorkoutBodySkinSwap
{
    private static PlayerBody? _swappedBody;
    private static global::DependencyGraphClass<IEasyBundle>.GClass1661? _retainedBundles;
    private static int _generation;

    internal static void Apply(HideoutPlayerOwner owner)
    {
        try // ref: CR-01-02 — patch body must never break PrepareWorkout
        {
            // ref: PA-01-01 — Prepare/Stop pairing is not guaranteed by the game; clear
            // orphaned state from a body destroyed without StopWorkout (Unity lifetime check)
            if (_swappedBody is not null && !_swappedBody)
            {
                _swappedBody = null;
                _retainedBundles?.Release();
                _retainedBundles = null;
            }
            if (Plugin.SwapWorkoutBodySkin?.Value != true)
            {
                return;
            }
            // ref: Assembly-CSharp/EFT/HideoutPlayerOwner.cs:120 (HideoutPlayer property)
            PlayerBody? playerBody = owner?.HideoutPlayer?.PlayerBody;
            if (playerBody == null || _swappedBody != null)
            {
                return; // double-start without Stop: keep the first state (idempotent)
            }
            _ = ApplyAsync(playerBody, ++_generation);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to start body skin swap: {ex}");
        }
    }

    private static async Task ApplyAsync(PlayerBody playerBody, int generation)
    {
        try
        {
            // ref: CR-01-04 — invalid config id gets a specific message instead of a stack trace
            MongoID skinId;
            try
            {
                // ref: Assembly-CSharp/EFT/MongoID.cs:59 (string ctor)
                skinId = new MongoID(Plugin.WorkoutBodySkinId.Value);
            }
            catch (Exception)
            {
                Plugin.Log?.LogWarning($"AutoGym: '{Plugin.WorkoutBodySkinId.Value}' is not a valid customization id.");
                return;
            }
            // ref: Assembly-CSharp/EFT/PlayerBody.cs:514 (BodyCustomization keeps the profile values)
            MongoID originalId = playerBody.BodyCustomization[EBodyModelPart.Body];
            if (skinId == originalId)
            {
                return; // player already wears the target skin
            }
            CustomizationSolverClass? solver = Singleton<CustomizationSolverClass>.Instance;
            if (solver == null)
            {
                return;
            }
            // ref: Assembly-CSharp/CustomizationSolverClass.cs:348
            ResourceKey? bundle = solver.GetBundle(skinId);
            if (bundle == null)
            {
                // ref: PA-01-05 — distinguish a suite id from a missing template
                // ref: Assembly-CSharp/CustomizationSolverClass.cs:391 (GetSuite)
                if (solver.GetSuite(skinId) != null)
                {
                    Plugin.Log?.LogWarning($"AutoGym: {skinId} is a SUITE id; configure the body template id instead (see AllTheClothes config 'body' field).");
                }
                else
                {
                    Plugin.Log?.LogWarning($"AutoGym: workout body skin {skinId} not found (AllTheClothes missing?), skipping swap.");
                }
                return;
            }
            // ref: Assembly-CSharp/GClass1857.cs:125 + :173 (same pattern as GClass1041.cs:195-196)
            var handle = GClass1857.Retain(Singleton<IEasyAssets>.Instance, new[] { bundle.path });
            try // ref: CR-01-01 — never leak the retained handle on failure
            {
                await GClass1857.LoadBundles(handle);
                if (generation != _generation || playerBody == null)
                {
                    handle.Release(); // workout ended during the load — do not apply
                    return;
                }
                // ref: Assembly-CSharp/EFT/PlayerBody.cs:747 (SetSkin destroys the previous part skin)
                playerBody.SetSkin(
                    new KeyValuePair<EBodyModelPart, ResourceKey>(EBodyModelPart.Body, bundle),
                    playerBody.SkeletonRootJoint); // ref: Assembly-CSharp/EFT/PlayerBody.cs:510
                // ref: PA-01-04 — keep clipping rules coherent with the displayed body
                // ref: Assembly-CSharp/CustomizationSolverClass.cs:367 + EFT/PlayerBody.cs:571
                playerBody.HasIntergratedArmor = solver.HasIntegratedArmor(skinId);
                _retainedBundles = handle;
                _swappedBody = playerBody;
            }
            catch
            {
                handle.Release();
                throw;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to swap workout body skin: {ex}");
        }
    }

    internal static void Restore(HideoutPlayerOwner owner)
    {
        _generation++; // invalidate any in-flight ApplyAsync
        PlayerBody? playerBody = _swappedBody;
        _swappedBody = null;
        try
        {
            if (playerBody != null)
            {
                MongoID originalId = playerBody.BodyCustomization[EBodyModelPart.Body];
                CustomizationSolverClass? solver = Singleton<CustomizationSolverClass>.Instance;
                ResourceKey? bundle = solver?.GetBundle(originalId);
                if (solver != null && bundle != null)
                {
                    // ref: Assembly-CSharp/EFT/PlayerBody.cs:747
                    playerBody.SetSkin(
                        new KeyValuePair<EBodyModelPart, ResourceKey>(EBodyModelPart.Body, bundle),
                        playerBody.SkeletonRootJoint);
                    // ref: PA-01-04
                    playerBody.HasIntergratedArmor = solver.HasIntegratedArmor(originalId);
                }
            }
        }
        catch (MissingReferenceException) // ref: CR-01-03 — normal hideout teardown, not a failure
        {
            Plugin.Log?.LogDebug("AutoGym: body destroyed before restore (hideout teardown), nothing to do.");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to restore body skin: {ex}");
        }
        finally
        {
            try // ref: CR-01-02 — a Release failure must never break StopWorkout
            {
                _retainedBundles?.Release(); // ref: Assembly-CSharp/GClass1041.cs:112-116
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"AutoGym failed to release workout skin bundles: {ex}");
            }
            _retainedBundles = null;
        }
    }
}
