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
///     (015/057) Identidade da classe no nome do jogador, via o widget comum EFT.UI.ChatSpecialIcon
///     (deploy / chat / grupo / party / confirmation). Local → identidade via SkillMultipliers; REMOTO →
///     resolvido pelo mapa nickname→classe do server (review CR-057F3-02 — este widget é a superfície real
///     da listagem de grupo do deploy, via PartyPlayerItem). Aplica ícone tingido + cor do nome + tooltip
///     "This player is &lt;classe&gt;". Vanilla/desconhecido: revertido ao NATIVO (cor capturada — CR-UI5-02).
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

            // (review CR-057F3-02) identidade também pros REMOTOS: este widget é a superfície REAL da listagem
            // de grupo do deploy (PartyPlayerItem._chatSpecialIcon) — antes o patch REVERTIA qualquer nickname
            // ≠ local, anulando a identidade per-player do 057. Mapa via ClassIdentities (nickname→classe).
            ClassIdentities.Identity? id = null;
            if (isLocalClass)
            {
                id = ClassIdentities.Local();
            }
            else if (!string.IsNullOrEmpty(playerName) && ClassIdentities.TryResolve(playerName, out var resolved))
            {
                id = resolved;
            }

            if (id == null)
            {
                // Player vanilla/desconhecido (célula reciclada em chat/lista): reverte tudo ao nativo.
                // (review CR-UI5-02) na v2 a COR vive no .color → devolve a cor capturada, não só o gradiente.
                ClassIdentityView.RestoreNativeLabel(____specialLabel);
                ClassIdentityView.RevertIconGradient(____icon);  // (06-fix-02) remove o gradiente do ícone (evita vazar p/ outros)
                if (____icon != null)
                {
                    ____icon.transform.localScale = Vector3.one;
                }

                ClassTooltip.Clear(____specialLabel.gameObject);
                return;
            }

            // Ícone da classe (tint silhueta; força sprite/ativação no caso Default). 006-fix: tamanho proporcional à fonte do nome.
            ClassIdentityView.ApplyClassIcon(____icon, id.IconFile, id.NameColor, ClassIdentityView.IconSizeFor(____specialLabel));

            // Nome com GRADIENTE da cor da classe (efeito canônico) — sem texto da classe (vai no tooltip).
            // (review CR-UI5-01) classe SEM nameColor → label NATIVO (o fallback `.color` vivo compunha o
            // clareamento a cada re-apply — drift geométrico rumo ao branco).
            if (isLocalClass)
            {
                ____specialLabel.text = playerName;
            }

            if (!string.IsNullOrWhiteSpace(id.NameColor))
            {
                ClassIdentityView.ApplyGradient(____specialLabel, id.NameColor, Color.white);
            }
            else
            {
                ClassIdentityView.RestoreNativeLabel(____specialLabel);
            }

            // Tooltip "This player is <classe>" (i18n) no nome.
            var display = id.DisplayName;
            if (!string.IsNullOrEmpty(display))
            {
                ClassTooltip.Attach(____specialLabel.gameObject,
                    ClassIdentityView.BuildTooltip(display!, id.NameColor, Color.white));
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] chat special icon falhou: {ex.Message}");
        }
    }
}
