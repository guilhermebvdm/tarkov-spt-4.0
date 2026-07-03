using System;
using System.Reflection;
using EFT.UI;          // HoverTooltipArea, ItemUiContext
using EFT.UI.Health;   // HealthParametersPanel, HealthParameterPanel
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (056) Marcador "▲ +X%" + tooltip no LIMITE DE PESO (aba Health), atribuindo o bônus ao Pack Mule da classe.
///     O número do peso JÁ reflete o perk (PackMulePatch postfixa <c>SkillManager.CarryingWeightRelativeModifier</c>,
///     que entra no <c>Max</c> do peso) — aqui só ANOTAMOS a origem. Postfix em <c>HealthParametersPanel.method_0</c>
///     (refresh chamado no fim do Show, após os BindEvents setarem o peso, e no Update — idempotente).
///     Gate = mesmo do Pack Mule (classe local Scavenger/Tank + F12). Molde do marcador = <see cref="SkillPanelPatch"/>.
///     ref: Assembly-CSharp.dll → EFT.UI.Health.HealthParametersPanel { HealthParameterPanel _weight; void method_0(); }
///          EFT.UI.Health.HealthParameterPanel { TMP_Text _maxValue; }
/// </summary>
internal class WeightMarkerPatch : ModulePatch
{
    private const string MarkerName = "CC_WeightMarker";
    private const float MarkerGap = 14f;   // px após o fim do "/NN" do peso (não sobrepor)

    private static readonly FieldInfo? WeightField = AccessTools.Field(typeof(HealthParametersPanel), "_weight");
    private static readonly FieldInfo? MaxValueField = AccessTools.Field(typeof(HealthParameterPanel), "_maxValue");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HealthParametersPanel), "method_0");   // ref: HealthParametersPanel.cs:169
    }

    [PatchPostfix]
    private static void Postfix(HealthParametersPanel __instance)
    {
        try
        {
            var maxTmp = WeightField?.GetValue(__instance) is HealthParameterPanel weight
                ? MaxValueField?.GetValue(weight) as TMP_Text
                : null;
            if (maxTmp == null)
            {
                return;   // painel sem sub-painel de peso
            }

            // gate = mesmo do Pack Mule (classe local + F12), + indicador de UI ligado, + fator ≠ 1.
            var factor = 1f + (PerksConfig.PackMuleCarryBonus?.Value ?? 0f);
            var show = Plugin.ShowOnUi
                       && PerksConfig.PackMuleEnabled?.Value == true
                       && MultiplierFormat.IsActive(factor);
            if (show)
            {
                SkillMultipliers.EnsureLoaded();
                show = SkillMultipliers.IsLocalClass("Scavenger") || SkillMultipliers.IsLocalClass("Tank");
            }

            // marcador reusável (idempotente) — sempre reescrito/escondido.
            var marker = GetOrCreateMarker(maxTmp);
            var tmp = marker.GetComponent<TextMeshProUGUI>();

            if (!show)
            {
                tmp.text = string.Empty;
                marker.SetActive(false);
                return;
            }

            tmp.text = MultiplierFormat.Marker(factor);
            // posiciona logo após o fim do "/NN" do peso (padrão SkillPanelPatch) + offset X/Y do F12 (056: ajuste fino).
            var offX = PerksConfig.WeightMarkerOffsetX?.Value ?? 0f;
            var offY = PerksConfig.WeightMarkerOffsetY?.Value ?? 0f;
            ((RectTransform)marker.transform).anchoredPosition = new Vector2(maxTmp.preferredWidth + MarkerGap + offX, offY);
            marker.GetComponent<HoverTooltipArea>()
                .SetMessageText(MultiplierFormat.CarryTooltip(factor, SkillMultipliers.ClassName), rawText: true);
            marker.SetActive(true);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (056) weight marker falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     Acha (ou cria 1×) o marcador como filho do TMP do valor (herda fonte/material). Ancorado à ESQUERDA
    ///     (pivot 0, cresce p/ direita); a posição X final é definida no Postfix via <c>preferredWidth</c>.
    ///     Igual ao <see cref="SkillPanelPatch"/>, mas ancorado a um <see cref="TMP_Text"/>.
    /// </summary>
    private static GameObject GetOrCreateMarker(TMP_Text anchor)
    {
        var existing = anchor.transform.Find(MarkerName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var go = new GameObject(MarkerName, typeof(RectTransform));
        go.transform.SetParent(anchor.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(160f, 28f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = anchor.font;
        tmp.fontSharedMaterial = anchor.fontSharedMaterial;
        tmp.fontSize = anchor.fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = true;   // necessário p/ o HoverTooltipArea receber o ponteiro

        var area = go.AddComponent<HoverTooltipArea>();   // Awake resolve ItemUiContext.Instance.Tooltip
        area.Init(ItemUiContext.Instance.Tooltip, string.Empty, rawText: true);
        return go;
    }
}
