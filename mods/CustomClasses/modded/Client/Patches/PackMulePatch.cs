using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     🎒 Saqueador + 🛡️ Tanque — Pack Mule. Desdobrado por classe (2026-07-10): cada uma tem config própria
///     (Enabled + bônus). Retorna o bônus de limite de carga da classe LOCAL (se habilitado); null se não aplica.
///     Compartilhado pelo <see cref="PackMulePatch"/> (piso do modifier) e pelo <see cref="WeightMarkerPatch"/> (marcador de UI).
/// </summary>
internal static class PackMule
{
    internal static float? LocalBonus()
    {
        if (PerksConfig.PackMuleScavEnabled?.Value == true && SkillMultipliers.IsLocalClass("Scavenger"))
        {
            return PerksConfig.PackMuleScavCarryBonus?.Value ?? 0f;
        }

        if (PerksConfig.PackMuleTankEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank"))
        {
            return PerksConfig.PackMuleTankCarryBonus?.Value ?? 0f;
        }

        // 079 — Light Frame (Caçador + Furtivo): limite de carga REDUZIDO. Bônus NEGATIVO → o Postfix aplica
        // como TETO (não piso), senão um piso < 1 nunca morderia (o modifier vanilla é ≥ 1).
        if (PerksConfig.LightFrameEnabled?.Value == true
            && (SkillMultipliers.IsLocalClass("Hunter") || SkillMultipliers.IsLocalClass("Stealth")))
        {
            return PerksConfig.LightFrameCarryPenalty?.Value ?? 0f;
        }

        return null;
    }
}

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
            var bonus = PackMule.LocalBonus();
            if (bonus == null)
            {
                return;   // classe sem Pack Mule (ou desabilitado)
            }

            // Na raid: só o SkillManager do MainPlayer local (não bufar bots).
            // Fora da raid (stash/menu — GameWorld null, sem bots): gateia só pela classe →
            // o limite de peso no stash já reflete o +30%.
            var gw = Singleton<GameWorld>.Instance;
            if (gw != null && !ReferenceEquals(__instance, gw.MainPlayer?.Skills))
            {
                return;
            }

            var target = 1f + bonus.Value;
            if (bonus.Value >= 0f)
            {
                if (__result < target) __result = target;   // PISO (Pack Mule: garante o +bônus, não soma)
            }
            else
            {
                if (__result > target) __result = target;   // TETO (079 Light Frame: reduz o limite de carga)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Pack Mule falhou: {ex.Message}");
        }
    }
}
