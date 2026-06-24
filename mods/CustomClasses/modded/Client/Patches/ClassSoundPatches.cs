using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.4 — som emitido pelo player.
///     <c>Player.method_67</c> = funil do RAIO de audibilidade de TODO som de movimento
///     (passos/gear/sprint/turn/prone) → multiplica o quão longe inimigos te ouvem. Gate: MainPlayer local.
///     🔧 Ghost Step (Furtivo) ×0.4 (mais silencioso) · 🔻 Loud Operator (Fuzileiro) ×1.3 (mais alto).
/// </summary>
internal class SoundRadiusPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), "method_67");
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, ref float __result)
    {
        try
        {
            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;
            }

            // 🔧 Ghost Step (Furtivo): reduz o raio de audibilidade.
            if (PerksConfig.GhostStepEnabled?.Value == true && SkillMultipliers.IsLocalClass("Stealth"))
            {
                __result *= PerksConfig.GhostStepSoundRadius?.Value ?? 1f;
            }

            // 🔻 Loud Operator (Fuzileiro): aumenta o raio de audibilidade.
            if (PerksConfig.LoudOperatorEnabled?.Value == true && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                __result *= PerksConfig.LoudOperatorSoundRadius?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] sound radius falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Silent Looter (Saqueador) — sons de interação/loot (abrir container/porta/zíper) mais baixos.
///     Prefix em <c>Player.PlayInteractionSound(clip, volume, …)</c> (só dispara em 1ª pessoa = player local).
/// </summary>
internal class InteractionSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.PlayInteractionSound));
    }

    [PatchPrefix]
    private static void Prefix(Player __instance, ref float volume)
    {
        try
        {
            if (PerksConfig.SilentLooterEnabled?.Value != true)
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;
            }

            if (SkillMultipliers.IsLocalClass("Scavenger"))
            {
                volume *= PerksConfig.SilentLooterVolume?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] interaction sound falhou: {ex.Message}");
        }
    }
}
