using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using EFT.UI;       // MenuScreen, DefaultUIButton
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (013) Adiciona um botão "SKILLS" no menu principal, clonado do botão CHARACTER (_playerButton),
///     logo abaixo dele. Ao clicar: aciona o CHARACTER (abre o inventário via OnClick.Invoke) e seleciona
///     a aba Skills (InventoryTabNavigator). Idempotente (Find por nome). Gated por ShowSkillsButton.
///
///     06-fix-01 (Menu-Overhaul): o MO reposiciona os botões por NOME via `anchoredPosition` absoluta
///     (`ButtonHelpers`: y = -index*60, lista [Play, Character, Trade, Hideout, Exit]) a cada Show — então
///     um clone só com SetSiblingIndex ficava órfão/invisível. Com o MO presente, posicionamos o clone via
///     coroutine APÓS o MO agir (espera alguns frames) e EMPURRAMOS Trade/Hideout/Exit p/ abrir espaço,
///     usando posições ABSOLUTAS relativas ao Character (não acumula se reexecutar). Sem o MO: SetSiblingIndex.
///     ref: Assembly-CSharp.dll → EFT.UI.MenuScreen { DefaultUIButton _playerButton; };
///          mods/SPT-Menu-Overhaul/original/Helpers/ButtonHelpers.cs (ButtonYOffset=60, ApplyButtonTransform).
/// </summary>
internal class SkillsNavButtonPatch : ModulePatch
{
    private const string SkillsButtonName = "CC_SkillsButton";
    private const float MoButtonYOffset = 60f;   // ref: MO ButtonHelpers.ButtonYOffset
    // Botões que o MO empilha ABAIXO do Character — empurrados p/ baixo p/ abrir espaço ao SKILLS.
    private static readonly string[] MoButtonsBelowCharacter = { "TradeButton", "HideoutButton", "ExitButtonGroup" };
    private static readonly FieldInfo? PlayerButtonField = AccessTools.Field(typeof(MenuScreen), "_playerButton");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(MenuScreen))
            .First(m => m.Name == nameof(MenuScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(MenuScreen __instance)
    {
        if (!Plugin.ShowSkillsButton)
        {
            return;
        }

        // Coroutine: o CHARACTER pode não estar pronto e (com Menu-Overhaul) os botões são reposicionados
        // de forma assíncrona — precisamos agir DEPOIS do MO.
        Plugin.Instance?.StartCoroutine(SetupSkillsButton(__instance));
    }

    private static IEnumerator SetupSkillsButton(MenuScreen menu)
    {
        // 1) Espera o botão CHARACTER existir.
        DefaultUIButton? playerBtn = null;
        for (var i = 0; i < 120 && playerBtn == null; i++)
        {
            playerBtn = PlayerButtonField?.GetValue(menu) as DefaultUIButton;
            if (playerBtn == null || playerBtn.transform.parent == null)
            {
                playerBtn = null;
                yield return null;
            }
        }

        if (menu == null || playerBtn == null)
        {
            yield break;
        }

        var parent = playerBtn.transform.parent;

        // 2) Cria o clone 1x (idempotente) e configura o clique.
        GameObject go;
        var existing = parent.Find(SkillsButtonName);
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = UnityEngine.Object.Instantiate(playerBtn.gameObject, parent);
            go.name = SkillsButtonName;
            go.transform.SetSiblingIndex(playerBtn.transform.GetSiblingIndex() + 1);   // layout vanilla (sem MO)
            ConfigureClick(go, playerBtn);
        }

        // 3) Label no idioma do EFT (item 008).
        var label = GameLocale.IsPortuguese ? "HABILIDADES" : "SKILLS";
        foreach (var tmp in go.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.text = label;
        }

        // 4) Posicionamento. Com o Menu-Overhaul, esperar ele reposicionar e então inserir+empurrar.
        if (MenuOverhaulBridge.IsPresent)
        {
            for (var i = 0; i < 10; i++)
            {
                yield return null;   // deixa o MO rodar ProcessButtons/ApplyButtonTransform neste Show
            }

            PositionWithMenuOverhaul(parent, playerBtn, go);
        }
    }

    private static void ConfigureClick(GameObject go, DefaultUIButton playerBtn)
    {
        var btn = go.GetComponent<DefaultUIButton>();
        if (btn == null)
        {
            return;
        }

        btn.OnClick.RemoveAllListeners();   // o clone não herda listeners de runtime; garante limpo
        btn.OnClick.AddListener(delegate
        {
            try
            {
                playerBtn.OnClick.Invoke();   // abre o CHARACTER (inventário) — mesmo fluxo do botão original
                Plugin.Instance?.StartCoroutine(InventoryTabNavigator.SelectSkillsTabWhenReady());
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[CustomClasses] skills nav click falhou: {ex.Message}");
            }
        });
    }

    /// <summary>
    ///     Posiciona o SKILLS logo abaixo do Character e empurra Trade/Hideout/Exit p/ baixo, em posições
    ///     ABSOLUTAS relativas à âncora do Character (idempotente — reexecutar não acumula deslocamento).
    /// </summary>
    private static void PositionWithMenuOverhaul(Transform parent, DefaultUIButton playerBtn, GameObject skills)
    {
        if (playerBtn.transform is not RectTransform charRt || skills.transform is not RectTransform skillsRt)
        {
            return;
        }

        // SKILLS herda âncora/pivô/escala do Character e fica 1 slot abaixo.
        skillsRt.anchorMin = charRt.anchorMin;
        skillsRt.anchorMax = charRt.anchorMax;
        skillsRt.pivot = charRt.pivot;
        skillsRt.localScale = charRt.localScale;

        var baseY = charRt.anchoredPosition.y;
        skillsRt.anchoredPosition = new Vector2(charRt.anchoredPosition.x, baseY - MoButtonYOffset);

        // Trade/Hideout/Exit descem 1 slot (i+2 = abaixo do SKILLS). Absoluto p/ não acumular.
        for (var i = 0; i < MoButtonsBelowCharacter.Length; i++)
        {
            if (parent.Find(MoButtonsBelowCharacter[i]) is RectTransform rt)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, baseY - (i + 2) * MoButtonYOffset);
            }
        }
    }
}
