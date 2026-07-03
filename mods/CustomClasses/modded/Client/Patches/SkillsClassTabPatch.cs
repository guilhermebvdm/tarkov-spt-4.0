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
        PerksPanelView.Refresh(_panel);   // 055: painel extraído p/ PerksPanelView (reuso com o loading da raid)
    }

    public Task<bool> TryHide()
    {
        _panel.SetActive(false);
        return Task.FromResult(true);   // sempre permite trocar de aba
    }
}

/// <summary>
///     Item 053 — sub-aba "CLASS" (primeira: CLASS | SKILLS | MASTERING) na tela de Skills. Cuida só da ABA
///     (clone do tab, toggle-group, overlay [ícone][CLASS], posição); o CONTEÚDO (header + 2 colunas de perks/
///     drawbacks) é o <see cref="PerksPanelView"/> compartilhado (059, extraído no 055).
///     <list type="bullet">
///       <item>Aba: clona a <c>_masteringTab</c> (que está em estado NORMAL no Postfix — evita herdar a prancha
///       "selecionada" da SKILLS) e recria o toggle-group (<c>GClass3808</c>) com as 3 tabs. Idempotente.</item>
///       <item>Posicionamento: CLASS vai à ESQUERDA da SKILLS; SKILLS/MASTERING ficam nas posições nativas (não
///       empurra a caixa de busca da MASTERING).</item>
///     </list>
///     Via reflection (campos estáveis; tipos GClassNNNN obfuscados só no <c>ClassTabController : GInterface486</c>, ref de compile).
/// </summary>
internal class SkillsClassTabPatch : ModulePatch
{
    private const string TabName = "CC_ClassTab";
    private const string TabOverlayName = "CC_ClassTabLabel";   // 059: overlay [ícone][CLASS] próprio sobre a aba

    private static bool _loggedTabImages;      // req 1: loga os nomes dos Images da aba 1× (ajuste fino do ícone).

    // 059 CLASS#1 (F12 live): última tela/aba montadas, p/ reposicionar no SettingChanged com a tela aberta.
    // MonoBehaviours → o == sobrecarregado do Unity detecta instância destruída (pooling/troca de cena).
    private static SkillsAndMasteringScreen? _lastScreen;
    private static Tab? _lastClassTab;
    private static bool _settingHooked;

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

            EnsureSettingHook();   // 059 CLASS#1: F12 live (reposiciona no SettingChanged)

            // idempotência: a tela dá Show() várias vezes; só monta a aba 1×.
            // CR-03-05: na reabertura (screen pooled) NÃO re-normalizo a seleção aqui — o `Show` nativo termina em
            // `gclass3808_0.Show(null)` → `SelectTab(Tab_2)`, que restaura a ÚLTIMA aba (não força SKILLS). Por isso
            // a reabertura não re-quebra o double-select. Se um build futuro do EFT trocar por `Show(_skillsTab)`,
            // revisitar (mover a des-seleção das não-CLASS pra fora deste guard).
            var existingTab = __instance.GetComponentsInChildren<Tab>(true).FirstOrDefault(x => x.name == TabName);
            if (existingTab != null)
            {
                // 059 CLASS#1: aba já montada → só REPOSICIONA (F12 live também entre aberturas da tela).
                _lastScreen = __instance;
                _lastClassTab = existingTab;
                RepositionClassTab(__instance, existingTab);
                return;
            }

            var t = typeof(SkillsAndMasteringScreen);
            var skillsTab = AccessTools.Field(t, "_skillsTab")?.GetValue(__instance) as Tab;
            var masteringTab = AccessTools.Field(t, "_masteringTab")?.GetValue(__instance) as Tab;
            var skillsScreen = AccessTools.Field(t, "_skillsScreen")?.GetValue(__instance) as UIElement;
            var groupField = AccessTools.Field(t, "gclass3808_0");
            var oldGroup = groupField?.GetValue(__instance);

            if (skillsTab == null || masteringTab == null || skillsScreen == null || groupField == null || oldGroup == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) tab: campos da SkillsAndMasteringScreen não resolvidos.");
                return;
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;

            // 059: rótulo GENÉRICO "CLASS"/"CLASSE" (não o nome da classe — o header do painel já mostra "TANK").
            SkillMultipliers.EnsureLoaded();
            var tabLabel = GameLocale.IsPortuguese ? "CLASSE" : "CLASS";

