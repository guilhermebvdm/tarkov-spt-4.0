using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     🔧 Execution (Furtivo) — dano de melee ×5 quando o ATACANTE é o player local.
///     Prefix em <c>Player.ApplyDamageInfo</c> (mesma infra do Bulwark/Adrenaline): escala
///     <c>damageInfo.Damage</c> de entrada se for golpe de melee partindo do MainPlayer.
///     (O dano melee é construído em BaseKnifeController.vmethod_0 com DamageType=Melee.)
/// </summary>
internal class ExecutionMeleePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPrefix]
    private static void Prefix(ref DamageInfoStruct damageInfo)
    {
        try
        {
            if (PerksConfig.ExecutionMeleeEnabled?.Value != true
                || damageInfo.DamageType != EDamageType.Melee)
            {
                return;
            }

            var mp = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mp == null)
            {
                return;
            }

            // Atacante = player local (damageInfo.Player é IPlayerOwner → comparar pelo ProfileId).
            if (damageInfo.Player?.iPlayer == null || damageInfo.Player.iPlayer.ProfileId != mp.ProfileId)
            {
                return;
            }

            if (SkillMultipliers.IsLocalClass("Stealth"))
            {
                damageInfo.Damage *= PerksConfig.ExecutionMeleeDamage?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] execution melee falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Lever de metabolismo (fome/sede) por classe. Helper compartilhado entre <c>ActiveHealthController.ChangeEnergy</c>
///     e <c>ChangeHydration</c>. Só toca o DRAIN (value &lt; 0); restauração por comida/bebida (value &gt; 0) fica intocada.
///     Branch por classe (mutuamente exclusivas): 🔻 Heavy Frame (Tanque) drena mais rápido (×1.3) ·
///     🩺 Efficient Metabolism (Médico, B17) drena mais devagar (×0.85).
/// </summary>
internal static class HeavyFrameMetabolism
{
    internal static void Apply(ActiveHealthController instance, ref float value)
    {
        try
        {
            if (value >= 0f)
            {
                return;   // só o DRAIN
            }

            if (!ReferenceEquals(instance.Player, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;   // só o player local
            }

            if (PerksConfig.HeavyFrameEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank"))
            {
                value *= PerksConfig.HeavyFrameHungerThirst?.Value ?? 1f;
            }
            else if (PerksConfig.EfficientMetabolismEnabled?.Value == true && SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                value *= PerksConfig.EfficientMetabolismHungerThirst?.Value ?? 1f;   // B17
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] metabolism drain falhou: {ex.Message}");
        }
    }
}

internal class ChangeEnergyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.ChangeEnergy));
    }

    [PatchPrefix]
    private static void Prefix(ActiveHealthController __instance, ref float value)
    {
        HeavyFrameMetabolism.Apply(__instance, ref value);
    }
}

internal class ChangeHydrationPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.ChangeHydration));
    }

    [PatchPrefix]
    private static void Prefix(ActiveHealthController __instance, ref float value)
    {
        HeavyFrameMetabolism.Apply(__instance, ref value);
    }
}

/// <summary>
///     🔧 Iron Lungs (Caçador) — segura a respiração por mais tempo (menos dreno de O₂).
///     LEVER CORRETO (recon 2026-06-24): o dreno vivo é <c>Oxygen.Process</c> lendo o <c>Delta</c> do consumption
///     HoldBreath = lambda <c>BaseHoldBreathConsumption × …</c> (sem cache → relido todo frame). Reduzimos o campo
///     de instância <c>PlayerPhysicalClass.BaseHoldBreathConsumption</c>. Postfix em <c>HoldBreath(enable)</c>
///     (dispara ao começar/parar de segurar). Gate: physical do MainPlayer + Hunter.
///     ⚠️ Idempotência: campo persistente por-raid → cacheia o valor-base original (por instância) e SETA
///     (<c>= base × fator</c>), nunca <c>×=</c> cru (empilharia). O physical é recriado a cada raid → re-captura.
/// </summary>
internal class IronLungsPatch : ModulePatch
{
    private static PlayerPhysicalClass? _lastPhysical;
    private static float _originalBase;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PlayerPhysicalClass), nameof(PlayerPhysicalClass.HoldBreath));
    }

    [PatchPostfix]
    private static void Postfix(PlayerPhysicalClass __instance)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.Physical))
            {
                return;   // só o player local
            }

            // captura o valor-base 1× por instância de physical (nova raid = novo physical → re-captura).
            if (!ReferenceEquals(_lastPhysical, __instance))
            {
                _lastPhysical = __instance;
                _originalBase = __instance.BaseHoldBreathConsumption;
            }

            var on = PerksConfig.IronLungsEnabled?.Value == true && SkillMultipliers.IsLocalClass("Hunter");
            var f = on ? (PerksConfig.IronLungsBreathDrain?.Value ?? 1f) : 1f;
            __instance.BaseHoldBreathConsumption = _originalBase * f;   // SETA (idempotente), não ×=
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] iron lungs falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Cool Under Fire (Fuzileiro) — anti-jam: ×0.5 na chance de travamento da arma.
///     Postfix em <c>FirearmController.GetTotalMalfunctionChance</c> (o único funil da chance, lido
///     pelo roll de <c>GetMalfunctionState</c>). Gate: a arma atual do MainPlayer.
/// </summary>
internal class MalfunctionChancePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.GetTotalMalfunctionChance));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.CoolUnderFireEnabled?.Value != true)
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só a arma do player local
            }

            if (SkillMultipliers.IsLocalClass("Rifleman"))
            {
                __result *= PerksConfig.CoolUnderFireMalfChance?.Value ?? 1f;
            }

            if (PerkDiag.Enabled)
            {
                PerkDiag.MalfChance = __result;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] anti-jam falhou: {ex.Message}");
        }
    }
}
