using EFT.InventoryLogic;
using MagCheckInterrupt.Patches;
using UnityEngine;

namespace MagCheckInterrupt.Utils;

public static class AnimationUtil
{
    private static readonly int _magCheckHash = Animator.StringToHash("CHECK");
    private static readonly int _magReloadOutHash = Animator.StringToHash("RELOAD OUT");
    private static readonly int _magReloadOutFastHash = Animator.StringToHash("RELOAD OUT ALL");

    private static readonly int _magWithInternalCheckHash = Animator.StringToHash("CHECK MAG");
    private static readonly int _magWithInternalReloadOutHash = Animator.StringToHash("RELOAD OUT MAG");
    private static readonly int _magWithInternalReloadOutFastHash = Animator.StringToHash("RELOAD OUT ALL MAG");

    private static readonly int _chamberCatchCheckHash = Animator.StringToHash("CHECK CHAMBER CATCHED");
    private static readonly int _chamberCatchReloadStartHash = Animator.StringToHash("RELOAD CATCH START");

    public static float GetNormalizedTime(this ObjectInHandsAnimator objectInHandsAnimator, int layerIndex)
    {
        return objectInHandsAnimator.Animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;
    }

    /// <summary>
    /// Performs the animation from a magazine check to a reload.
    /// Also includes, hiding the ammo count and sending of Fika packet.
    /// </summary>
    /// <param name="isFast">True if the reload is a quick reload</param>
    /// <param name="isSwap">True if triggered by a swap operation</param>
    public static void TransitionToReload(this FirearmOperation operation, bool isFast = false, bool isSwap = false)
    {
        if (operation.FirearmsAnimator_0.Animator is not AnimatorWrapper wrapper)
        {
            var typeFullName = operation.FirearmsAnimator_0.Animator.GetType().FullName;
            LoggerUtil.Error($"MagCheckReloadOperation::TransitionToReload Cannot transition directly into a reload. {typeFullName}");
            return;
        }

        DoReloadCrossfade(operation, wrapper, isFast, isSwap);

        if (!operation.Player_0.FirstPersonPointOfView) return;

        // We don't want observed players re-sending packets and
        // our packet needs to be sent first before Fika's reload packet (ReloadMag.startCallback).
        // But if it's a swap reload, no need to send a packet.
        if (!isSwap && External.Fika.IsPresent)
        {
            External.Fika.SendReloadCalledPacket();
        }

        AmmoDetailsPatch.HideAmmoCount();
    }

    private static void DoReloadCrossfade(FirearmOperation operation, AnimatorWrapper wrapper, bool isFast, bool isSwap)
    {
        if (wrapper.TryGetReloadOutHash(out var reloadOutHash, isFast, operation.Weapon_0))
        {
            // GClass2016.Start calls FirearmsAnimator.Reload(bool b), so we need to skip the reload animation and do our own crossfade.
            // But if it's a swap reload, no need to skip the reload animation, it's not called by the insert mag operation.
            if (!isSwap)
            {
                SkipReloadAnimation(isFast);
            }

            wrapper.Animator_0.CrossFade(
                reloadOutHash,
                isSwap ? 0.15f : 0.10f, // Anything more than 0.10f looks like a magazine swap
                FirearmsAnimator.HANDS_LAYER_INDEX,
                0.50f // Skip mag out from weapon animation
            );
        }
        else if (isSwap && operation.FirearmsAnimator_0.GetBoltCatch())
        {
            // Special case: For weapons such as the SKS with an empty mag and no bullet in the chamber,
            // don't do crossfade and instead play the reload animation.
            operation.FirearmsAnimator_0.PullOutMagInInventoryMode();
            operation.FirearmsAnimator_0.ResetInsertMagInInventoryMode();
        }
    }

    private static bool TryGetReloadOutHash(this AnimatorWrapper animator, out int reloadOutHash, bool isFast = false, Weapon weapon = null)
    {
        var currentStateHash = animator.GetCurrentAnimatorStateInfo(FirearmsAnimator.HANDS_LAYER_INDEX).shortNameHash;
        if (currentStateHash == _magCheckHash)
        {
            reloadOutHash = isFast ? _magReloadOutFastHash : _magReloadOutHash;
            return true;
        }
        if (currentStateHash == _magWithInternalCheckHash)
        {
            reloadOutHash = isFast ? _magWithInternalReloadOutFastHash : _magWithInternalReloadOutHash;
            return true;
        }
        if (currentStateHash == _chamberCatchCheckHash)
        {
            reloadOutHash = _chamberCatchReloadStartHash;
            return false;
        }

        LoggerUtil.Error(
            $"Unsupported mag check hash: {GClass758.GetAnimStateByNameHash(currentStateHash)} ({currentStateHash}) for weapon: {weapon?.ToFullString()}"
        );
        reloadOutHash = -1;
        return false;
    }

    private static void SkipReloadAnimation(bool isFast)
    {
        if (isFast)
        {
            ReloadFastAnimationPatch.SkipReloadAnimation();
        }
        else
        {
            ReloadAnimationPatch.SkipReloadAnimation();
        }
    }
}
