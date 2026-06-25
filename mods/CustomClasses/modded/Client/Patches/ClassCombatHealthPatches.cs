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
///     🔻 Heavy Frame (Tanque) — fome/sede drenam mais rápido (×1.3). Helper compartilhado entre
///     <c>ActiveHealthController.ChangeEnergy</c> e <c>ChangeHydration</c>. Só amplifica o DRAIN
///     (value &lt; 0); restauração por comida/bebida (value &gt; 0) não é afetada.
/// </summary>
internal static class HeavyFrameMetabolism
{
    internal static void Apply(ActiveHealthController instance, ref float value)
    {
        try
        {
            if (PerksConfig.HeavyFrameEnabled?.Value != true || value >= 0f)
            {
                return;
            }

            if (!ReferenceEquals(instance.Player, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;   // só o player local
            }

            if (SkillMultipliers.IsLocalClass("Tank"))
            {
                value *= PerksConfig.HeavyFrameHungerThirst?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] heavy frame metabolism falhou: {ex.Message}");
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
///     🔧 Iron Lungs (Caçador) — segura a respiração por mais tempo (menos dreno de oxigênio).
///     Postfix em <c>PlayerPhysicalClass.method_12</c> (consumo de O₂ do hold-breath) → divide o consumo.
///     Gate: o physical do MainPlayer + Hunter. (método obfuscado → wiring em try/catch.)
/// </summary>
internal class IronLungsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PlayerPhysicalClass), "method_12");
    }

    [PatchPostfix]
    private static void Postfix(PlayerPhysicalClass __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.IronLungsEnabled?.Value != true)
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.Physical))
            {
                return;   // só o player local
            }

            if (SkillMultipliers.IsLocalClass("Hunter"))
            {
                var f = PerksConfig.IronLungsBreathDrain?.Value ?? 1f;
                if (f > 0f && f < 1f)
                {
                    __result *= f;   // 0.5 = metade do consumo de O₂ → ~2× o tempo de fôlego
                }
            }
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
                PerkDiag.LastMalfunction = $"{__result * 100f:F2}%";
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] anti-jam falhou: {ex.Message}");
        }
    }
}
