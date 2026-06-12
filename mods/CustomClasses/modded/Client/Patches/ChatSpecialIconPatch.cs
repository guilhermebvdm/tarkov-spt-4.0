using System;
using System.Linq;
using System.Reflection;
using EFT.UI;       // ChatSpecialIcon
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;        // TextMeshProUGUI
using UnityEngine;      // Color
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (015) Identidade da classe no nome do JOGADOR LOCAL, via o widget comum EFT.UI.ChatSpecialIcon
///     (deploy / chat / grupo / party / confirmation). Para o jogador local e classe do mod: troca o ícone
///     "person" pelo ícone da classe (tingido + escala F12), colore o NOME com a cor da classe (sem texto
///     da classe inline) e anexa o tooltip "This player is &lt;classe&gt;". Outros jogadores: revertido ao vanilla.
///     ref: Assembly-CSharp.dll → ChatSpecialIcon.Show(EMemberCategory, string, bool, int) { Image _icon; TextMeshProUGUI _specialLabel; }.
///     EMemberCategory.Default == 0 → GetDataByMemberCategory(Default) é null → Show retorna cedo (ApplyClassIcon força o sprite/ativação).
/// </summary>
internal class ChatSpecialIconPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // 2 overloads de Show — pegar o de 4 params (EMemberCategory, playerName, isNameColored, prestigeLevel).
        return AccessTools.GetDeclaredMethods(typeof(ChatSpecialIcon))
            .First(m => m.Name == nameof(ChatSpecialIcon.Show) && m.GetParameters().Length == 4);
    }

    [PatchPostfix]
    private static void Postfix(string playerName, TextMeshProUGUI ____specialLabel, Image ____icon)
    {
        if (!Plugin.ShowClassOnPlayerName || ____specialLabel == null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            var isLocalClass = SkillMultipliers.ClassName != null
                               && !string.IsNullOrEmpty(playerName)
                               && string.Equals(playerName, SkillMultipliers.Nickname, StringComparison.Ordinal);

            if (!isLocalClass)
            {
                // Outro jogador (célula reciclada em chat/lista): reverte gradiente/tint e remove nosso tooltip.
                ____specialLabel.enableVertexGradient = false;   // o vanilla não desliga → evita vazar o gradiente
                ClassIdentityView.RevertIconGradient(____icon);  // (06-fix-02) remove o gradiente do ícone (evita vazar p/ outros)
                if (____icon != null)
                {
                    ____icon.transform.localScale = Vector3.one;
                }

                ClassTooltip.Clear(____specialLabel.gameObject);
                return;
            }

            // Ícone da classe (tint silhueta; força sprite/ativação no caso Default). 006-fix: tamanho proporcional à fonte do nome.
            ClassIdentityView.ApplyClassIcon(____icon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, ClassIdentityView.IconSizeFor(____specialLabel));

            // Nome com GRADIENTE da cor da classe (efeito canônico) — sem texto da classe (vai no tooltip).
            ____specialLabel.text = playerName;
            ClassIdentityView.ApplyGradient(____specialLabel, SkillMultipliers.NameColor, ____specialLabel.color);

            // Tooltip "This player is <classe>" (i18n) no nome.
            ClassTooltip.Attach(____specialLabel.gameObject,
                ClassIdentityView.BuildTooltip(SkillMultipliers.ClassName!, SkillMultipliers.NameColor, ____specialLabel.color));
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] chat special icon falhou: {ex.Message}");
        }
    }
}
