using System.Reflection;
using MagCheckInterrupt.Utils;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.Patches;

/// <summary>
/// This patch skips the fast reload animation.
/// </summary>
public class ReloadFastAnimationPatch : ModulePatch
{
    private static bool _toSkip;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(FirearmsAnimator).GetMethod(nameof(FirearmsAnimator.ReloadFast), [typeof(bool)]);
    }

    [PatchPrefix]
    public static bool Prefix(FirearmsAnimator __instance)
    {
        if (!_toSkip) return true;

        LoggerUtil.Debug("ReloadAnimationPatch::Prefix Skipped reload animation");
        _toSkip = false;
        return false;
    }

    public static void SkipReloadAnimation()
    {
        _toSkip = true;
    }
}
