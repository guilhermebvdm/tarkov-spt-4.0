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
        // Desambigua o overload (fix review): existe method_67() sem-args de outra classe; queremos o de áudio.
        return AccessTools.Method(typeof(Player), "method_67",
            new[] { typeof(CommonAssets.Scripts.Audio.EAudioMovementState), typeof(bool) });
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

            var r0 = __result;   // (052) baseline p/ o diagnóstico

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

            if (PerkDiag.Enabled)
            {
                PerkDiag.AudioBefore = r0;
                PerkDiag.AudioAfter = __result;
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

/// <summary>
///     Item 050.4 (fix 2026-06-24) — audibilidade do player PARA A IA (bots).
///     O <c>SoundRadiusPatch</c> (method_67) só mexe no rolloff de ÁUDIO que VOCÊ ouve; a percepção do bot
///     vem de outro pipeline: <c>MovementContext</c> dispara <c>BotEventHandler.PlaySound(person, pos, power, step)</c>
///     e o <c>power</c> escala o raio de detecção do <c>BotHearingSensor</c>. Aqui multiplicamos o power do
///     passo/pulo do MainPlayer local (mesmos F12 do Ghost Step / Loud Operator).
///     ⚠️ Coop: só cobre o player LOCAL (host/IsYourPlayer). Passo de peer remoto precisa de sync — gap registrado.
/// </summary>
internal class AiSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotEventHandler), nameof(BotEventHandler.PlaySound));
    }

    [PatchPrefix]
    private static void Prefix(IPlayer person, ref float power, AISoundType type)
    {
        try
        {
            if (type != AISoundType.step || !(person is Player p) || !p.IsYourPlayer)
            {
                return;
            }

            var p0 = power;

            if (PerksConfig.GhostStepEnabled?.Value == true && SkillMultipliers.IsLocalClass("Stealth"))
            {
                power *= PerksConfig.GhostStepSoundRadius?.Value ?? 1f;
            }

            if (PerksConfig.LoudOperatorEnabled?.Value == true && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                power *= PerksConfig.LoudOperatorSoundRadius?.Value ?? 1f;
            }

            if (PerkDiag.Enabled)
            {
                PerkDiag.AiPowerBefore = p0;
                PerkDiag.AiPowerAfter = power;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] AI sound falhou: {ex.Message}");
        }
    }
}
