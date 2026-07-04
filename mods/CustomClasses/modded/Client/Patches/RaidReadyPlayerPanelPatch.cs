using System;
using System.Reflection;
using EFT.UI;               // ChatSpecialIcon
using EFT.UI.Matchmaker;    // RaidReadyPlayerPanel
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (015) Aumenta o conjunto ícone+nome do jogador na tela de deploy ("DEPLOYING TO LOCATION") para leitura.
///     Escala o _icon (ChatSpecialIcon — contém o Image do ícone + o TMP do nome) por DeployNameScale (F12, ~1.2).
///     ref: Assembly-CSharp.dll → EFT.UI.Matchmaker.RaidReadyPlayerPanel.Show(...):102 (_icon.Show:109); campo ChatSpecialIcon _icon.
///     (057 06-fix-02, rodada 3): este painel vira TAMBÉM o host do popover per-player no deploy — o hover nas
///     linhas do LoadingScreenUI do FIKA nunca disparou in-game (canvas sem raycast confiável), e AQUI os
///     tooltips já funcionam. Nickname de cada linha: Show(player).Info.Nickname (ref: GClass1410.cs:9);
///     resolução via ClassIdentities (mesma do 057). Raid scav degrada sozinha (linha usa SavageNickname → sem match).
/// </summary>
internal class RaidReadyPlayerPanelPatch : ModulePatch
{
    private static readonly FieldInfo? IconField = AccessTools.Field(typeof(RaidReadyPlayerPanel), "_icon");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RaidReadyPlayerPanel), nameof(RaidReadyPlayerPanel.Show));
    }

    [PatchPostfix]
    private static void Postfix(RaidReadyPlayerPanel __instance, GroupPlayerViewModelClass player)
    {
        try
        {
            // (015) Escala o ChatSpecialIcon inteiro (ícone + nome são filhos) → ambos crescem juntos.
            if (Plugin.ShowClassOnPlayerName && IconField?.GetValue(__instance) is ChatSpecialIcon special)
            {
                special.transform.localScale = Vector3.one * (Plugin.DeployNameScale?.Value ?? 1.2f);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] raid ready scale falhou: {ex.Message}");
        }

        try
        {
            // (057 06-fix-02) popover da classe no hover da linha do player (qualquer membro do grupo).
            if (PerksConfig.ClassDetailOnLoading?.Value != true)
            {
                return;
            }

            var nickname = player?.Info?.Nickname;   // ref: GroupPlayerDataClass.Info (GClass1410.cs:9)
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

            if (id == null)
            {
                return;   // vanilla/scav/desconhecido → linha intocada
            }

            var hover = __instance.GetComponent<LoadingClassHover>();
            if (hover == null)
            {
                hover = __instance.gameObject.AddComponent<LoadingClassHover>();
            }

            hover.Identity = id;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (057) ready panel hover falhou: {ex.Message}");
        }
    }
}
