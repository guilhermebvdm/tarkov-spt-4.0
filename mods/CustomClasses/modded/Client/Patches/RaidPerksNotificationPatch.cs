using System;
using System.Collections;
using System.Reflection;
using EFT;
using EFT.Communications;   // ENotificationDurationType
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 050 — ao iniciar a raid, mostra UMA notificação (canto inf. esquerdo) com os perks (verde)
///     e drawbacks (vermelho) da classe ativa, pra o jogador lembrar dos buffs/debuffs.
///     Hook: <c>GameWorld.OnGameStarted</c> (player já carregado — mesmo ponto usado p/ áudio, ver memória).
///     ⚠️ TIMING: delay de segurança pós-load antes de exibir (não solta antes do personagem carregar).
///     Toggle no F12 (PerksConfig.ShowRaidPerksNotification). Ignora hideout/academia.
/// </summary>
internal class RaidPerksNotificationPatch : ModulePatch
{
    /// <summary>Fix 2026-07-03 — texto da última notificação exibida; o <see cref="NotificationDurationPatch"/> casa por ele (10s).</summary>
    internal static string? LastNotificationText;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        try
        {
            // (review fix) reseta a Adrenaline no início da raid — o cooldown não deve atravessar de uma raid
            // anterior (Time.time é monotônico no processo). Roda independente do toggle da notificação.
            AdrenalineState.Reset();
            PerkDiag.ResetPeerLog();   // B20: throttle do diag de peer é por raid (o roster muda)
            StancesArmStaminaBridge.TryAttach(finalAttempt: true);   // (051 PA-01-01) re-try do hook — aqui todos os plugins já carregaram

            // ⚠️ ORDEM (code-review B14, achado 3): o guard de hideout vem ANTES dos prefetches — senão TODA entrada
            // no hideout dispara 2 GETs síncronos inúteis. E vem DEPOIS do Adrenaline/Stances (que não dependem de
            // rede), e ANTES do toggle da notificação — os prefetches NÃO podem ficar reféns de um toggle de UI:
            // eles é que sustentam os perks de som (B14/B20).
            if (__instance == null || __instance.GetType().Name.IndexOf("Hideout", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            // (review CR-051-01 + B14 achado 2) REFETCH da classe LOCAL. Era só EnsureLoaded(), o que criava uma
            // assimetria: o mapa de peers vinha fresco todo raid e a classe local não (o único Reset() é a tela de
            // deploy, que não roda em headless / sem painel de grupo). Trocar de classe no editor web entre raids
            // deixava você com a classe ANTIGA e os peers com a NOVA. Aqui (tela de load) o GET é hitch invisível.
            SkillMultipliers.Prefetch();
            // (B14/B20, 2026-07-11) MESMO motivo, agora para o mapa nickname→classe: os patches de som resolvem a
            // classe de QUALQUER emissor (peers Fika inclusive). Sem este warm, a 1ª resolução de um peer cairia
            // dentro do BotEventHandler.PlaySound (a cada passo!) e o GET SÍNCRONO daria hitch no meio da raid —
            // e desde o achado 4 o hot path NÃO busca mais sozinho: mapa frio aqui = perk de som morto na raid.
            ClassIdentities.Prefetch();

            // (fix 2026-07-15) Pack Mule — dreno de stamina do Tank ao ANDAR com pouco peso. BUG DE TIMING:
            // BasePhysicalClass.UpdateWeightLimits() roda em Physical.Init(), ANTES de MainPlayer estar setado, então
            // o gate do PackMulePatch falha (gw!=null mas MainPlayer==null → não aplica o piso +30%) e os LIMITES de
            // overweight ficam cacheados no valor vanilla. Como UpdateWeightLimits() quase nunca é rechamado na raid,
            // o Tank fica WalkOverweight>0 em ~45 kg em vez de ~58.5 (45×1.30) → drena stamina ao andar. O HUD mostra
            // "+30%" porque o marcador lê o getter LIVE (já floorado) — é a discrepância, não a causa.
            // AQUI o estado já estabilizou (MainPlayer setado, Prefetch resolveu a classe), então forçar o recálculo
            // re-cacheia os limites JÁ com o piso. OnWeightLimitsUpdated() = UpdateWeightLimits() + OnWeightUpdated().
            // ⚠️ LOAD-BEARING TAMBÉM PRO OVERLADEN (074/F5): a 1ª OnWeightUpdated do player roda em Player.Init (MainPlayer
            // ainda null → o OverladenInertiaPatch pula) → a inércia nasceria vanilla até a 1ª mudança de peso. Este
            // OnWeightUpdated (via OnWeightLimitsUpdated), pós-MainPlayer, re-aplica o boost do Overladen no raid-start.
            // NÃO remover pensando que é só Pack Mule. (ref: reference_spt_init_before_mainplayer)
            try
            {
                __instance.MainPlayer?.Physical?.OnWeightLimitsUpdated();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[CustomClasses] recálculo de limites de peso (Pack Mule) falhou: {ex.Message}");
            }

            if (PerksConfig.ShowRaidPerksNotification?.Value != true)
            {
                return;
            }

            Plugin.Instance?.StartCoroutine(ShowAfterLoad());
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] raid perks notif (hook) falhou: {ex.Message}");
        }
    }

    private static IEnumerator ShowAfterLoad()
    {
        // Segurança: espera o personagem/UI carregar por completo antes de notificar.
        yield return new WaitForSeconds(3f);

        string? text = null;
        try
        {
            SkillMultipliers.EnsureLoaded();
            text = PerksCatalog.BuildNotificationText();
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] raid perks notif (build) falhou: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(text))
        {
            // Fix 2026-07-03: 10s exatos via NotificationDurationPatch (casa pelo texto e promove p/ Infinite
            // + hide agendado). O `Long` fica de FALLBACK caso o patch da view não case (≈2× o default).
            LastNotificationText = text;
            NotificationManagerClass.DisplayMessageNotification(text, ENotificationDurationType.Long);
        }
    }
}
