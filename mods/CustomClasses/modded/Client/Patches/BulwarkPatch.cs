using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.0 — Bulwark (🛡️ Tanque): dano recebido na vida ×0.85 (-15%).
///     Prefix em <c>Player.ApplyDamageInfo</c> — escala <c>damageInfo.Damage</c> de entrada
///     (a armadura absorve do valor já escalado → a perda de HP final cai ~15%, proporcional).
///     Gating: SÓ o player local (MainPlayer) E classe = Tank. Nunca afeta bots/remotos.
///     Lê o valor do F12 no apply-time (F12-live).
/// </summary>
internal class BulwarkPatch : ModulePatch
{
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
            if (mult < 1f)
            {
                damageInfo.Damage *= mult;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Bulwark falhou: {ex.Message}");
        }
    }
}
