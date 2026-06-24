using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.0 — Pack Mule (🎒 Saqueador + 🛡️ Tanque): +30% no limite de carga, como PISO.
///     Postfix no getter <c>SkillManager.CarryingWeightRelativeModifier</c> (= 1 + StrengthBuffLiftWeightInc):
///     garante o bônus máximo de carga desde o início SEM somar com o ganho da Strength
///     (efetivo = o maior; teto +30%, respeita o cap vanilla Max(0.3) — decisão K).
///     Gating: SÓ o SkillManager do player local E classe = Scavenger/Tank.
///     Lê o valor do F12 no apply-time (F12-live).
/// </summary>
internal class PackMulePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(SkillManager), nameof(SkillManager.CarryingWeightRelativeModifier));
    }

    [PatchPostfix]
    private static void Postfix(SkillManager __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.PackMuleEnabled?.Value != true)
            {
                return;
            }

            // Na raid: só o SkillManager do MainPlayer local (não bufar bots).
            // Fora da raid (stash/menu — GameWorld null, sem bots): gateia só pela classe →
            // o limite de peso no stash já reflete o +30%.
            var gw = Singleton<GameWorld>.Instance;
            if (gw != null && !ReferenceEquals(__instance, gw.MainPlayer?.Skills))
            {
                return;
            }

            if (!SkillMultipliers.IsLocalClass("Scavenger") && !SkillMultipliers.IsLocalClass("Tank"))
            {
                return;
            }

            var floor = 1f + (PerksConfig.PackMuleCarryBonus?.Value ?? 0f);
            if (__result < floor)
            {
                __result = floor;   // piso (não soma)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Pack Mule falhou: {ex.Message}");
        }
    }
}
