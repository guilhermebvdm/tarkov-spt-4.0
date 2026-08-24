using System;
using System.Collections.Generic;   // B20 — Dictionary do _cachedMovementRolloff
using System.Linq.Expressions;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using EAudioMovementState = CommonAssets.Scripts.Audio.EAudioMovementState;

namespace CustomClasses.Client;

/// <summary>
///     🔧 Ruído de movimento REDUZIDO por classe — 👻 Ghost Step (Furtivo, ×0.70 = −30%) e
///     🎯 Stalker (Caçador, ×0.80 = −20%; 2026-07-11). O Furtivo segue sendo o dono da furtividade;
///     o Caçador ganha uma versão mais fraca (espreitar a presa). Retorna o multiplicador da classe LOCAL
///     (1 = sem efeito). Usado pelos 3 pipelines de som (rolloff do player, IA base, SAIN) — mesma forma do
///     <see cref="LoudOperator"/>, que é o oposto (aumenta o raio).
///     ✅ Coop: DESDE O B14 (2026-07-11) o host aplica o perk de som de um peer Fika contra a IA (via
///     <see cref="ClassIdentities.ClassIdOf"/>) — deixou de ser host-only. Ver <see cref="AiSoundPatch"/>.
/// </summary>
internal static class QuietStep
{
    /// <summary>
    ///     Multiplicador de UMA classe (por nome EN). Classe desconhecida/vanilla → 1 (sem efeito).
    ///     <para>
    ///     B14 destravou a percepção da IA (o host aplica o perk do peer que EMITIU o som); B20 fez o mesmo com o
    ///     rolloff que você OUVE. Com isso <b>todos</b> os call-sites passaram a resolver a classe do emissor, e o
    ///     antigo atalho <c>Mult()</c> (classe local) ficou sem consumidor — removido em 2026-07-11.
    ///     </para>
    /// </summary>
    internal static float MultFor(EClassId classId)   // ref: AUD-01-02 — era string? classNameEn
    {
        if (SkillMultipliers.IsClass(classId, EClassId.Stealth))
        {
            return PerksConfig.GhostStepEnabled?.Value == true
                ? (PerksConfig.GhostStepSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsClass(classId, EClassId.Hunter))
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
    internal static float MultFor(EClassId classId)   // ref: AUD-01-02 — era string? classNameEn
    {
        // Scavenger: loot mais SILENCIOSO (perk). Rifleman: loot mais ALTO (079 — Saque Barulhento, drawback).
        // Resolve a classe de QUALQUER emissor (local ou peer via rota 057) → vale no pipeline coop/AI (SAIN).
        if (SkillMultipliers.IsClass(classId, EClassId.Scavenger) && PerksConfig.SilentLooterEnabled?.Value == true)
        {
            return PerksConfig.SilentLooterVolume?.Value ?? 1f;
        }
        if (SkillMultipliers.IsClass(classId, EClassId.Rifleman) && PerksConfig.LoudLooterEnabled?.Value == true)
        {
            return PerksConfig.LoudLooterVolume?.Value ?? 1f;
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
    /// <summary>Multiplicador de UMA classe (por nome EN) — ver <see cref="QuietStep.MultFor"/>. Vanilla → 1.</summary>
    internal static float MultFor(EClassId classId)   // ref: AUD-01-02 — era string? classNameEn
    {
        if (SkillMultipliers.IsClass(classId, EClassId.Rifleman))
        {
            return PerksConfig.LoudOperatorRiflemanEnabled?.Value == true
                ? (PerksConfig.LoudOperatorRiflemanSoundRadius?.Value ?? 1f)
                : 1f;
        }

        if (SkillMultipliers.IsClass(classId, EClassId.Tank))
        {
            return PerksConfig.LoudOperatorTankEnabled?.Value == true
                ? (PerksConfig.LoudOperatorTankSoundRadius?.Value ?? 1f)
                : 1f;
        }

        return 1f;
    }
}

/// <summary>
///     Item 050.4 + <b>B20 — rolloff de PEER (coop, 2026-07-11)</b>.
///     <para>
///     <c>Player.method_67</c> (Player.cs:28226) é o funil do RAIO de audibilidade de TODO som de movimento
///     (passo/gear/sprint/turn/prone): <c>rolloff = base × ProtagonistHearing × Physical.SoundRadius × multByMovement</c>.
///     🔧 Ghost Step (Furtivo ×0.70) / Stalker (Caçador ×0.80) · 🔻 Loud Operator (Fuzileiro + Tanque ×1.30).
///     </para>
///     <para>
///     <b>Era MainPlayer-only; agora vale para QUALQUER player</b> — e sem protocolo novo. O board dizia que o
///     rolloff de peer "exigiria sync real"; está ERRADO. Prova (fika-plugin):
///     <c>ObservedPlayer : FikaPlayer : LocalPlayer : Player</c> e
///     <c>ObservedPlayer.ProtagonistHearing => Max(1, BetterAudio.Instance.ProtagonistHearing + 1)</c>
///     (ObservedPlayer.cs:137) — ou seja, o <c>method_67</c> de um peer roda NO SEU CLIENTE, com a SUA audição.
///     O passo do peer sai por <c>Player.PlayStepSound</c> (público, SEM gate de local) → <c>method_66</c> →
///     <c>method_68(NestedStepSoundSource, method_67(...))</c>. Basta resolver a classe do EMISSOR
///     (<see cref="ClassIdentities.ClassIdOf"/>, que já barra bots) — mesma peça do B14.
///     ⚠️ O VALOR sai do F12 de quem ouve; sem sync de config entre peers (idem B14).
///     </para>
/// </summary>
internal class SoundRadiusPatch : ModulePatch
{
    /// <summary>
    ///     <b>O buraco que fazia o perk valer pela METADE</b> (achado no recon do B20). O <c>method_67</c> grava
    ///     <c>_cachedMovementRolloff[state]</c> ANTES de qualquer Postfix rodar (Player.cs:28233), e é ESSE CACHE —
    ///     não o retorno — que alimenta o <b>VOLUME</b> do passo:
    ///     <c>method_64 = method_69(state) / base × Single_1</c> e <c>method_69</c> lê o cache (Player.cs:28243).
    ///     Em <c>PlayStepSound</c> o <c>method_64</c> (volume) roda ANTES do <c>method_66</c> (rolloff).
    ///     <para>
    ///     Resultado do patch antigo (só <c>__result</c>): mexia no ALCANCE e não no VOLUME. O vanilla prova que
    ///     isso é meio-caminho — <c>Physical.SoundRadius</c> (sobrepeso) entra no MESMO <c>num3</c> e portanto afeta
    ///     os DOIS canais. Espelhar o valor no cache é fidelidade ao vanilla, não um efeito extra (não é "dobrar":
    ///     volume e distância são canais distintos, cada um multiplicado UMA vez).
    ///     </para>
    ///     <para>FieldRef cacheado (csharp-mod-best-practices §3). Campo ausente (build futura do EFT) → null →
    ///     degrada pro comportamento antigo (alcance só) com 1 aviso, em vez de derrubar o patch.</para>
    /// </summary>
    private static readonly AccessTools.FieldRef<Player, Dictionary<EAudioMovementState, float>>? RolloffCache =
        ResolveRolloffCache();

    private static AccessTools.FieldRef<Player, Dictionary<EAudioMovementState, float>>? ResolveRolloffCache()
    {
        try
        {
            return AccessTools.FieldRefAccess<Player, Dictionary<EAudioMovementState, float>>("_cachedMovementRolloff");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"[CustomClasses] _cachedMovementRolloff não resolvido — volume do passo fica vanilla: {ex.Message}");
            return null;
        }
    }

    protected override MethodBase GetTargetMethod()
    {
        // Desambigua o overload (fix review): existe method_67() sem-args de outra classe; queremos o de áudio.
        return AccessTools.Method(typeof(Player), "method_67",
            new[] { typeof(EAudioMovementState), typeof(bool) });
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, EAudioMovementState movementState, ref float __result)
    {
        try
        {
            // B20: classe do EMISSOR (você OU um peer Fika). Bots ficam de fora dentro do ClassIdOf.
            // ref: AUD-01-02 · PA-03-01 — ClassIdOf é o ÚNICO resolvedor deste hot path (por som de
            // movimento de CADA player/bot). O NOME só é resolvido dentro do gate de diagnóstico, abaixo.
            // PERF-INSTR AUD-01-02 — temporary, remove after validation (PA-02-05: Calls antes do gate)
            if (PerkDiag.Enabled)
            {
                PerfCount.RolloffCalls++;
            }

            var emitterId = ClassIdentities.ClassIdOf(__instance);
            if (emitterId == EClassId.None)
            {
                return;   // vanilla/bot/desconhecido → não toca no som
            }

            // PERF-INSTR AUD-01-02 — temporary, remove after validation
            if (PerkDiag.Enabled)
            {
                PerfCount.RolloffPassed++;
            }

            var r0 = __result;   // (052) baseline p/ o diagnóstico

            // 🔧 Ghost Step (Furtivo −30%) / Stalker (Caçador −20%): reduz o raio de audibilidade.
            __result *= QuietStep.MultFor(emitterId);

            // 🔻 Loud Operator (Fuzileiro + Tanque, desdobrado por classe): aumenta o raio de audibilidade.
            __result *= LoudOperator.MultFor(emitterId);

            // Espelha no cache p/ o VOLUME não divergir do alcance (ver doc de RolloffCache). Mult == 1 (classe sem
            // perk de som) → não escreve: preserva o caminho vanilla byte a byte. Comparação EXATA de propósito
            // (não é tolerância): o único caso a pular é o multiplicador neutro, que devolve o valor idêntico.
            if (__result != r0)
            {
                var cache = RolloffCache?.Invoke(__instance);
                if (cache != null)
                {
                    cache[movementState] = __result;
                }
            }

            if (PerkDiag.Enabled)
            {
                if (__instance.IsYourPlayer)
                {
                    // Overlay 052 = painel do SEU personagem; um peer barulhento não pode sobrescrever seus números.
                    PerkDiag.AudioBefore = r0;
                    PerkDiag.AudioAfter = __result;
                }
                else if (__result != r0)
                {
                    // B20: única prova objetiva de que o perk de um PEER está valendo no SEU áudio (o overlay não
                    // alcança peers). Throttled; só com Diagnostics ligado no F12.
                    // ref: PA-03-01 — NameOf (switch puro) só aqui dentro; nunca um 2º lookup no hot path.
                    PerkDiag.LogPeer("rolloff (you hear)", __instance.Profile?.Nickname ?? "?",
                                     SkillMultipliers.NameOf(emitterId) ?? "?", r0, __result);
                }
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
            // 079: 1ª pessoa — só o MainPlayer local. Scavenger (Silent Looter, perk) OU Rifleman (Loud Looter, drawback).
            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer))
            {
                return;
            }

            if (PerksConfig.SilentLooterEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Scavenger))
            {
                volume *= PerksConfig.SilentLooterVolume?.Value ?? 1f;
            }
            else if (PerksConfig.LoudLooterEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Rifleman))
            {
                volume *= PerksConfig.LoudLooterVolume?.Value ?? 1f;   // 079 — Saque Barulhento (loot mais alto)
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
///     a classe de QUEM EMITIU o som (<see cref="ClassIdentities.ClassIdOf"/>: local via SkillMultipliers,
///     peer via o mapa nickname→classe da rota 057) e aplicamos o multiplicador DELA. Sem protocolo novo.
///     ⚠️ O VALOR sai do F12 de quem roda isto (o host) — ele é a autoridade da percepção da IA, que é dele.
///     Este patch cobre o canal da <b>IA</b>; o canal do <b>áudio que humanos ouvem</b> é o
///     <see cref="SoundRadiusPatch"/> (B20). São independentes — o <c>power</c> daqui NÃO deriva do
///     <c>method_67</c> nem do <c>Physical.SoundRadius</c> (MovementContext.cs:1753), então não há efeito dobrado.
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
            // ref: AUD-01-02 · PA-03-01 — ÚNICO lookup deste hot path (1 por passo de CADA player/bot).
            // PERF-INSTR AUD-01-02 — temporary, remove after validation (Calls após o descarte de não-passo)
            if (PerkDiag.Enabled)
            {
                PerfCount.StepAiCalls++;
            }

            var emitterId = ClassIdentities.ClassIdOf(p);
            if (emitterId == EClassId.None)
            {
                return;   // vanilla/desconhecido → sem efeito
            }

            // PERF-INSTR AUD-01-02 — temporary, remove after validation
            if (PerkDiag.Enabled)
            {
                PerfCount.StepAiPassed++;
            }

            var p0 = power;

            power *= QuietStep.MultFor(emitterId);
            power *= LoudOperator.MultFor(emitterId);

            if (PerkDiag.Enabled)
            {
                if (p.IsYourPlayer)   // o overlay (052) só descreve o SEU player
                {
                    PerkDiag.AiPowerBefore = p0;
                    PerkDiag.AiPowerAfter = power;
                }
                else if (power != p0)
                {
                    // B14: prova de que o HOST está aplicando o perk de som de um PEER contra a IA — o cenário que
                    // era placebo antes e que não dá para verificar de ouvido (é a percepção do BOT que muda).
                    // ref: PA-03-01 — NameOf só dentro do gate de diagnóstico.
                    PerkDiag.LogPeer("AI hear power", p.Profile?.Nickname ?? "?",
                                     SkillMultipliers.NameOf(emitterId) ?? "?", p0, power);
                }
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
            var emitterId = ClassIdentities.ClassIdOf(emitter);   // ref: AUD-01-02 · PA-03-01
            if (emitterId == EClassId.None)
            {
                return;   // bot, vanilla ou desconhecido → sem efeito
            }

            var before = __2;
            var soundType = Convert.ToInt32(__0);

            __2 *= QuietStep.MultFor(emitterId);      // reduz TODOS (Ghost Step / Stalker)
            __2 *= LoudOperator.MultFor(emitterId);   // aumenta TODOS

            if (soundType == SainSoundTypeLooting)
            {
                __2 *= SilentLooter.MultFor(emitterId);   // anti-detecção: reduz só o Looting (Saqueador)
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
