using System;
using System.Reflection;
using EFT;          // SkillClass
using EFT.UI;       // SkillPanel, HoverTooltipArea, ItemUiContext
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;        // TextMeshProUGUI
using UnityEngine;  // GameObject, RectTransform, Vector2

namespace CustomClasses.Client;

/// <summary>
///     (010) UI — marcador "▲ +X%" / "▼ -X%" à direita do nome da skill + tooltip dedicado da classe.
///     Postfix em SkillPanel.method_1 (refresh da linha).
///     ref: Assembly-CSharp.dll → EFT.UI.SkillPanel { TextMeshProUGUI _name; SkillClass skillClass; }.
///     Não mexe mais nas setas vanilla _effectivenessUp/Down (item 005) — volta ao comportamento original.
/// </summary>
internal class SkillPanelPatch : ModulePatch
{
    private const string MarkerName = "CC_MultMarker";

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.method_1));
    }

    [PatchPostfix]
    private static void Postfix(SkillClass ___skillClass, TextMeshProUGUI ____name)
    {
        if (!Plugin.ShowOnUi || ___skillClass is null || ____name is null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();   // perfil novo (sem XP) → garante cache+ClassName ao abrir a tela
            var has = SkillMultipliers.TryGet(___skillClass.Id, out var f) && MultiplierFormat.IsActive(f);

            // Marcador reusável (a lista recicla SkillPanel ao rolar) — sempre reescrito/escondido.
            var marker = GetOrCreateMarker(____name);
            var tmp = marker.GetComponent<TextMeshProUGUI>();

            if (!has)
            {
                // ref: CR-01-02 — desativar (OnDisable fecha o tooltip) antes de mexer na mensagem
                // evita flash de tooltip vazio se o cursor estiver sobre o marcador durante o scroll.
                tmp.text = string.Empty;
                marker.SetActive(false);
                return;
            }

            tmp.text = MultiplierFormat.Marker(f);
            // ref: CR-01-03 — o SimpleTooltip é resolvido 1x na criação (GetOrCreateMarker);
            // aqui só atualiza o texto (rawText:true preserva <color>/<b>).
            marker.GetComponent<HoverTooltipArea>()
                .SetMessageText(MultiplierFormat.TooltipText(f, SkillMultipliers.ClassName), rawText: true);
            marker.SetActive(true);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skill panel marker falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     Acha (ou cria 1x) o marcador como filho do _name, herdando fonte/material do nome.
    ///     PA-01-04: ancorado esticado sobre a área do nome, texto alinhado à direita.
    /// </summary>
    private static GameObject GetOrCreateMarker(TextMeshProUGUI name)
    {
        var existing = name.transform.Find(MarkerName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var go = new GameObject(MarkerName, typeof(RectTransform));
        go.transform.SetParent(name.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = name.font;
        tmp.fontSharedMaterial = name.fontSharedMaterial;
        tmp.fontSize = name.fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineRight;   // marcador na direita da coluna do nome
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = true;                            // necessário p/ HoverTooltipArea receber o ponteiro

        // ref: CR-01-03 — resolve o SimpleTooltip uma única vez aqui (na criação); refreshes só fazem SetMessageText.
        var area = go.AddComponent<HoverTooltipArea>();       // Awake também resolve ItemUiContext.Instance.Tooltip
        area.Init(ItemUiContext.Instance.Tooltip, string.Empty, rawText: true);
        return go;
    }
}
