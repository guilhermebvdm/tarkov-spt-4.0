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
///     Item 050.0 — Bulwark (🛡️ Tanque): dano recebido na vida ×0.85 (-15%) — CONDICIONAL desde o B6 (ver abaixo).
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

    /// <summary>
    ///     B6 — há armadura de TRONCO equipada com classe &gt;= o mínimo do F12?
    ///     <para>
    ///     ⚠️ (code-review 2026-07-11) <c>GetPutOnArmorsNonAlloc</c> devolve TODO <c>ArmorComponent</c> equipado —
    ///     inclusive <b>capacete e visor</b> (ref: BotNightVisionData.cs:68 lê o <c>ArmorComponent</c> do Headwear;
    ///     PlayerAIDataClass.method_9 SOMA tudo = "blindagem total do player"). Aceitar qualquer um deles deixaria
    ///     um Altyn/Vulkan cl.5-6 conceder a Couraça ao CORPO com o tronco pelado — exatamente o build que o B6
    ///     existe para matar. Por isso filtramos por PERTENCIMENTO: só conta a armadura que está sob o colete
    ///     (<c>ArmorVest</c>) ou o rig (<c>TacticalVest</c>).
    ///     </para>
    ///     <para>
    ///     Cobre os dois formatos: colete MONOLÍTICO (o <c>ArmorComponent</c> vive no próprio item) e PLATE CARRIER
    ///     (o portador é cl.0 e a classe está nas PLACAS/inserts aninhados — a subida de hierarquia os alcança).
    ///     Fail-closed: sem colete nem rig, não há Couraça.
    ///     </para>
    /// </summary>
    internal static bool HasHeavyArmor(Player player)   // internal: o PerkDiagnostics (052) mostra a condição
    {
        var min = PerksConfig.BulwarkMinArmorClass?.Value ?? 4;
        var inventory = player.Inventory;
        var equipment = inventory?.Equipment;
        if (inventory is null || equipment is null)
        {
            return false;
        }

        // Peças de TRONCO (ref: BotDeadBodyWork.cs:282 — mesmo idioma de acesso ao slot).
        var vest = equipment.GetSlot(EquipmentSlot.ArmorVest)?.ContainedItem;
        var rig = equipment.GetSlot(EquipmentSlot.TacticalVest)?.ContainedItem;
        if (vest is null && rig is null)
        {
            return false;   // tronco desprotegido → sem Couraça
        }

        ArmorBuffer.Clear();
        inventory.GetPutOnArmorsNonAlloc(ArmorBuffer);   // ref: Player.cs:30037 (append — por isso o Clear antes)
        foreach (var armor in ArmorBuffer)
        {
            if (armor is null || armor.ArmorClass < min)
            {
                continue;
            }

            var item = armor.Item;   // ref: Player.cs:30166 (armorComponent.Item)
            if (item is not null && (IsUnder(item, vest) || IsUnder(item, rig)))
            {
                return true;   // armadura de tronco (o próprio colete/rig ou uma placa dentro dele)
            }
        }

        return false;
    }

    /// <summary>
    ///     O item é <paramref name="root"/> ou descende dele? Sobe a hierarquia por
    ///     <c>Parent.Container.ParentItem</c> (ref: GClass2181.cs:130). Limite de profundidade = guarda contra
    ///     hierarquia cíclica/corrompida.
    /// </summary>
    private static bool IsUnder(Item item, Item? root)
    {
        if (root is null)
        {
            return false;
        }

        var current = item;
        for (var depth = 0; depth < 8 && current is not null; depth++)
        {
            if (ReferenceEquals(current, root))
            {
                return true;
            }

            current = current.Parent?.Container?.ParentItem;
        }

        return false;
    }
}
