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
///     Item 053 — painel "Perks / Drawbacks" da classe ativa na tela de Skills, bilíngue, reusando o
///     <see cref="PerksCatalog"/> + cores do <see cref="MultiplierFormat"/>.
///     A lista de skills é um <c>FlexibleGridLayoutGroup</c> (2 colunas) → não dá pra inserir full-width DENTRO
///     do grid. Estratégia adaptativa:
///     <list type="bullet">
///       <item>Se o 'Content' (pai do grid, o content do ScrollRect) for <c>VerticalLayoutGroup</c> → insere o
///       painel ali como PRIMEIRO item (full-width, empurra as skills, rola junto). Como é o container do MODO
///       LISTA, some sozinho no MODO GALERIA (outro container).</item>
///       <item>Senão → overlay flutuante posicionável no F12 (fallback).</item>
///     </list>
///     Postfix em <c>SkillsAndMasteringScreen.Show</c>. Idempotente. Toggle F12 (<c>PerksConfig.PerksPanelEnabled</c>).
/// </summary>
internal class SkillsPerksPanelPatch : ModulePatch
{
    private const string PanelName = "CC_PerksPanel";
    private static bool _loggedLayout;

    protected override MethodBase GetTargetMethod()
    {
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

            var content = TryGetSkillsContent(__instance);
            var inline = content != null && content.GetComponent<VerticalLayoutGroup>() != null;
            var parent = inline ? content! : __instance.transform;

            // esconde no MODO GALERIA (a lista/'Content' fica inativa; o grid de ícones é outro container) — pedido do usuário.
            var galleryMode = content != null && !content.gameObject.activeInHierarchy;

            DiagLog(content, inline);

            // acha um painel já existente (em qualquer lugar da tela) — evita duplicata ao alternar inline/overlay.
            var existing = __instance.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == PanelName)?.gameObject;

            if (existing != null && existing.transform.parent != parent)
            {
                UnityEngine.Object.Destroy(existing);   // pai mudou → recria no lugar certo
                existing = null;
            }

            if (PerksConfig.PerksPanelEnabled?.Value != true)
            {
                existing?.SetActive(false);
                return;
            }

            SkillMultipliers.EnsureLoaded();
            var text = PerksCatalog.BuildPanelText();
            if (string.IsNullOrEmpty(text) || galleryMode)
            {
                existing?.SetActive(false);
                return;   // classe vanilla/sem entradas, ou modo galeria
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
            var go = existing ?? CreatePanel(parent, font, inline);
            go.SetActive(true);

            if (inline)
            {
                go.transform.SetAsFirstSibling();   // topo da lista (o EFT trata filhos não-gerenciados como header)
            }
            else
            {
                ((RectTransform)go.transform).anchoredPosition = ConfigPos();
            }

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

    /// <summary>Content do ScrollRect da lista de skills (= pai do <c>_skillsContainer</c> do <c>SkillList</c>), via reflection.</summary>
    private static Transform? TryGetSkillsContent(SkillsAndMasteringScreen screen)
    {
        try
        {
            var slType = AccessTools.TypeByName("EFT.UI.SkillList") ?? AccessTools.TypeByName("SkillList");
            var sl = slType != null ? screen.GetComponentInChildren(slType, true) : null;
            var container = sl != null ? AccessTools.Field(sl.GetType(), "_skillsContainer")?.GetValue(sl) as Transform : null;
            return container?.parent;
        }
        catch
        {
            return null;
        }
    }

    private static GameObject CreatePanel(Transform parent, TMP_FontAsset? font, bool inline)
    {
        var stale = parent.Find(PanelName);
        if (stale != null)
        {
            UnityEngine.Object.Destroy(stale.gameObject);
        }

        var go = new GameObject(PanelName,
            typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);

        go.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.06f, 0.90f);   // caixa escura (mais opaca = lê como painel)

        var vl = go.GetComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(14, 14, 10, 12);
        vl.spacing = 2f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;

        var csf = go.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = inline ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;

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

        var rt = (RectTransform)go.transform;
        if (inline)
        {
            // dentro do VerticalLayoutGroup do 'Content' — preenche a largura, altura pelo conteúdo.
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
        }
        else
        {
            // overlay flutuante — canto superior-direito, posição no F12; label com largura fixa.
            label.AddComponent<LayoutElement>().preferredWidth = 440f;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = ConfigPos();
        }

        return go;
    }

    private static Vector2 ConfigPos()
    {
        return new Vector2(PerksConfig.PerksPanelPosX?.Value ?? -24f, PerksConfig.PerksPanelPosY?.Value ?? -90f);
    }

    /// <summary>Reposiciona o overlay em tempo real quando o X/Y muda no F12 (só no modo overlay/flutuante).</summary>
    internal static void Reposition()
    {
        var go = GameObject.Find(PanelName);
        if (go != null && go.GetComponent<LayoutElement>() == null)   // só o overlay (o inline não usa pos fixa)
        {
            ((RectTransform)go.transform).anchoredPosition = ConfigPos();
        }
    }

    private static void DiagLog(Transform? content, bool inline)
    {
        if (_loggedLayout)
        {
            return;
        }

        _loggedLayout = true;
        var lg = content?.GetComponent<LayoutGroup>();
        Plugin.Log?.LogInfo($"[CustomClasses][053-diag] content='{content?.name}' layout={lg?.GetType().Name ?? "none"} → modo={(inline ? "INLINE (topo da lista)" : "OVERLAY (flutuante)")}");
    }
}
