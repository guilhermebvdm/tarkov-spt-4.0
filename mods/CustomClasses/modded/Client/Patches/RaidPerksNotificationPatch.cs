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
            // (review CR-051-01) warm do cache AQUI (tela de load, hitch invisível): com as UIs de identidade
            // desligadas no F12, o 1º consumidor seria o Factor() DENTRO do Tick de stamina do stances —
            // HTTP síncrono no 1º ADS. EnsureLoaded é no-op se o deploy (PartyInfoPanelPrefetchPatch) já buscou.
            SkillMultipliers.EnsureLoaded();
            // (B14, 2026-07-11) MESMO motivo do warm acima, agora para o mapa nickname→classe: desde o B14 o
            // AiSoundPatch/SainSoundPatch resolvem a classe de QUALQUER emissor (peers Fika inclusive). Sem este
            // warm, a 1ª resolução de um peer cairia dentro do BotEventHandler.PlaySound (a cada passo) e o GET
            // SÍNCRONO daria hitch no meio da raid. No-op se o deploy (PartyInfoPanelPrefetchPatch) já buscou —
            // mas o deploy não é garantido (solo / painel de grupo não renderizado).
            ClassIdentities.Prefetch();
            StancesArmStaminaBridge.TryAttach(finalAttempt: true);   // (051 PA-01-01) re-try do hook — aqui todos os plugins já carregaram
            // (057 CR-057F3-03) o refetch do mapa nickname→classe migrou pro host real: Prefix de
            // PartyInfoPanel.Show (PartyPlayerItemPatch.cs) — a tela de deploy abre ANTES do raid-start.

            if (PerksConfig.ShowRaidPerksNotification?.Value != true)
            {
                return;
            }

            // Só raid real — não hideout/academia (que também passam por um GameWorld).
            if (__instance == null || __instance.GetType().Name.IndexOf("Hideout", StringComparison.OrdinalIgnoreCase) >= 0)
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
