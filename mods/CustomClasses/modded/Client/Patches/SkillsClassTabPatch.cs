using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT.UI;   // SkillsAndMasteringScreen, UIElement, LocalizedText
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     Item 053 — controller da sub-aba CLASS. Implementa a interface nativa <c>GInterface486</c>
///     (<c>void Show()</c> + <c>Task&lt;bool&gt; TryHide()</c>) que o toggle-group (<c>GClass3808</c>) chama ao
///     selecionar/desselecionar a aba. Só liga/desliga o meu painel.
/// </summary>
internal class ClassTabController : GInterface486
{
    private readonly GameObject _panel;

    internal ClassTabController(GameObject panel)
    {
        _panel = panel;
    }

    public void Show()
    {
        _panel.SetActive(true);
        SkillsClassTabPatch.RefreshPanel(_panel);
    }

    public Task<bool> TryHide()
    {
        _panel.SetActive(false);
        return Task.FromResult(true);   // sempre permite trocar de aba
    }
}

/// <summary>
///     Item 053 — sub-aba "CLASS" (primeira: CLASS | SKILLS | MASTERING) na tela de Skills. Ao selecionar CLASS,
///     o toggle-group esconde skills/mastering (via <c>TryHide</c> deles) e mostra meu painel (Nome da Classe +
///     Perks/Drawbacks); ao clicar SKILLS/MASTERING, meu painel some — tudo pelo próprio group, com realce visual
///     correto das 3 abas. Clona a `_skillsTab`, recria o `GClass3808` com as 3 tabs. Idempotente. Via reflection
///     (campos estáveis; tipos GClassNNNN obfuscados só no <c>ClassTabController : GInterface486</c>, ref de compile).
/// </summary>
internal class SkillsClassTabPatch : ModulePatch
{
    private const string TabName = "CC_ClassTab";
    private const string PanelName = "CC_ClassPanel";

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

            // idempotência: a tela dá Show() várias vezes; só monta a aba 1×.
            if (__instance.GetComponentsInChildren<Tab>(true).Any(x => x.name == TabName))
            {
                return;
            }

            var t = typeof(SkillsAndMasteringScreen);
            var skillsTab = AccessTools.Field(t, "_skillsTab")?.GetValue(__instance) as Tab;
            var masteringTab = AccessTools.Field(t, "_masteringTab")?.GetValue(__instance) as Tab;
            var skillsScreen = AccessTools.Field(t, "_skillsScreen")?.GetValue(__instance) as UIElement;
            var groupField = AccessTools.Field(t, "gclass3808_0");
            var oldGroup = groupField?.GetValue(__instance);

            if (skillsTab == null || masteringTab == null || skillsScreen == null || oldGroup == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) tab: campos da SkillsAndMasteringScreen não resolvidos.");
                return;
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;

            // (a) clona a aba SKILLS → aba CLASS (1ª posição), troca o label.
            var classTab = UnityEngine.Object.Instantiate(skillsTab.gameObject, skillsTab.transform.parent).GetComponent<Tab>();
            classTab.name = TabName;
            classTab.transform.SetSiblingIndex(0);

            // label: LocalizedText sobrescreveria o texto → remove e seta o TMP na mão.
            foreach (var loc in classTab.GetComponentsInChildren<LocalizedText>(true))
            {
                UnityEngine.Object.Destroy(loc);
            }

            foreach (var tmp in classTab.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.text = GameLocale.IsPortuguese ? "CLASSE" : "CLASS";
            }

            // painel: filho do content pai (comum a skills/mastering), preenche a área, começa escondido.
            var panel = BuildPanel(skillsScreen.transform.parent, font);

            // (b) registra o controller na aba CLASS.
            classTab.Init(new ClassTabController(panel));

            // (c) recria o toggle-group com as 3 tabs (Close antes p/ não duplicar handlers).
            AccessTools.Method(oldGroup.GetType(), "Close")?.Invoke(oldGroup, null);

            var tabsArray = new[] { classTab, skillsTab, masteringTab };
            var ctor = AccessTools.Constructor(oldGroup.GetType(), new[] { typeof(Tab[]), typeof(Tab), typeof(bool) });
            if (ctor == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) ctor do toggle-group não encontrado.");
                return;
            }

            var newGroup = ctor.Invoke(new object[] { tabsArray, skillsTab, true });
            groupField.SetValue(__instance, newGroup);

            // reaplica a seleção inicial (SKILLS) no group novo → realça as 3 corretamente.
            AccessTools.Method(newGroup.GetType(), "Show", new[] { typeof(Tab), typeof(bool) })
                ?.Invoke(newGroup, new object[] { skillsTab, true });

            Plugin.Log?.LogInfo("[CustomClasses] (053) aba CLASS adicionada à tela de Skills.");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) aba CLASS falhou: {ex.Message}");
        }
    }

    /// <summary>Atualiza o texto do painel (chamado pelo controller ao mostrar).</summary>
    internal static void RefreshPanel(GameObject panel)
    {
        try
        {
            SkillMultipliers.EnsureLoaded();
            var tmp = panel.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = PerksCatalog.BuildPanelText() ?? (GameLocale.IsPortuguese
                    ? "Classe vanilla — sem perks/drawbacks."
                    : "Vanilla class — no perks/drawbacks.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) refresh painel falhou: {ex.Message}");
        }
    }

    private static GameObject BuildPanel(Transform contentParent, TMP_FontAsset? font)
    {
        var go = new GameObject(PanelName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(contentParent, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;   // preenche a área de conteúdo
        rt.offsetMin = new Vector2(24f, 24f);
        rt.offsetMax = new Vector2(-24f, -12f);

        go.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.05f, 0.55f);

        var body = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
        body.transform.SetParent(go.transform, false);
        var brt = (RectTransform)body.transform;
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(1f, 1f);
        brt.pivot = new Vector2(0f, 1f);
        brt.offsetMin = new Vector2(28f, 0f);
        brt.offsetMax = new Vector2(-28f, -24f);
        brt.sizeDelta = new Vector2(brt.sizeDelta.x, 800f);

        var tmp = body.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.fontSize = 24f;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.raycastTarget = false;

        go.SetActive(false);   // começa escondido (SKILLS é a aba inicial)
        return go;
    }
}
