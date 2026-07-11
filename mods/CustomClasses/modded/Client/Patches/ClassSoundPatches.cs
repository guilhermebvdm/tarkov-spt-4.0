using System;
using System.Linq.Expressions;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     🔧 Ruído de movimento REDUZIDO por classe — 👻 Ghost Step (Furtivo, ×0.70 = −30%) e
///     🎯 Stalker (Caçador, ×0.80 = −20%; 2026-07-11). O Furtivo segue sendo o dono da furtividade;
///     o Caçador ganha uma versão mais fraca (espreitar a presa). Retorna o multiplicador da classe LOCAL
///     (1 = sem efeito). Usado pelos 3 pipelines de som (rolloff do player, IA base, SAIN) — mesma forma do
///     <see cref="LoudOperator"/>, que é o oposto (aumenta o raio).
///     ⚠️ Coop: som é host-only vs bots (B14) — um CLIENTE Fika não afeta a percepção da IA (ela vive no host).
/// </summary>
internal static class QuietStep
{
    internal static float Mult()
    {
        if (SkillMultipliers.IsLocalClass("Stealth"))
        {
            return PerksConfig.GhostStepEnabled?.Value == true
                ? (PerksConfig.GhostStepSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsLocalClass("Hunter"))
        {
            return PerksConfig.StalkerEnabled?.Value == true
                ? (PerksConfig.StalkerSoundRadius?.Value ?? 1f)
                : 1f;
        }

        return 1f;
    }
}

/// <summary>
///     🔻 Loud Operator (Fuzileiro + Tanque) — raio de audibilidade dos sons de movimento. Desdobrado por classe
///     (2026-07-10): cada classe tem config própria no F12. Retorna o multiplicador da classe LOCAL (1 = sem efeito).
///     Usado pelos 3 pipelines de som (rolloff do player, IA base, SAIN).
/// </summary>
internal static class LoudOperator
{
    internal static float Mult()
    {
        if (SkillMultipliers.IsLocalClass("Rifleman"))
        {
            return PerksConfig.LoudOperatorRiflemanEnabled?.Value == true
                ? (PerksConfig.LoudOperatorRiflemanSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsLocalClass("Tank"))
        {
            return PerksConfig.LoudOperatorTankEnabled?.Value == true
                ? (PerksConfig.LoudOperatorTankSoundRadius?.Value ?? 1f)
                : 1f;
        }

        return 1f;
    }
}

/// <summary>
///     Item 050.4 — som emitido pelo player.
///     <c>Player.method_67</c> = funil do RAIO de audibilidade de TODO som de movimento
///     (passos/gear/sprint/turn/prone) → multiplica o quão longe inimigos te ouvem. Gate: MainPlayer local.
///     🔧 Ghost Step (Furtivo) ×0.4 (mais silencioso) · 🔻 Loud Operator (Fuzileiro + Tanque, 2026-07-05) ×1.3.
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

            // 🔧 Ghost Step (Furtivo −30%) / Stalker (Caçador −20%): reduz o raio de audibilidade.
            __result *= QuietStep.Mult();

            // 🔻 Loud Operator (Fuzileiro + Tanque, desdobrado por classe): aumenta o raio de audibilidade.
            __result *= LoudOperator.Mult();

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

            power *= QuietStep.Mult();
            power *= LoudOperator.Mult();

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

/// <summary>
///     Item 050.4 (SAIN 2026-06-24) — audibilidade do player local PARA A IA quando o SAIN está ativo.
///     O SAIN tem um pipeline de percepção PRÓPRIO (não passa pelo `BotEventHandler` base) que registra passo
///     + AÇÕES (recarregar, curar, comer, lootar, porta, gear…). Prefix em
///     <c>PlayerComponent.PlayAISound(SAINSoundType, Vector3, InRange, InVolume, …)</c> multiplica o <c>InRange</c>
///     (alcance que o bot ouve). Via REFLECTION (SAIN não é ref de compile-time → no-op se ausente).
///     Ghost Step reduz TODOS os tipos · Loud Operator aumenta TODOS · Silent Looter reduz só Looting(=5).
///     ⚠️ Coop: gate por ProfileId (só você); peer remoto emite o próprio som no host mas o gate barra (gap 057).
/// </summary>
internal class SainSoundPatch : ModulePatch
{
    private const int SainSoundTypeLooting = 5;
    private static Func<object, string>? _getProfileId;   // getter compilado 1× — tira o reflection do hot-path (review)

    protected override MethodBase? GetTargetMethod()
    {
        var t = AccessTools.TypeByName("SAIN.Components.PlayerComponentSpace.PlayerComponent");
        if (t == null)
        {
            return null;   // SAIN ausente (a habilitação no Plugin já checa antes)
        }

        var getter = AccessTools.Property(t, "ProfileId")?.GetGetMethod();
        if (getter != null)
        {
            var pc = Expression.Parameter(typeof(object), "pc");
            _getProfileId = Expression.Lambda<Func<object, string>>(
                Expression.Call(Expression.Convert(pc, t), getter), pc).Compile();
        }

        return AccessTools.Method(t, "PlayAISound");
    }

    [PatchPrefix]
    private static void Prefix(object __instance, object __0, ref float __2)
    {
        try
        {
            var mp = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mp == null || _getProfileId == null)
            {
                return;
            }

            // gate: só o player local (ProfileId) — não afeta peers coop nem bots. Getter compilado (sem reflection).
            if (_getProfileId(__instance) != mp.ProfileId)
            {
                return;
            }

            var before = __2;
            var soundType = Convert.ToInt32(__0);

            __2 *= QuietStep.Mult();      // reduz TODOS (Ghost Step / Stalker)
            __2 *= LoudOperator.Mult();   // aumenta TODOS

            if (soundType == SainSoundTypeLooting
                && PerksConfig.SilentLooterEnabled?.Value == true && SkillMultipliers.IsLocalClass("Scavenger"))
            {
                __2 *= PerksConfig.SilentLooterVolume?.Value ?? 1f;   // anti-detecção: reduz só o Looting
            }

            if (PerkDiag.Enabled)
            {
                PerkDiag.SainBefore = before;
                PerkDiag.SainAfter = __2;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] SAIN sound falhou: {ex.Message}");
        }
    }
}
