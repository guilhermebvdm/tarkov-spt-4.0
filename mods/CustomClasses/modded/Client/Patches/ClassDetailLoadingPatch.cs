using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomClasses.Client;

/// <summary>
///     (055) Detalhe da classe LOCAL na tela de carregamento do FIKA. Postfix soft-detect em
///     <c>LoadingScreenUI.AddPlayer(int netId, string nickname)</c>: na linha do player local
///     (nickname == <see cref="SkillMultipliers.Nickname"/>, padrão do 015) anexa um <see cref="LoadingClassHover"/>
///     que mostra o painel <see cref="PerksPanelView"/> (2 colunas). **Nenhum tipo FIKA no IL** — o alvo é resolvido
///     por <c>AccessTools.TypeByName</c> (padrão SAIN, Plugin.cs:126) e o GameObject da linha sai de
///     <c>_loadingPlayers[netId]</c> castado para <see cref="Component"/> (Unity, não FIKA). Degrada 100% solo.
///     ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97 (AddPlayer) / :14 (_loadingPlayers).
/// </summary>
internal class ClassDetailLoadingPatch : ModulePatch
{
    private static readonly Type? UiType = AccessTools.TypeByName("LoadingScreenUI");
    private static readonly FieldInfo? PlayersField = UiType != null ? AccessTools.Field(UiType, "_loadingPlayers") : null;

    protected override MethodBase GetTargetMethod()
    {
        // ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97
        return AccessTools.Method(UiType, "AddPlayer", new[] { typeof(int), typeof(string) });
    }

    [PatchPostfix]
    private static void Postfix(object __instance, int netId, string nickname)
    {
        try
        {
            if (PerksConfig.ClassDetailOnLoading?.Value != true)
            {
                return;
            }

            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.ClassName == null)
            {
                return;   // classe vanilla → sem painel
            }

            // só o player LOCAL (padrão do 015 — sem tipo FIKA). ref: PlayerNamePanelPatch.cs:48
            if (string.IsNullOrEmpty(nickname)
                || !string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal))
            {
                return;
            }

            // GameObject da linha via _loadingPlayers[netId] (LoadingScreenPlayer é MonoBehaviour → Component).
            if (PlayersField?.GetValue(__instance) is not IDictionary dict || !dict.Contains(netId))
            {
                return;
            }

            if (dict[netId] is not Component row)
            {
                return;
            }

            if (row.GetComponent<LoadingClassHover>() != null)
            {
                return;   // idempotência (re-init do loading na mesma sessão)
            }

            row.gameObject.AddComponent<LoadingClassHover>();
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (055) class detail loading: {ex.Message}");
        }
    }
}

/// <summary>
///     (055) Na linha do player local, monta e mostra o painel de detalhe da classe. PA-01-01: **auto-visível**
///     no OnEnable (a tela de carregamento é transiente e pode não ter EventSystem/raycast ativo — não dependemos
///     de hover). O hover/click é um toggle OPCIONAL (só atua se houver GraphicRaycaster + EventSystem).
///     PA-01-02: a fonte vem de um TMP da própria linha (Nickname/Percentage).
/// </summary>
internal sealed class LoadingClassHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject? _panel;

    private void OnEnable() => Show();   // auto-visível (PA-01-01)

    /// <summary>Monta (lazy) e exibe o painel — TODO o caminho protegido (F-3: OnPointerEnter fica fora do try/catch do Postfix).</summary>
    private void Show()
    {
        try
        {
            Ensure();
            if (_panel != null)
            {
                PerksPanelView.Refresh(_panel);
                _panel.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (055) show detail: {ex.Message}");
        }
    }

    private void Ensure()
    {
        if (_panel != null)
        {
            return;
        }

        var font = GetComponentInChildren<TMP_Text>()?.font;   // PA-01-02: fonte da própria linha
        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform.root;

        _panel = PerksPanelView.Build(parent, font);

        // o Build ancora "fill" (bom p/ a aba); no loading reancoramos COMPACTO à direita da tela (não cobre tudo).
        var rt = (RectTransform)_panel.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 460f);
        rt.anchoredPosition = new Vector2(-60f, 0f);
    }

    // hover = toggle OPCIONAL (refinamento; só dispara se houver GraphicRaycaster + EventSystem).
    public void OnPointerEnter(PointerEventData eventData) => Show();

    public void OnPointerExit(PointerEventData eventData)
    {
        // auto-visível: mantém aberto durante o loading (decisão de gate).
    }

    // F-9: se a linha for desativada (sem destruir), esconde o painel junto (ele é filho do Canvas, não da linha).
    private void OnDisable()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_panel != null)
        {
            Destroy(_panel);
        }
    }
}
