using System;
using System.Linq;
using System.Reflection;
using EFT.UI;       // InventoryPlayerModelWithStatsWindow, ChatSpecialIcon
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;      // ColorUtility
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (015) Identidade da classe no nome do jogador na tela de personagem (OVERALL) e no perfil —
///     EFT.UI.InventoryPlayerModelWithStatsWindow (campos _nicknameLabel + _specialIcon). Só o jogador
///     LOCAL (o nickname recém-escrito pelo vanilla bate com o do perfil): colore o nome na cor da classe,
///     troca o ícone do _specialIcon pelo ícone da classe (tingido + escala) e anexa o tooltip da classe.
///     ref: Assembly-CSharp.dll → InventoryPlayerModelWithStatsWindow.Show(GInterface214,int,EMemberCategory,
///          ProfileStats,LastPlayerStateClass,InventoryController,bool):116 (_nicknameLabel.text),:118 (_specialIcon.Show).
/// </summary>
internal class PlayerModelWithStatsIdentityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // 2 overloads de Show — o de 7 params é o que popula _nicknameLabel/_specialIcon.
        return AccessTools.GetDeclaredMethods(typeof(InventoryPlayerModelWithStatsWindow))
            .First(m => m.Name == nameof(InventoryPlayerModelWithStatsWindow.Show) && m.GetParameters().Length == 7);
    }

    [PatchPostfix]
    private static void Postfix(TextMeshProUGUI ____nicknameLabel, ChatSpecialIcon ____specialIcon)
    {
        if (!Plugin.ShowClassOnPlayerName || ____nicknameLabel == null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.ClassName == null)
            {
                return;   // edition vanilla
            }

            var nickname = SkillMultipliers.Nickname;
            if (string.IsNullOrEmpty(nickname))
            {
                return;
            }

            // O vanilla acabou de setar _nicknameLabel.text = GetCorrectedNickname(...) → só o LOCAL casa.
            if (!string.Equals(____nicknameLabel.text, nickname, StringComparison.Ordinal))
            {
                return;
            }

            // Nome com GRADIENTE da cor da classe (efeito canônico) — sem texto da classe (vai no tooltip).
            // (review CR-UI5-01) sem nameColor → label NATIVO (fallback `.color` vivo = drift de clareamento).
            ____nicknameLabel.text = nickname;
            if (!string.IsNullOrWhiteSpace(SkillMultipliers.NameColor))
            {
                ClassIdentityView.ApplyGradient(____nicknameLabel, SkillMultipliers.NameColor, Color.white);
            }

            // Ícone da classe (tint silhueta) no _specialIcon. 006-fix: tamanho proporcional à fonte do nome.
            ClassIdentityView.ApplyClassIcon(____specialIcon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, ClassIdentityView.IconSizeFor(____nicknameLabel));

            // Tooltip "This player is <classe>" (i18n) no nome.
            ClassTooltip.Attach(____nicknameLabel.gameObject,
                ClassIdentityView.BuildTooltip(SkillMultipliers.ClassName!, SkillMultipliers.NameColor, Color.white));
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] player stats identity falhou: {ex.Message}");
        }
    }
}
