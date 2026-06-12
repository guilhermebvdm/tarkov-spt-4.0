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
///     ref: Assembly-CSharp.dll → EFT.UI.Matchmaker.RaidReadyPlayerPanel.Show(...):101 (_icon.Show); campo ChatSpecialIcon _icon.
/// </summary>
internal class RaidReadyPlayerPanelPatch : ModulePatch
{
    private static readonly FieldInfo? IconField = AccessTools.Field(typeof(RaidReadyPlayerPanel), "_icon");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RaidReadyPlayerPanel), nameof(RaidReadyPlayerPanel.Show));
    }

    [PatchPostfix]
    private static void Postfix(RaidReadyPlayerPanel __instance)
    {
        if (!Plugin.ShowClassOnPlayerName)
        {
            return;
        }

        try
        {
            // Escala o ChatSpecialIcon inteiro (ícone + nome são filhos) → ambos crescem juntos.
            if (IconField?.GetValue(__instance) is ChatSpecialIcon special)
            {
                special.transform.localScale = Vector3.one * (Plugin.DeployNameScale?.Value ?? 1.2f);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] raid ready scale falhou: {ex.Message}");
        }
    }
}
