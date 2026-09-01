using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     ref: AUD-01-03 — branches de <c>Player.ApplyDamageInfo</c>, movidos SEM alteração de fórmula dos
///     QUATRO patches que consolidaram em <see cref="ClassDamagePatch"/>: <c>LocalHitTypePatch</c> (carimbo
///     do hit de combate) + <c>BulwarkPatch</c> (Couraça) + <c>ExecutionMeleePatch</c> (melee) nos Prefixes,
///     e <c>AdrenalineTriggerPatch</c> no Postfix.
///     <para>
///     ⚠️ PA-02-06: nenhum dos quatro tinha <c>[HarmonyPriority]</c> — consolidar não move fronteira de ordem
///     contra mods externos (diferente do <c>PWA.Shoot</c>, ver PA-01-01).
///     </para>
///     <para>
///     ⚠️ <b>A ORDEM DO PRIMEIRO BRANCH É CONTRATO:</b> o carimbo do hit de combate tem de rodar ANTES de
///     tudo, porque o <c>AimPunchPatch</c> (alvo diferente — <c>ForceEffector.AddForce</c>, invocado a jusante
///     no mesmo frame) lê o timestamp por RECÊNCIA para distinguir dano de combate de dano de QUEDA.
///     </para>
/// </summary>
internal static class DamageBranches
{
    /// <summary>
    ///     (review fix 2026-06-24) Carimba o instante do último dano de COMBATE do player local, para o
    ///     <c>AimPunchPatch</c> (Rattled / Cool Under Fire) NÃO disparar em dano de QUEDA — que não passa
    ///     por <c>ApplyDamageInfo</c> (vai por <c>ActiveHealthController.ApplyDamage</c>), então o timestamp
    ///     fica velho e a janela de recência barra. ref: origem LocalHitTypePatch.
    /// </summary>
    internal static void StampCombatHit(Player instance, Player mainPlayer)
    {
        if (ReferenceEquals(instance, mainPlayer))
        {
            LocalHitState.LastCombatHitTime = UnityEngine.Time.time;
        }
    }

    /// <summary>🛡️ Couraça / Bulwark (Tanque) — dano recebido ×0.85. ref: origem BulwarkPatch.</summary>
    internal static void Bulwark(Player instance, Player mainPlayer, ref DamageInfoStruct damageInfo)
    {
        if (PerksConfig.BulwarkEnabled?.Value != true)
        {
            return;
        }

        if (!ReferenceEquals(instance, mainPlayer))
        {
            return;   // só o player local (não bots/remotos)
        }

        if (!SkillMultipliers.IsLocalClass(EClassId.Tank))
        {
            return;
        }

        var mult = PerksConfig.BulwarkDamageTaken?.Value ?? 1f;
        if (mult >= 1f)
        {
            return;   // sem redução configurada
        }

        // B6: sem armadura pesada de TRONCO equipada → sem Couraça.
        if (PerksConfig.BulwarkRequireHeavyArmor?.Value == true && !BulwarkArmor.HasHeavyArmor(instance))
        {
            return;
        }

        damageInfo.Damage *= mult;
    }

    /// <summary>
    ///     🔧 Execution (Furtivo) — dano de melee ×3.5 (B7) quando o ATACANTE é o player local.
    ///     (O dano melee é construído em BaseKnifeController.vmethod_0 com DamageType=Melee.)
    ///     ref: origem ExecutionMeleePatch.
    /// </summary>
    internal static void ExecutionMelee(Player mainPlayer, ref DamageInfoStruct damageInfo)
    {
        if (PerksConfig.ExecutionMeleeEnabled?.Value != true || damageInfo.DamageType != EDamageType.Melee)
        {
            return;
        }

        // Atacante = player local (damageInfo.Player é IPlayerOwner → comparar pelo ProfileId).
        if (damageInfo.Player?.iPlayer == null || damageInfo.Player.iPlayer.ProfileId != mainPlayer.ProfileId)
        {
            return;
        }

        if (SkillMultipliers.IsLocalClass(EClassId.Stealth))
        {
            damageInfo.Damage *= PerksConfig.ExecutionMeleeDamage?.Value ?? 1f;
        }
    }

