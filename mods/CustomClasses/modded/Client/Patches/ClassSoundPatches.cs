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
    /// <summary>Multiplicador do player LOCAL (atalho — pipelines que só valem para você, ex.: o rolloff que você ouve).</summary>
    internal static float Mult() => MultFor(SkillMultipliers.ClassNameEn);

    /// <summary>
    ///     B14 — multiplicador de UMA classe (por nome EN). Permite ao HOST aplicar o perk de som de um peer
    ///     Fika (a IA vive no host), em vez de só o do player local. Classe desconhecida/vanilla → 1 (sem efeito).
    /// </summary>
    internal static float MultFor(string? classNameEn)
    {
        if (SkillMultipliers.IsClass(classNameEn, "Stealth"))
        {
            return PerksConfig.GhostStepEnabled?.Value == true
                ? (PerksConfig.GhostStepSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsClass(classNameEn, "Hunter"))
        {
            return PerksConfig.StalkerEnabled?.Value == true
                ? (PerksConfig.StalkerSoundRadius?.Value ?? 1f)
                : 1f;
        }

        return 1f;
    }
}

/// <summary>
///     🔧 Silent Looter (Saqueador) — som de interação/loot. B14: resolvido POR CLASSE, para o host poder aplicar
///     o perk de um Saqueador remoto no pipeline de percepção do SAIN (o de interação local segue no
///     <see cref="InteractionSoundPatch"/>, que é 1ª pessoa e portanto sempre local).
/// </summary>
internal static class SilentLooter
{
    internal static float MultFor(string? classNameEn)
    {
        return SkillMultipliers.IsClass(classNameEn, "Scavenger") && PerksConfig.SilentLooterEnabled?.Value == true
            ? (PerksConfig.SilentLooterVolume?.Value ?? 1f)
            : 1f;
    }
}

/// <summary>
///     🔻 Loud Operator (Fuzileiro + Tanque) — raio de audibilidade dos sons de movimento. Desdobrado por classe
///     (2026-07-10): cada classe tem config própria no F12. Retorna o multiplicador da classe LOCAL (1 = sem efeito).
///     Usado pelos 3 pipelines de som (rolloff do player, IA base, SAIN).
/// </summary>
internal static class LoudOperator
{
    /// <summary>Multiplicador do player LOCAL (atalho).</summary>
    internal static float Mult() => MultFor(SkillMultipliers.ClassNameEn);

    /// <summary>B14 — multiplicador de UMA classe (por nome EN), para o host aplicar o de um peer Fika.</summary>
    internal static float MultFor(string? classNameEn)
    {
        if (SkillMultipliers.IsClass(classNameEn, "Rifleman"))
        {
            return PerksConfig.LoudOperatorRiflemanEnabled?.Value == true
                ? (PerksConfig.LoudOperatorRiflemanSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsClass(classNameEn, "Tank"))
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
///     e o <c>power</c> escala o raio de detecção do <c>BotHearingSensor</c>.
///     <para>
///     <b>B14 (coop 2026-07-11) — som host-side para REMOTOS.</b> Antes gateávamos em <c>IsYourPlayer</c>, o que
///     tornava os perks de som um PLACEBO contra a IA para quem joga como CLIENTE Fika: os bots vivem no processo
///     do HOST, então é o host quem calcula o que eles ouvem — inclusive do barulho de um peer. Agora resolvemos
///     a classe de QUEM EMITIU o som (<see cref="ClassIdentities.ClassNameEnOf"/>: local via SkillMultipliers,
///     peer via o mapa nickname→classe da rota 057) e aplicamos o multiplicador DELA. Sem protocolo novo.
///     ⚠️ O VALOR sai do F12 de quem roda isto (o host) — ele é a autoridade da percepção da IA, que é dele.
///     ⚠️ Fica de fora o rolloff audível (<see cref="SoundRadiusPatch"/>, method_67): o som que VOCÊ ouve de um
///     peer exigiria sync real. Aqui só corrigimos a percepção da IA, que é o que muda o gameplay.
///     </para>
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
            if (type != AISoundType.step || person is not Player p)
            {
                return;   // só passo/pulo de PLAYER (bots não têm classe do mod)
            }

            // B14: a classe do EMISSOR (não a local) — é isto que faz o perk do peer valer contra a IA no host.
            var emitterClass = ClassIdentities.ClassNameEnOf(p);
            if (emitterClass is null)
            {
                return;   // vanilla/desconhecido → sem efeito
            }

            var p0 = power;

            power *= QuietStep.MultFor(emitterClass);
            power *= LoudOperator.MultFor(emitterClass);

            if (PerkDiag.Enabled && p.IsYourPlayer)   // o overlay (052) só descreve o SEU player
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
///     Ghost Step/Stalker reduzem TODOS os tipos · Loud Operator aumenta TODOS · Silent Looter reduz só Looting(=5).
///     <para>
///     <b>B14 (coop 2026-07-11):</b> antes o gate por ProfileId barrava peers — o peer emitia o som no host, mas
///     o perk dele era ignorado. Agora resolvemos o player pelo ProfileId (<c>GameWorld.GetAlivePlayerByProfileID</c>)
///     e aplicamos o multiplicador da classe DELE. Mesma lógica do <see cref="AiSoundPatch"/> (a IA do SAIN
///     também vive no host).
///     </para>
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
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || _getProfileId == null)
            {
                return;
            }

            // B14: o EMISSOR pode ser o player local OU um peer Fika (o SAIN roda no host, junto com os bots).
            // Getter compilado (sem reflection no hot-path).
            var emitter = gameWorld.GetAlivePlayerByProfileID(_getProfileId(__instance));   // ref: GameWorld.cs:1238
            var emitterClass = ClassIdentities.ClassNameEnOf(emitter);
            if (emitterClass is null)
            {
                return;   // bot, vanilla ou desconhecido → sem efeito
            }

            var before = __2;
            var soundType = Convert.ToInt32(__0);

            __2 *= QuietStep.MultFor(emitterClass);      // reduz TODOS (Ghost Step / Stalker)
            __2 *= LoudOperator.MultFor(emitterClass);   // aumenta TODOS

            if (soundType == SainSoundTypeLooting)
            {
                __2 *= SilentLooter.MultFor(emitterClass);   // anti-detecção: reduz só o Looting (Saqueador)
            }

            if (PerkDiag.Enabled && emitter is not null && emitter.IsYourPlayer)   // o overlay só descreve o SEU player
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
