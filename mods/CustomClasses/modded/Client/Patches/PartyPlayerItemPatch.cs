using System;
using System.Linq;
using System.Reflection;
using EFT.UI;                   // ChatSpecialIcon
using EFT.UI.Matchmaker;        // PartyInfoPanel
using EFT.UI.UI.Matchmaker;     // PartyPlayerItem (namespace real — decompile 2026-07-04)
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (015/057 — review CR-057F3-01) Host REAL da listagem superior-esquerda do deploy ("DEPLOYING TO
///     LOCATION"): <c>PartyPlayerItem</c> (rows do <c>PartyInfoPanel</c> em <c>MatchmakerTimeHasCome</c>,
///     populadas pelo FIKA via AddPartyMembers). O host anterior (<c>RaidReadyPlayerPanel.Show</c>) é CÓDIGO
///     MORTO no SPT: o branch <c>ERaidMode.Local</c> (forçado pelo ForceRaidModeToLocalPatch) executa
///     <c>_playersRaidReadyPanel.Close()</c> incondicionalmente (decompile MatchMakerAcceptScreen:354-357).
///     A identidade visual da linha (brasão + cor por player, local E remoto) é aplicada pelo
///     <see cref="ChatSpecialIconPatch"/> no <c>_chatSpecialIcon.Show(info)</c> interno (roda ANTES deste
///     Postfix); aqui entram a escala do 015, o popover no cursor (057) e a limpeza do tooltip redundante.
///     ref: decompile PartyPlayerItem.Show(GroupPlayerViewModelClass) → _chatSpecialIcon.Show(Info);
///     PartyInfoPanel.Show → AddViewList (Instantiate POR player — sem pooling → sem identidade stale).
/// </summary>
internal class PartyPlayerItemPatch : ModulePatch
{
    private static readonly FieldInfo? IconField = AccessTools.Field(typeof(PartyPlayerItem), "_chatSpecialIcon");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PartyPlayerItem), nameof(PartyPlayerItem.Show));
    }

    [PatchPostfix]
    private static void Postfix(PartyPlayerItem __instance, GroupPlayerViewModelClass raidPlayer)
    {
        try
        {
            // (015) escala do conjunto ícone+nome — no host morto isso NUNCA disparou; default agora 1.2 (neutro-leve).
            if (Plugin.ShowClassOnPlayerName && IconField?.GetValue(__instance) is ChatSpecialIcon special)
            {
                special.transform.localScale = Vector3.one * (Plugin.DeployNameScale?.Value ?? 1.2f);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (015) party item scale falhou: {ex.Message}");
        }

        try
        {
            // (057) popover da classe no hover da linha (qualquer membro do grupo com classe resolvida).
            if (PerksConfig.ClassDetailOnLoading?.Value != true)
            {
                return;
            }

            var nickname = raidPlayer?.Info?.Nickname;   // ref: GClass1410.cs:9
            if (string.IsNullOrEmpty(nickname))
            {
                return;
            }

            ClassIdentities.Identity? id = null;
            if (ClassIdentities.TryResolve(nickname, out var resolved))
            {
                id = resolved;
            }
            else
            {
                SkillMultipliers.EnsureLoaded();
                if (string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal))
                {
                    id = ClassIdentities.Local();
                }
            }

            var hover = __instance.GetComponent<LoadingClassHover>();
            if (id == null)
            {
                if (hover != null)
                {
                    hover.Identity = null;   // re-Show da MESMA linha p/ player vanilla → popover não abre (defensivo)
                }

                return;
            }

            if (hover == null)
            {
                hover = __instance.gameObject.AddComponent<LoadingClassHover>();
            }

            hover.Identity = id;
            hover.FollowCursor = true;   // (06-fix-03) popover abre NO CURSOR, não sobre o carrossel

            // (review CR-057F3-07) o tooltip "This player is <classe>" (015, anexado pelo ChatSpecialIconPatch
            // no Show interno) duplicaria com o popover no MESMO hover — nesta tela o popover ganha.
            if (IconField?.GetValue(__instance) is ChatSpecialIcon icon)
            {
                var label = icon.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    ClassTooltip.Clear(label.gameObject);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (057) party item hover falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     (057 — review CR-057F3-03) Mapa nickname→classe FRESCO por tela de deploy: Prefix no
///     <c>PartyInfoPanel.Show</c> (roda ANTES das rows serem populadas) reseta os caches — troca de classe no
///     editor web / perfil novo no grupo refletem sem restart do client. O fetch em si segue lazy/síncrono no
///     1º consumidor (mesmo frame): 1 GET pequeno por abertura da tela — hitch pontual aceito (CR-057F3-04).
///     Também refetcha a classe LOCAL (review CR-051-03: SkillMultipliers.Reset não tinha caller).
/// </summary>
internal class PartyInfoPanelPrefetchPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // 1 Show declarado (decompile PartyInfoPanel:93) — First evita referenciar GClass1628 no IL.
        return AccessTools.GetDeclaredMethods(typeof(PartyInfoPanel))
            .First(m => m.Name == nameof(PartyInfoPanel.Show));
    }

    [PatchPrefix]
    private static void Prefix()
    {
        try
        {
            ClassIdentities.Reset();
            SkillMultipliers.Reset();
            SkillMultipliers.EnsureLoaded();   // warm imediato (as rows desta MESMA tela leem o local logo em seguida)
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (057) prefetch do deploy falhou: {ex.Message}");
        }
    }
}
