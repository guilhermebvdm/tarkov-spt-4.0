using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.0 — Bulwark (🛡️ Tanque): dano recebido na vida ×0.85 (-15%).
///     Prefix em <c>Player.ApplyDamageInfo</c> — escala <c>damageInfo.Damage</c> de entrada
///     (a armadura absorve do valor já escalado → a perda de HP final cai ~15%, proporcional).
///     Gating: SÓ o player local (MainPlayer) E classe = Tank. Nunca afeta bots/remotos.
///     Lê o valor do F12 no apply-time (F12-live).
///     <para>
///     <b>B6 (balance 2026-07-11) — Couraça CONDICIONAL:</b> antes o −15% era INCONDICIONAL (valia até pelado,
///     o que o board apontou como o pacote mais forte do mod). Agora exige estar de fato BLINDADO: alguma
///     armadura equipada com classe &gt;= <c>Bulwark — Min armor class</c> (default 4 = colete pesado).
///     Sem ela, dano normal (×1.0). Temático, counterável, e casa com o `HeavyVests ×2` que a classe treina.
///     Detecção pelo método CANÔNICO do EFT: <c>Inventory.GetPutOnArmorsNonAlloc(list)</c> — o mesmo que o
///     próprio <c>Player.ApplyExplosionDamageToArmor</c> usa (ref: Player.cs:30037) e que o
///     <c>PlayerAIDataClass.method_9</c> usa para somar a armadura do player. Versão NonAlloc + buffer estático:
///     o <c>ApplyDamageInfo</c> roda a cada dano, então não alocamos por hit.
///     </para>
/// </summary>
internal class BulwarkPatch : ModulePatch
{
    // Buffer reusado (main thread — Harmony prefix). Espelha o `_preAllocatedArmorComponents` do próprio Player.
    private static readonly List<ArmorComponent> ArmorBuffer = new();

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPrefix]
    private static void Prefix(Player __instance, ref DamageInfoStruct damageInfo)
    {
        try
        {
            if (PerksConfig.BulwarkEnabled?.Value != true)
            {
                return;
            }

            // Só o player local (não bots/remotos).
            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;
            }

            if (!SkillMultipliers.IsLocalClass("Tank"))
            {
                return;
            }

            var mult = PerksConfig.BulwarkDamageTaken?.Value ?? 1f;
            if (mult >= 1f)
            {
                return;   // sem redução configurada
            }

            // B6: sem armadura pesada equipada → sem Couraça.
            if (PerksConfig.BulwarkRequireHeavyArmor?.Value == true && !HasHeavyArmor(__instance))
            {
                return;
            }

            damageInfo.Damage *= mult;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Bulwark falhou: {ex.Message}");
        }
    }

    /// <summary>B6 — alguma armadura EQUIPADA com classe &gt;= o mínimo do F12? (colete, rig blindado, placas…)</summary>
    private static bool HasHeavyArmor(Player player)
    {
        var min = PerksConfig.BulwarkMinArmorClass?.Value ?? 4;
        var inventory = player.Inventory;
        if (inventory is null)
        {
            return false;
        }

        ArmorBuffer.Clear();
        inventory.GetPutOnArmorsNonAlloc(ArmorBuffer);   // ref: Player.cs:30037 (mesmo método do EFT)
        foreach (var armor in ArmorBuffer)
        {
            if (armor is not null && armor.ArmorClass >= min)
            {
                return true;
            }
        }

        return false;
    }
}