    /// <summary>
    ///     🔧 Adrenaline (Fuzileiro) — gatilho: causar dano (atacante = local) OU receber dano (vítima =
    ///     local) abre/renova a janela. ref: origem AdrenalineTriggerPatch (era o único Postfix dos quatro).
    /// </summary>
    internal static void AdrenalineTrigger(Player instance, Player mainPlayer, DamageInfoStruct damageInfo)
    {
        if (PerksConfig.AdrenalineEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Rifleman))
        {
            return;
        }

        var dealt = damageInfo.Player?.iPlayer != null
                    && damageInfo.Player.iPlayer.ProfileId == mainPlayer.ProfileId;
        if (ReferenceEquals(instance, mainPlayer) || dealt)
        {
            AdrenalineState.Trigger();
            AdrenalineState.EnsureReloadResync();   // 085: re-sync do reload no open/close da janela
        }
    }
}

/// <summary>
///     ref: AUD-01-03 — patch consolidado de <c>Player.ApplyDamageInfo</c> (4 patches → 1 Prefix + 1 Postfix).
///     O gate (<c>MainPlayer</c>) era resolvido QUATRO vezes por evento de dano de qualquer entidade do mapa;
///     agora é uma vez por Prefix e uma por Postfix.
/// </summary>
internal class ClassDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPrefix]
    private static void Prefix(Player __instance, ref DamageInfoStruct damageInfo)
    {
        var mp = Singleton<GameWorld>.Instance?.MainPlayer;
        if (mp == null)
        {
            return;   // GATE ÚNICO (era resolvido 3× nos Prefixes)
        }

        // PERF-INSTR AUD-01-03 — temporary, remove after validation
        if (PerkDiag.Enabled)
        {
            PerfCount.DamageCalls++;
            PerfCount.DamageGates++;
        }

        // ⚠️ ORDEM: o carimbo vem PRIMEIRO (contrato com o AimPunchPatch — ver DamageBranches).
        // ref: PA-02-01 — try/catch por branch, nunca um externo único.
        try { DamageBranches.StampCombatHit(__instance, mp); } catch (Exception ex) { BranchFailLog.Once("dmg/stamp", ex); }
        try { DamageBranches.Bulwark(__instance, mp, ref damageInfo); } catch (Exception ex) { BranchFailLog.Once("dmg/bulwark", ex); }
        try { DamageBranches.ExecutionMelee(mp, ref damageInfo); } catch (Exception ex) { BranchFailLog.Once("dmg/execution", ex); }
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, DamageInfoStruct damageInfo)
    {
        // ⚠️ ref: CR-01-02 — ORDEM BARATO→CARO. O único branch do Postfix é a Adrenalina, então o teste de
        // config + classe (1 deref + 1 compare de int) vem ANTES de resolver o Singleton/MainPlayer. Para
        // quem não é Fuzileiro — a maioria dos perfis — isso sai na primeira linha, e este Postfix roda em
        // TODO evento de dano de QUALQUER entidade do mapa. A consolidação tinha invertido essa ordem.
        if (PerksConfig.AdrenalineEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Rifleman))
        {
            return;
        }

        var mp = Singleton<GameWorld>.Instance?.MainPlayer;
        if (mp == null)
        {
            return;
        }

        // PERF-INSTR AUD-01-03 — temporary, remove after validation
        if (PerkDiag.Enabled)
        {
            PerfCount.DamageGates++;
        }

        try { DamageBranches.AdrenalineTrigger(__instance, mp, damageInfo); } catch (Exception ex) { BranchFailLog.Once("dmg/adrenaline", ex); }
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

            if (PerksConfig.HeavyFrameEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Tank))
            {
                value *= PerksConfig.HeavyFrameHungerThirst?.Value ?? 1f;
            }
            else if (PerksConfig.EfficientMetabolismEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.CombatMedic))
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

            var on = PerksConfig.IronLungsEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Hunter);
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

            if (SkillMultipliers.IsLocalClass(EClassId.Rifleman))
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
