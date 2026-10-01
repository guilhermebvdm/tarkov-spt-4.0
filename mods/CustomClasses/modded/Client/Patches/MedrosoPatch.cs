using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     082 — 🔻 <b>Medroso</b> (Saqueador): mãos trêmulas SOB FOGO. Porta a lógica do mod <c>UnderFire</c> (rpmwpm),
///     gateada só p/ o Scavenger local. Dois gatilhos: <b>levar tiro</b> (<c>Player.ReceiveDamage</c>) e
///     <b>supressão / near-miss</b> (<c>GClass897.OnShoot</c> = BulletSoundsUtils, bala passando perto). Efeito: o
///     Tremor NATIVO do EFT, aplicado por reflexão (ver <see cref="Medroso.ResolveAddTremor"/>). ⚠️ O mod UnderFire
///     GLOBAL deve ficar DESATIVADO (senão TODOS ganham o tremor, não só o Scav).
/// </summary>
internal static class Medroso
{
    private static float _cooldownUntil;
    private static bool _shootHooked;

    // 082-fix (v0.16.8): aplica o Tremor NATIVO (tipo aninhado protegido de ActiveHealthController), NÃO uma
    // subclasse do mod. A antiga ScavengerTremor : GClass3008 causava KeyNotFoundException na serialização/sync
    // de saúde: GClass3058.Dictionary_0 (registro que mapeia efeito→byte na rede/save) é montado SÓ dos tipos
    // ANINHADOS de ActiveHealthController, então "ScavengerTremor" não existia nele → GClass3059.Make estourava
    // FORA do nosso try/catch, a cada sync. Crítico no FIKA coop (o sync de saúde roda ao adicionar o efeito); em
    // SPT solo só estouraria no save (_sendNetworkSyncPackets=false). O Tremor nativo já está no registro, também
    // implementa GInterface331 (não deduplica → o cooldown continua sendo a trava) e dispara o mesmo
    // EPhysicalCondition.Tremor via GInterface361. Os overrides de DefaultDelay/ResidueTime da antiga subclasse eram
    // código morto (o AddEffect recebe delay/work/residue explícitos → o "?? Default" curto-circuita).
    private static MethodInfo? _addTremor;
    private static bool _resolveFailed;

    /// <summary>Registra o hook de near-miss (evento ESTÁTICO). Chamado 1× do Plugin; o gate real é no handler.</summary>
    internal static void Init()
    {
        if (_shootHooked)
        {
            return;
        }

        try
        {
            GClass897.OnShoot += OnBulletFlyBy;   // ref: GClass897 (BulletSoundsUtils).OnShoot
            _shootHooked = true;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (082) medroso hook falhou: {ex.Message}");
        }
    }

    /// <summary>Reseta o cooldown entre raids (estado não vaza p/ a próxima).</summary>
    internal static void ResetRaid() => _cooldownUntil = 0f;

