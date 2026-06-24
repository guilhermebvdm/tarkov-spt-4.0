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
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        try
        {
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
            // Long ≈ 10s (≥5s pedido) — a notificação default sumia rápido demais.
            NotificationManagerClass.DisplayMessageNotification(text, ENotificationDurationType.Long);
        }
    }
}
