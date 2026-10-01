using System.Reflection;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using Systems.Effects;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch allows bots to hear if a bullet has impacted nearby
/// </summary>
public class BulletImpactPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EffectsCommutator), nameof(EffectsCommutator.PlayHitEffect));
    }

    [PatchPostfix]
    public static void PatchPostfix(EftBulletClass info)
    {
        if (BotManagerComponent.Instance != null)
        {
            BotManagerComponent.Instance.BotHearing.BulletImpacted(info);
        }
    }
}
