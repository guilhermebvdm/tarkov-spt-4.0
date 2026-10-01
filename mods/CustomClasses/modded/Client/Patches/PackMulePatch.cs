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
        if (PerksConfig.PackMuleScavEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Scavenger))
        {
            return PerksConfig.PackMuleScavCarryBonus?.Value ?? 0f;
        }

        if (PerksConfig.PackMuleTankEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Tank))
        {
            return PerksConfig.PackMuleTankCarryBonus?.Value ?? 0f;
        }

        // 079 — Light Frame (Caçador + Furtivo): limite de carga REDUZIDO. Bônus NEGATIVO → o Postfix MULTIPLICA
        // o modifier (−X% RELATIVO, preserva o Strength), não teto absoluto (v0.16.6 — ver PackMulePatch.Postfix).
        if (PerksConfig.LightFrameEnabled?.Value == true
            && (SkillMultipliers.IsLocalClass(EClassId.Hunter) || SkillMultipliers.IsLocalClass(EClassId.Stealth)))
        {
            return PerksConfig.LightFrameCarryPenalty?.Value ?? -0.1f;
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

            if (bonus.Value >= 0f)
            {
                var floor = 1f + bonus.Value;
                if (__result < floor) __result = floor;   // PISO (Pack Mule: garante o +bônus, não soma com Strength)
            }
            else
            {
                // 079 Light Frame — CORRIGIDO: −X% RELATIVO ao limite (MULTIPLICA), NÃO teto absoluto. O modifier
                // vanilla é 1 + StrengthBuffLiftWeightInc (≥ 1.0, até 1.30 no Strength elite); o teto absoluto anterior
                // (=1+bonus=0.9) SUBSTITUÍA o modifier inteiro, apagando TODO o bônus de Strength → como o limite de
                // overweight é linear no modifier (BasePhysicalClass.UpdateWeightLimits: base×modifier+offset), o limite
                // despencava −25%..−31% (pior quanto maior a Strength) em vez de −10%, e a bigorna acendia bem antes do
                // esperado (report in-game). Multiplicar dá −10% REAL p/ qualquer Strength (ex.: Str40 1.24→1.116) e é
                // ordem-independente. (ref: reference_spt_init_before_mainplayer — OnWeightLimitsUpdated re-aplica no raid-start)
                __result *= 1f + bonus.Value;   // bonus −0.1 → ×0.9
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Pack Mule falhou: {ex.Message}");
        }
    }
}
