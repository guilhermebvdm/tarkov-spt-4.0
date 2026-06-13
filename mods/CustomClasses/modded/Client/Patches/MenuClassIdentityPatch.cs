using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using EFT.UI;       // MenuScreen
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (015) Identidade da classe no painel do jogador do Menu-Overhaul, em 3 linhas:
///     linha 1 = [ícone + nome] na cor da classe; linha 2 = [NOME DA CLASSE] (CAPSLOCK, cor da classe);
///     o restante (EXP/nível) em branco para contraste. Sem o Menu-Overhaul o painel não existe → no-op.
///     Idempotente: reaplica a cada Show; cria a linha de classe e o ícone uma única vez (Find por nome).
///     ref: EFT.UI.MenuScreen.Show(Profile, MatchmakerPlayerControllerClass, ESessionMode);
///          Menu-Overhaul cria MainMenuPlayerModelView/BottomField/NicknameText (PlayerProfileFeaturesPatch).
/// </summary>
internal class MenuClassIdentityPatch : ModulePatch
{
    private const string ClassLineName = "CC_ClassLine";
    private const string MenuIconName = "CC_MenuIcon";

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(MenuScreen))
            .First(m => m.Name == nameof(MenuScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(MenuScreen __instance)
    {
        if (!Plugin.ShowClassOnPlayerName)
        {
            MenuOverhaulBridge.RestoreAccent();   // desligado → cor original do Menu-Overhaul
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            if (string.IsNullOrEmpty(SkillMultipliers.ClassName))
            {
                MenuOverhaulBridge.RestoreAccent();   // perfil vanilla → cor original do Menu-Overhaul
                return;
            }

            Plugin.Instance?.StartCoroutine(ApplyToMenu(__instance));
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] menu identity falhou: {ex.Message}");
        }
    }

    private static IEnumerator ApplyToMenu(MenuScreen menu)
    {
        // O Menu-Overhaul cria o painel do jogador de forma assíncrona (idempotente) → espera alguns frames.
        TextMeshProUGUI? nick = null;
        for (var i = 0; i < 60 && nick == null; i++)
        {
            var pmv = GameObject.Find("MainMenuPlayerModelView");
            var nickTr = pmv?.transform.Find("BottomField/NicknameText");
            if (nickTr != null)
            {
                nick = nickTr.GetComponent<TextMeshProUGUI>();
            }

            if (nick == null)
            {
                yield return null;
            }
        }

        var nickname = SkillMultipliers.Nickname;
        if (menu == null || nick == null || string.IsNullOrEmpty(nickname))
        {
            yield break;   // sem Menu-Overhaul / fechou / sem nickname → no-op
        }

        try
        {
            var baseColor = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);

            // EXP, top glow, botões e luzes do menu seguem a cor da classe (AccentColor do Menu-Overhaul).
            MenuOverhaulBridge.SetAccent(baseColor);

            // Linha 1: nome do jogador com GRADIENTE (vertex vence o .color que o MO aplica).
            nick.text = nickname;
            ClassIdentityView.ApplyGradient(nick, baseColor);

            // Linha 1: ícone da classe à esquerda do nome (tint silhueta). 006-fix: tamanho proporcional à fonte do nome.
            var icon = GetOrCreateMenuIcon(nick.transform);
            ClassIdentityView.ApplyClassIcon(icon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, ClassIdentityView.IconSizeFor(nick));

            // Linha 2: nome da classe (CAPSLOCK) com gradiente, logo abaixo do nome.
            var classLine = GetOrCreateClassLine(nick.transform.parent, nick);
            classLine.text = SkillMultipliers.ClassName!.ToUpperInvariant();
            ClassIdentityView.ApplyGradient(classLine, baseColor);
            classLine.transform.SetSiblingIndex(nick.transform.GetSiblingIndex() + 1);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] menu layout falhou: {ex.Message}");
        }
    }

    /// <summary>Cria (1x) ou reusa o Image do ícone, ancorado à esquerda do nome.</summary>
    private static Image GetOrCreateMenuIcon(Transform nickTransform)
    {
        var existing = nickTransform.Find(MenuIconName);
        if (existing != null)
        {
            return existing.GetComponent<Image>();
        }

        var go = new GameObject(MenuIconName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(nickTransform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);   // borda esquerda do nome
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(28f, 28f);
        rt.anchoredPosition = new Vector2(-8f, 0f);             // gap à esquerda
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    /// <summary>Cria (1x) ou reusa o TMP da linha do nome da classe, herdando o estilo do nome.</summary>
    private static TextMeshProUGUI GetOrCreateClassLine(Transform bottomField, TextMeshProUGUI template)
    {
        var existing = bottomField.Find(ClassLineName);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        var go = new GameObject(ClassLineName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(bottomField, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = template.font;
        t.fontSize = template.fontSize * 0.8f;
        t.alignment = template.alignment;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        return t;
    }
}