            // (a) clona a aba MASTERING (estado NORMAL no Postfix → clone limpo, sem a prancha "selecionada" da SKILLS).
            var classTab = UnityEngine.Object.Instantiate(masteringTab.gameObject, masteringTab.transform.parent).GetComponent<Tab>();
            classTab.name = TabName;
            classTab.transform.SetSiblingIndex(0);

            // 059: LocalizedText re-localizaria o texto nativo → remove. StyleClassTab esconde o conteúdo NATIVO
            // (texto + ícone, preservando o fundo) e sobrepõe um label próprio [ícone][CLASS] sempre visível.
            foreach (var loc in classTab.GetComponentsInChildren<LocalizedText>(true))
            {
                UnityEngine.Object.Destroy(loc);
            }

            classTab.LocalizedText = null;
            StyleClassTab(classTab, tabLabel);
            classTab.OnSelectionChanged += (tab, _) => StyleClassTab(tab, tabLabel);   // reaplica na (de)seleção

            // painel: filho do content pai (comum a skills/mastering), preenche a área, começa escondido.
            var panel = PerksPanelView.Build(skillsScreen.transform.parent, font);

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

            var newGroup = ctor.Invoke(new object[] { tabsArray, skillsTab, false });   // false = NÃO reordena siblings ao selecionar
            if (newGroup == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) toggle-group não instanciou.");
                return;
            }

            groupField.SetValue(__instance, newGroup);

            // CLASS é a aba DEFAULT (pedido do usuário). SelectTab NÃO deseleciona as outras → forço o visual normal
            // nelas e escondo o conteúdo de SKILLS (senão CLASS + SKILLS aparecem ambas preenchidas).
            AccessTools.Method(newGroup.GetType(), "Show", new[] { typeof(Tab), typeof(bool) })
                ?.Invoke(newGroup, new object[] { classTab, true });   // seleciona CLASS + dispara o meu painel
            skillsTab.UpdateVisual(false);
            masteringTab.UpdateVisual(false);
            skillsScreen.Close();   // esconde a lista de skills (CLASS é o default agora)
            StyleClassTab(classTab, tabLabel);   // reaplica com a versão SELECTED já ativa (garante o texto no estado ativo)

            // as abas têm posição FIXA (não é layout group). Coloco CLASS à ESQUERDA da SKILLS e NÃO mexo em
            // SKILLS/MASTERING → a caixa de busca da MASTERING (ancorada à posição nativa dela) não é empurrada.
            var mRt = (RectTransform)masteringTab.transform;
            var bar = (RectTransform)skillsTab.transform.parent;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bar);

            _lastScreen = __instance;
            _lastClassTab = classTab;
            RepositionClassTab(__instance, classTab);

            Plugin.Log?.LogInfo($"[CustomClasses][053-tabs] barLG={bar.GetComponent<LayoutGroup>()?.GetType().Name ?? "none"} | CLASS={((RectTransform)classTab.transform).anchoredPosition} w={((RectTransform)classTab.transform).rect.width:F0} | SKILLS={((RectTransform)skillsTab.transform).anchoredPosition} | MASTER={mRt.anchoredPosition}");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) aba CLASS falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     059 CLASS#1 — posiciona a aba CLASS à esquerda da SKILLS (+ offset do F12). Extraído do Postfix pra
    ///     rodar também (a) a cada Show da tela (aba montada 1× — antes o offset só valia no boot) e (b) no
    ///     SettingChanged do F12 (ajuste ao vivo com a tela aberta). No-op se a barra tiver LayoutGroup.
    /// </summary>
    private static void RepositionClassTab(SkillsAndMasteringScreen screen, Tab classTab)
    {
        try
        {
            var skillsTab = AccessTools.Field(typeof(SkillsAndMasteringScreen), "_skillsTab")?.GetValue(screen) as Tab;
            if (skillsTab == null || classTab == null)
            {
                return;
            }

            var bar = (RectTransform)skillsTab.transform.parent;
            if (bar.GetComponent<LayoutGroup>() != null)
            {
                return;   // layout group manda na posição — offset manual não se aplica
            }

            // 059: CLASS ADJACENTE à esquerda da SKILLS (largura do clone + gap). cRt.rect.width pode ser 0
            // pré-layout → proxy = largura da SKILLS (a aba clonada tem largura equivalente). +F12 offset opcional.
            const float gap = 24f;
            var sRt = (RectTransform)skillsTab.transform;
            var cRt = (RectTransform)classTab.transform;
            var classW = cRt.rect.width > 1f ? cRt.rect.width : sRt.rect.width;
            var offsetX = PerksConfig.ClassTabOffsetX?.Value ?? 0f;
            cRt.anchoredPosition = new Vector2(sRt.anchoredPosition.x - classW - gap + offsetX, sRt.anchoredPosition.y);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (059) reposition tab: {ex.Message}");
        }
    }

    /// <summary>Assina o SettingChanged do offset (1×): mexeu no slider do F12 → reposiciona na hora.</summary>
    private static void EnsureSettingHook()
    {
        if (_settingHooked || PerksConfig.ClassTabOffsetX == null)
        {
            return;
        }

        _settingHooked = true;
        PerksConfig.ClassTabOffsetX.SettingChanged += (_, _) =>
        {
            // == do Unity: instância destruída (pooling/cena) → null → ignora até a próxima montagem.
            if (_lastScreen != null && _lastClassTab != null)
            {
                RepositionClassTab(_lastScreen, _lastClassTab);
            }
        };
    }

    /// <summary>
    ///     059 — dá o rótulo "(ícone) CLASS" à aba CLASS de forma ROBUSTA: **esconde o conteúdo nativo** do Tab
    ///     (texto + ícone, preservando o fundo/`_targetImage`) e **sobrepõe um label próprio** [ícone da classe][CLASS]
    ///     como filho do root do Tab, sempre visível (independe do estado normal/selected — o selected nativo não
    ///     renderizava o texto). Idempotente (cria o overlay 1×; só reaplica texto/ícone). Reaplicável na seleção.
    /// </summary>
    private static void StyleClassTab(Tab tab, string label)
    {
        try
        {
            // overlay primeiro (idempotente) — pra excluir seus próprios filhos ao esconder o conteúdo nativo.
            var overlayTf = tab.transform.Find(TabOverlayName);
            if (overlayTf == null)
            {
                overlayTf = BuildTabOverlay(tab).transform;
            }

            overlayTf.SetAsLastSibling();   // por cima das versões normal/selected

            var targetImage = AccessTools.Field(typeof(Tab), "_targetImage")?.GetValue(tab) as Image;   // ref: Tab.cs:26 (fundo — preservar)

            // esconde o TEXTO nativo (fora do overlay).
            foreach (var tmp in tab.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (!tmp.transform.IsChildOf(overlayTf))
                {
                    tmp.text = "";
                }
            }

            // esconde os ÍCONES nativos (Images "*icon*"), preservando o fundo (`_targetImage`) e o overlay.
            var images = tab.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == targetImage || img.transform.IsChildOf(overlayTf))
                {
                    continue;
                }

                if (img.gameObject.name.ToLowerInvariant().Contains("icon"))
                {
                    img.gameObject.SetActive(false);
                }
            }

            if (!_loggedTabImages)
            {
                _loggedTabImages = true;
                Plugin.Log?.LogInfo($"[CustomClasses][053-tabicon] images=[{string.Join(", ", images.Select(i => i.gameObject.name))}]");
            }

            // aplica o rótulo + o ícone da classe no MEU overlay.
            var otmp = overlayTf.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (otmp != null)
            {
                otmp.text = label;
            }

            var oicon = overlayTf.Find("Icon")?.GetComponent<Image>();
            ClassIdentityView.ApplyClassIcon(oicon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, 22f);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (059) style tab: {ex.Message}");
        }
    }

    /// <summary>Cria o overlay [ícone][CLASS] sobre a aba (idempotente). Fundo/hover nativos ficam por baixo.</summary>
    private static GameObject BuildTabOverlay(Tab tab)
    {
        var font = tab.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;

        var go = new GameObject(TabOverlayName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(tab.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;   // preenche a aba
        rt.offsetMin = new Vector2(16f, 0f);
        rt.offsetMax = new Vector2(-10f, 0f);

        var hl = go.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8f;
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childControlWidth = true;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        icon.transform.SetParent(go.transform, false);
        var iimg = icon.GetComponent<Image>();
        iimg.raycastTarget = false;
        iimg.preserveAspect = true;
        icon.SetActive(false);   // ApplyClassIcon reativa quando há sprite

        var txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        var tmp = txt.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return go;
    }
}