    /// <summary>Aplica o tremor se: perk on + Scav local + fora do cooldown + HC disponível.</summary>
    private static void Trigger()
    {
        try
        {
            if (PerksConfig.MedrosoEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Scavenger))
            {
                return;
            }

            if (Time.time < _cooldownUntil)
            {
                return;   // anti-spam: não empilha tremor
            }

            var hc = Singleton<GameWorld>.Instance?.MainPlayer?.ActiveHealthController;
            if (hc == null)
            {
                return;
            }

            var dur = PerksConfig.MedrosoDuration?.Value ?? 6f;
            if (!ApplyTremor(hc, dur))
            {
                return;   // tipo nativo não resolvido — não arma o cooldown (re-tenta no próximo gatilho)
            }

            // CR#7 (082): o AddEffect NÃO deduplica efeitos que implementam GInterface331 (cria nova instância).
            // O cooldown é a única trava — então nunca pode ser MENOR que a duração, senão o Tremor EMPILHA.
            _cooldownUntil = Time.time + Mathf.Max(PerksConfig.MedrosoCooldown?.Value ?? 8f, dur);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (082) medroso trigger falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     Aplica o Tremor NATIVO via <c>ActiveHealthController.AddEffect&lt;Tremor&gt;</c> (reflexão). Mesmos parâmetros
    ///     explícitos do port original (delay 0.1s, work=dur, residue 1.5s; strength/callback default). Retorna false
    ///     se o método/tipo não resolveu (assinatura mudou) — nesse caso o tremor fica inativo, sem crash.
    ///     ref: ActiveHealthController.cs:3117 (Tremor nativo) + :3514 (AddEffect&lt;T&gt;).
    /// </summary>
    private static bool ApplyTremor(ActiveHealthController hc, float dur)
    {
        var add = ResolveAddTremor();
        if (add == null)
        {
            return false;
        }

        add.Invoke(hc, new object?[] { EBodyPart.Head, (float?)0.1f, (float?)dur, (float?)1.5f, null, null });
        return true;
    }

    /// <summary>
    ///     Resolve (1×, cacheado) o <c>AddEffect&lt;T&gt;</c> fechado no tipo aninhado protegido <c>Tremor</c>. Null se
    ///     ausente (logado 1× e nunca mais tenta, p/ não spammar). Usar o tipo NATIVO é o que evita o
    ///     KeyNotFoundException da serialização — ver o comentário do campo <see cref="_addTremor"/>.
    /// </summary>
    private static MethodInfo? ResolveAddTremor()
    {
        if (_addTremor != null)
        {
            return _addTremor;
        }

        if (_resolveFailed)
        {
            return null;
        }

        try
        {
            var tremorType = typeof(ActiveHealthController).GetNestedType("Tremor", BindingFlags.NonPublic);
            var generic = AccessTools.GetDeclaredMethods(typeof(ActiveHealthController))
                .FirstOrDefault(m => m.Name == "AddEffect" && m.IsGenericMethodDefinition
                                     && m.GetParameters().Length == 6);
            if (tremorType == null || generic == null)
            {
                _resolveFailed = true;
                Plugin.Log?.LogError("[CustomClasses] (082) Tremor nativo não resolvido (tipo/método ausente) — tremor inativo nesta sessão");
                return null;
            }

            _addTremor = generic.MakeGenericMethod(tremorType);
            return _addTremor;
        }
        catch (Exception ex)
        {
            _resolveFailed = true;
            Plugin.Log?.LogError($"[CustomClasses] (082) resolução do Tremor nativo falhou: {ex.Message}");
            return null;
        }
    }

    /// <summary>Gatilho 1 — levar TIRO (chamado pelo Postfix de <c>ReceiveDamage</c>).</summary>
    internal static void OnHit(EDamageType type, float damage)
    {
        if (damage <= 0f || (type != EDamageType.Bullet && type != EDamageType.GrenadeFragment))
        {
            return;
        }

        Trigger();
    }

    /// <summary>
    ///     Gatilho 2 — SUPRESSÃO / near-miss (bala passa perto). Portado do <c>UnderFire.CheckFiredBullet</c>: projeta
    ///     a posição do jogador na reta do tiro e mede a menor distância. HOT PATH (cada tiro do mapa) — early-out cedo.
    /// </summary>
    private static void OnBulletFlyBy(SonicBulletSoundPlayer.GClass898 sonic)
    {
        try
        {
            var dist = PerksConfig.MedrosoSuppressDistance?.Value ?? 0f;
            if (dist <= 0f || PerksConfig.MedrosoEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Scavenger))
            {
                return;   // supressão desligada, perk off, ou não-Scav → sai antes da geometria
            }

            if (sonic == null || sonic.IsOccluded || sonic.Camera == null)
            {
                return;
            }

            Vector3 shotPos = sonic.ShotPosition;
            Vector3 shotDir = sonic.ShotDirection;
            Vector3 playerPos = sonic.Camera.transform.position;
            float denom = Vector3.Dot(shotDir, shotDir);
            if (denom <= 0f)
            {
                return;
            }

            float t = -Vector3.Dot(shotPos - playerPos, shotDir) / denom;
            Vector3 closest = shotPos + t * shotDir;
            if (Vector3.Distance(closest, playerPos) < dist)
            {
                Trigger();
            }
        }
        catch
        {
            // hot path — nunca lançar
        }
    }
}

/// <summary>082 — gatilho de DANO do Medroso: Postfix em <c>Player.ReceiveDamage</c> (Player.cs:26231).</summary>
internal class MedrosoDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ReceiveDamage));
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, float damage, EDamageType type)
    {
        try
        {
            if (__instance != null && __instance.IsYourPlayer)
            {
                Medroso.OnHit(type, damage);
            }
        }
        catch
        {
        }
    }
}
