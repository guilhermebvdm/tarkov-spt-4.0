using System;
using System.Linq;
using System.Reflection;
using EFT.UI;   // SkillsAndMasteringScreen
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     Item 053 — painel "Perks / Drawbacks" na tela de Skills, listando os perks 🔧 (verde) e drawbacks 🔻
///     (vermelho) da classe ativa, bilíngue (idioma do EFT). Reusa o <see cref="PerksCatalog"/> + as cores do
///     <see cref="MultiplierFormat"/> (mesmo padrão da tela de Skills). Postfix em
///     <c>SkillsAndMasteringScreen.Show</c> (mesmo hook do selo de identidade). Idempotente (find-or-create).
///     Toggle no F12 (<c>PerksConfig.PerksPanelEnabled</c>). Ancorado no canto superior-direito.
///     (MVP em painel — uma aba de tab-control nativa do EFT é refinamento futuro.)
/// </summary>
internal class SkillsPerksPanelPatch : ModulePatch
{
    private const string PanelName = "CC_PerksPanel";

    protected override MethodBase GetTargetMethod()
    {
        // 3 params (Profile, InventoryController, IHealthController) — robusto a sobrecargas.
        return AccessTools.GetDeclaredMethods(typeof(SkillsAndMasteringScreen))
            .First(m => m.Name == nameof(SkillsAndMasteringScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(SkillsAndMasteringScreen __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            var existing = __instance.transform.Find(PanelName);

            if (PerksConfig.PerksPanelEnabled?.Value != true)
            {
                existing?.gameObject.SetActive(false);
                return;
            }

            SkillMultipliers.EnsureLoaded();
            var text = PerksCatalog.BuildPanelText();
            if (string.IsNullOrEmpty(text))
            {
                existing?.gameObject.SetActive(false);
                return;   // classe vanilla / sem entradas
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
            var go = existing != null && existing.Find("Label") != null
                ? existing.gameObject
                : CreatePanel(__instance.transform, font);
            go.SetActive(true);

            var tmp = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                tmp.font = font;
            }

            tmp.text = text;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] perks panel falhou: {ex.Message}");
        }
    }

    private static GameObject CreatePanel(Transform parent, TMP_FontAsset? font)
    {
        var stale = parent.Find(PanelName);
        if (stale != null)
        {
            UnityEngine.Object.Destroy(stale.gameObject);
        }

        var go = new GameObject(PanelName,
            typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);

        go.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f, 0.60f);   // caixa escura translúcida (estilo EFT)

        var vl = go.GetComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(14, 14, 12, 12);
        vl.spacing = 2f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;

        var csf = go.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(go.transform, false);
        var tmp = label.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.fontSize = 18f;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;

        var le = label.AddComponent<LayoutElement>();
        le.preferredWidth = 440f;   // largura do painel (o texto quebra linha aqui)

        // canto superior-direito da tela de Skills (evita o selo de identidade, que fica no topo-centro).
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-24f, -90f);
        return go;
    }
}
