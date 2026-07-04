using System;
using System.Linq;
using System.Reflection;
using EFT.UI;       // PlayerNamePanel, ChatSpecialIcon
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;        // TextMeshProUGUI
using UnityEngine;  // ColorUtility

namespace CustomClasses.Client;

/// <summary>
///     (015) Identidade da classe no widget reutilizável EFT.UI.PlayerNamePanel (ex.: tela de confirmation /
///     MatchMakerPlayerPreview). O Set(...) chama _icon.Show(category, null, ...) → playerName null, logo o
///     ChatSpecialIconPatch não atua aqui; este patch trata nome + ícone + tooltip (só o jogador local).
///     ref: Assembly-CSharp.dll → EFT.UI.PlayerNamePanel.Set(bool,EMemberCategory,string,int,int) faz `_name.text = nickname`
///          (campo `_name`, TMP) e `_icon.Show(...)` (ChatSpecialIcon). Chamado por MatchMakerPlayerPreview.Show (confirmation).
/// </summary>
internal class PlayerNamePanelPatch : ModulePatch
{
    private static readonly FieldInfo? NickField = AccessTools.Field(typeof(PlayerNamePanel), "_name");   // Set faz _name.text = nickname
    private static readonly FieldInfo? IconField = AccessTools.Field(typeof(PlayerNamePanel), "_icon");

    protected override MethodBase GetTargetMethod()
    {
        // 2 overloads de Set — pegar o de 5 params (bool, EMemberCategory, string nickname, int, int).
        return AccessTools.GetDeclaredMethods(typeof(PlayerNamePanel))
            .First(m => m.Name == nameof(PlayerNamePanel.Set) && m.GetParameters().Length == 5);
    }

    [PatchPostfix]
    private static void Postfix(PlayerNamePanel __instance, string nickname)
    {
        if (!Plugin.ShowClassOnPlayerName)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.ClassName == null)
            {
                return;
            }

            // Só o jogador local (nickname recebido bate com o do perfil).
            if (string.IsNullOrEmpty(nickname)
                || !string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal))
            {
                return;
            }

            // Nome com GRADIENTE da cor da classe (efeito canônico) + tooltip.
            // (review CR-UI5-01) sem nameColor → label NATIVO (fallback `label.color` vivo compunha o
            // clareamento a cada re-apply — drift geométrico rumo ao branco).
            if (NickField?.GetValue(__instance) is TextMeshProUGUI label)
            {
                label.text = nickname;
                if (!string.IsNullOrWhiteSpace(SkillMultipliers.NameColor))
                {
                    ClassIdentityView.ApplyGradient(label, SkillMultipliers.NameColor, Color.white);
                }

                ClassTooltip.Attach(label.gameObject,
                    ClassIdentityView.BuildTooltip(SkillMultipliers.ClassName!, SkillMultipliers.NameColor, Color.white));
            }

            // Ícone da classe (tint silhueta) — o _icon é um ChatSpecialIcon (playerName null aqui).
            // 006-fix: tamanho proporcional à fonte do nome (_name) desta tela.
            if (IconField?.GetValue(__instance) is ChatSpecialIcon special)
            {
                var fontTmp = NickField?.GetValue(__instance) as TextMeshProUGUI;
                ClassIdentityView.ApplyClassIcon(special, SkillMultipliers.IconFile, SkillMultipliers.NameColor, ClassIdentityView.IconSizeFor(fontTmp));
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] player name panel falhou: {ex.Message}");
        }
    }
}
