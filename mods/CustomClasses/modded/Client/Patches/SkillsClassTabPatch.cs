using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT.UI;   // SkillsAndMasteringScreen, UIElement, LocalizedText
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
///     Item 053 — sub-aba "CLASS" (primeira: CLASS | SKILLS | MASTERING) na tela de Skills: lista de cards de
///     perks/drawbacks da classe local, em seções PERKS/DRAWBACKS, com ícone de skill temático, acento verde/vermelho,
///     header com o brasão da classe, marca d'água e fade-in.
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
    private const string PanelName = "CC_ClassPanel";
    private const string TabOverlayName = "CC_ClassTabLabel";   // 059: overlay [ícone][CLASS] próprio sobre a aba

    private static string? _lastPanelClass;   // CR-01-03: evita rebuild dos cards quando a classe não mudou.
    private static bool _loggedTabImages;      // req 1: loga os nomes dos Images da aba 1× (ajuste fino do ícone).

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
            // CR-03-05: na reabertura (screen pooled) NÃO re-normalizo a seleção aqui — o `Show` nativo termina em
            // `gclass3808_0.Show(null)` → `SelectTab(Tab_2)`, que restaura a ÚLTIMA aba (não força SKILLS). Por isso
            // a reabertura não re-quebra o double-select. Se um build futuro do EFT trocar por `Show(_skillsTab)`,
            // revisitar (mover a des-seleção das não-CLASS pra fora deste guard).
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
            var sRt = (RectTransform)skillsTab.transform;
            var mRt = (RectTransform)masteringTab.transform;
            var cRt = (RectTransform)classTab.transform;
            var bar = (RectTransform)skillsTab.transform.parent;
            var barLg = bar.GetComponent<LayoutGroup>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bar);

            if (barLg == null)
            {
                // 059: CLASS ADJACENTE à esquerda da SKILLS (largura do clone + gap). cRt.rect.width pode ser 0
                // pré-layout → proxy = largura da SKILLS (a aba clonada tem largura equivalente). +F12 offset opcional.
                const float gap = 24f;
                var classW = cRt.rect.width > 1f ? cRt.rect.width : sRt.rect.width;
                var offsetX = PerksConfig.ClassTabOffsetX?.Value ?? 0f;
                cRt.anchoredPosition = new Vector2(sRt.anchoredPosition.x - classW - gap + offsetX, sRt.anchoredPosition.y);
            }

            Plugin.Log?.LogInfo($"[CustomClasses][053-tabs] barLG={barLg?.GetType().Name ?? "none"} | CLASS={cRt.anchoredPosition} w={cRt.rect.width:F0} | SKILLS={sRt.anchoredPosition} | MASTER={mRt.anchoredPosition}");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) aba CLASS falhou: {ex.Message}");
        }
    }

    /// <summary>Reconstrói o painel (header + lista de cards) — chamado pelo controller ao mostrar a aba.</summary>
    internal static void RefreshPanel(GameObject panel)
    {
        try
        {
            SkillMultipliers.EnsureLoaded();
            panel.GetComponent<FadeIn>()?.Restart();   // CR-03-03: re-dispara o fade a cada exibição da aba
            var headerTmp = panel.transform.Find("Header/HeaderText")?.GetComponent<TextMeshProUGUI>();
            var headerIcon = panel.transform.Find("Header/Icon")?.GetComponent<Image>();
            var perksCol = panel.transform.Find("Columns/PerksCol");
            var drawbacksCol = panel.transform.Find("Columns/DrawbacksCol");
            if (headerTmp == null || perksCol == null || drawbacksCol == null)
            {
                return;
            }

            var font = headerTmp.font;

            // CR-01-03: só reconstrói quando a classe muda (evita flicker de Destroy-deferido e trabalho redundante).
            var cls = SkillMultipliers.ClassNameEn;
            if ((perksCol.childCount > 0 || drawbacksCol.childCount > 0) && _lastPanelClass == cls)
            {
                return;
            }

            _lastPanelClass = cls;

            // header: ícone da classe (brasão) + nome (cor da classe) + subtítulo esmaecido.
            ClassIdentityView.ApplyClassIcon(headerIcon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, 40f);

            // idéia 3: marca d'água = brasão da classe, bem apagado.
            var watermark = panel.transform.Find("Watermark")?.GetComponent<Image>();
            if (watermark != null)
            {
                var wmColor = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);
                var wmSprite = ClassIconCache.GetTinted(SkillMultipliers.IconFile, wmColor, wmColor);
                if (wmSprite != null)
                {
                    watermark.sprite = wmSprite;
                    watermark.color = new Color(1f, 1f, 1f, 0.05f);
                    watermark.gameObject.SetActive(true);   // CR-03-01: só ativa a marca d'água quando há brasão
                }
            }
            var name = SkillMultipliers.ClassName;
            var classHex = string.IsNullOrWhiteSpace(SkillMultipliers.NameColor) ? "#ffffff" : SkillMultipliers.NameColor;
            var sub = GameLocale.IsPortuguese ? "Perks e Drawbacks" : "Perks & Drawbacks";
            headerTmp.text = string.IsNullOrEmpty(name)
                ? sub
                : $"<b><color={classHex}>{name.ToUpperInvariant()}</color></b>   <size=55%><color=#7a7a7a><i>{sub}</i></color></size>";

            // limpa as duas colunas e reconstrói.
            ClearChildren(perksCol);
            ClearChildren(drawbacksCol);

            var groups = PerksCatalog.LocalGroups();
            if (groups == null || groups.Length == 0)
            {
                // vanilla (edge raro — classe não-mod): mensagem na coluna esquerda.
                BuildMessageCard(perksCol, font, GameLocale.IsPortuguese
                    ? "Classe vanilla — sem perks/drawbacks."
                    : "Vanilla class — no perks/drawbacks.");
            }
            else
            {
                // 059: perks à ESQUERDA, drawbacks à DIREITA. Um card por grupo, efeitos em linhas.
                var perks = groups.Where(g => g.IsPerk).ToArray();
                var draws = groups.Where(g => !g.IsPerk).ToArray();
                if (perks.Length > 0)
                {
                    BuildSectionHeader(perksCol, font, "PERKS");
                    foreach (var g in perks)
                    {
                        BuildGroupCard(perksCol, g, font);
                    }
                }

                if (draws.Length > 0)
                {
                    BuildSectionHeader(drawbacksCol, font, "DRAWBACKS");
                    foreach (var g in draws)
                    {
                        BuildGroupCard(drawbacksCol, g, font);
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) refresh painel falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     Painel = caixa escura (fill) com VerticalLayoutGroup: [Header (brasão + nome)] [List (seções + cards)]
    ///     + marca d'água (brasão apagado atrás) + fade-in. Coluna única (sem boneco 3D). Começa escondido.
    /// </summary>
    private static GameObject BuildPanel(Transform contentParent, TMP_FontAsset? font)
    {
        var go = new GameObject(PanelName,
            typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(CanvasGroup), typeof(FadeIn));
        go.transform.SetParent(contentParent, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;   // preenche a área de conteúdo
        rt.offsetMin = new Vector2(24f, 24f);
        rt.offsetMax = new Vector2(-24f, -12f);

        go.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.05f, 0.35f);

        // idéia 3: marca d'água — brasão apagado, atrás de tudo (ignora o layout). Sprite setado no RefreshPanel.
        var wm = new GameObject("Watermark", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        wm.transform.SetParent(go.transform, false);
        wm.GetComponent<LayoutElement>().ignoreLayout = true;
        var wmRt = (RectTransform)wm.transform;
        wmRt.anchorMin = wmRt.anchorMax = new Vector2(1f, 0.5f);
        wmRt.pivot = new Vector2(1f, 0.5f);
        wmRt.sizeDelta = new Vector2(460f, 460f);
        wmRt.anchoredPosition = new Vector2(-40f, 0f);
        var wmImg = wm.GetComponent<Image>();
        wmImg.raycastTarget = false;
        wmImg.preserveAspect = true;
        wmImg.color = new Color(1f, 1f, 1f, 0.05f);
        wm.SetActive(false);   // CR-03-01: sem sprite = quad branco → ativa só no RefreshPanel quando houver brasão

        var vl = go.GetComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(28, 28, 20, 20);
        vl.spacing = 12f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.childAlignment = TextAnchor.UpperLeft;

        // Header horizontal: [ícone da classe] [nome + subtítulo]. Ícone + texto setados no RefreshPanel.
        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(go.transform, false);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.spacing = 12f;
        hhl.childAlignment = TextAnchor.MiddleLeft;
        hhl.childControlWidth = true;
        hhl.childControlHeight = true;
        hhl.childForceExpandWidth = false;
        hhl.childForceExpandHeight = false;
        header.AddComponent<LayoutElement>().minHeight = 46f;

        var hicon = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        hicon.transform.SetParent(header.transform, false);
        var hiconImg = hicon.GetComponent<Image>();
        hiconImg.raycastTarget = false;
        hiconImg.preserveAspect = true;
        hicon.SetActive(false);   // CR-03-01: ApplyClassIcon reativa no sucesso; sem sprite fica inativo (sem quad branco)

        var htextGo = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI));
        htextGo.transform.SetParent(header.transform, false);
        var htmp = htextGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            htmp.font = font;
        }

        htmp.fontSize = 28f;
        htmp.richText = true;
        htmp.enableWordWrapping = false;
        htmp.alignment = TextAlignmentOptions.Left;
        htmp.raycastTarget = false;

        // 059: Columns — 2 colunas lado a lado. PerksCol (esquerda) / DrawbacksCol (direita).
        var columns = new GameObject("Columns", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        columns.transform.SetParent(go.transform, false);
        columns.AddComponent<LayoutElement>().flexibleHeight = 1f;
        var chl = columns.GetComponent<HorizontalLayoutGroup>();
        chl.spacing = 24f;
        chl.childControlWidth = true;
        chl.childControlHeight = true;
        chl.childForceExpandWidth = true;
        chl.childForceExpandHeight = true;
        chl.childAlignment = TextAnchor.UpperLeft;

        BuildColumn(columns.transform, "PerksCol");
        BuildColumn(columns.transform, "DrawbacksCol");

        go.SetActive(false);   // começa escondido (SKILLS é a aba inicial)
        return go;
    }

    /// <summary>Uma coluna do painel (VLG, ~50% largura): section header + group-cards.</summary>
    private static void BuildColumn(Transform parent, string name)
    {
        var col = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        col.transform.SetParent(parent, false);
        col.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var vl = col.GetComponent<VerticalLayoutGroup>();
        vl.spacing = 6f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.childAlignment = TextAnchor.UpperLeft;
    }

    /// <summary>
    ///     059 — card de um <see cref="PerksCatalog.PerkGroup"/>: acento + frame do ícone + [Nome do perk] e
    ///     **uma linha por efeito atômico** (chip do ValueToken colorido por line.IsPerk + label). Linha/grupo
    ///     deferido → "· em breve". A cor/seção do grupo saem de <c>group.IsPerk</c> (derivado).
    /// </summary>
    private static void BuildGroupCard(Transform parent, PerksCatalog.PerkGroup group, TMP_FontAsset? font)
    {
        var allPending = group.AllPending;
        var accent = allPending
            ? new Color(0.80f, 0.62f, 0.28f, 1f)
            : group.IsPerk ? MultiplierFormat.Green : MultiplierFormat.Red;

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        var cardBg = allPending
            ? new Color(0.10f, 0.09f, 0.055f, 0.55f)
            : group.IsPerk
                ? new Color(0.07f, 0.10f, 0.08f, 0.55f)
                : new Color(0.11f, 0.075f, 0.075f, 0.55f);
        cardImg.color = cardBg;

        var hover = card.AddComponent<CardHover>();   // idéia 1: realce no hover
        hover.Target = cardImg;
        hover.Normal = cardBg;
        hover.Hover = new Color(cardBg.r + 0.06f, cardBg.g + 0.06f, cardBg.b + 0.06f, Mathf.Min(1f, cardBg.a + 0.22f));
        var hl = card.GetComponent<HorizontalLayoutGroup>();
        hl.padding = new RectOffset(16, 16, 10, 10);
        hl.spacing = 13f;
        hl.childAlignment = TextAnchor.UpperLeft;   // ícone no topo, nome + linhas fluindo (card multi-linha)
        hl.childControlWidth = true;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        // acento vertical — FORA do layout (ignoreLayout) + âncoras: NÃO propaga flexibleHeight pro card.
        var accentGo = new GameObject("Accent", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        accentGo.transform.SetParent(card.transform, false);
        accentGo.GetComponent<LayoutElement>().ignoreLayout = true;
        var accentRt = (RectTransform)accentGo.transform;
        accentRt.anchorMin = new Vector2(0f, 0f);
        accentRt.anchorMax = new Vector2(0f, 1f);
        accentRt.pivot = new Vector2(0f, 0.5f);
        accentRt.sizeDelta = new Vector2(4f, 0f);
        accentRt.anchoredPosition = Vector2.zero;
        var accentImg = accentGo.GetComponent<Image>();
        accentImg.color = accent;
        accentImg.raycastTarget = false;

        // frame "slot" — borda na cor do acento + inset escuro + ícone por cima.
        var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        frame.transform.SetParent(card.transform, false);
        var frameImg = frame.GetComponent<Image>();
        frameImg.color = new Color(accent.r, accent.g, accent.b, 0.85f);
        frameImg.raycastTarget = false;
        var fle = frame.GetComponent<LayoutElement>();
        fle.minWidth = 46f;
        fle.preferredWidth = 46f;
        fle.minHeight = 46f;
        fle.preferredHeight = 46f;

        var inset = new GameObject("Inset", typeof(RectTransform), typeof(Image));
        inset.transform.SetParent(frame.transform, false);
        var insetRt = (RectTransform)inset.transform;
        insetRt.anchorMin = Vector2.zero;
        insetRt.anchorMax = Vector2.one;
        insetRt.offsetMin = new Vector2(2f, 2f);
        insetRt.offsetMax = new Vector2(-2f, -2f);
        var insetImg = inset.GetComponent<Image>();
        insetImg.color = new Color(0.05f, 0.06f, 0.07f, 1f);
        insetImg.raycastTarget = false;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(inset.transform, false);
        var irt = (RectTransform)icon.transform;
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(6f, 6f);
        irt.offsetMax = new Vector2(-6f, -6f);
        var iimg = icon.GetComponent<Image>();
        iimg.preserveAspect = true;
        iimg.raycastTarget = false;
        var sprite = PerksCatalog.IconSprite(group);
        if (sprite != null)
        {
            iimg.sprite = sprite;
            iimg.color = Color.white;
        }
        else
        {
            iimg.enabled = false;
        }

        // coluna de texto: [Nome do perk] + uma linha por efeito.
        var col = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup));
        col.transform.SetParent(card.transform, false);
        var cvl = col.GetComponent<VerticalLayoutGroup>();
        cvl.spacing = 3f;
        cvl.childControlWidth = true;
        cvl.childControlHeight = true;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        cvl.childAlignment = TextAnchor.UpperLeft;
        col.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var nameGo = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGo.transform.SetParent(col.transform, false);
        var ntmp = nameGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            ntmp.font = font;
        }

        ntmp.text = allPending
            ? group.Name + $"  <size=60%><color=#cc9a3e><i>{(GameLocale.IsPortuguese ? "· em breve" : "· soon")}</i></color></size>"
            : group.Name;
        ntmp.fontSize = 20f;
        ntmp.fontStyle = FontStyles.Bold;
        ntmp.color = Color.white;
        ntmp.raycastTarget = false;
        ntmp.enableWordWrapping = true;   // 059-CR: nome quebra linha em coluna estreita (evita transbordar o card)
        ntmp.overflowMode = TextOverflowModes.Overflow;

        foreach (var line in group.Lines)
        {
            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(TextMeshProUGUI));
            lineGo.transform.SetParent(col.transform, false);
            var ltmp = lineGo.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                ltmp.font = font;
            }

            var hex = line.Pending ? "#cc9a3e" : (line.IsPerk ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex);
            var chip = line.ValueToken.Length > 0 ? $"<b><color={hex}>{line.ValueToken}</color></b> " : "";
            var soon = line.Pending && !allPending
                ? $"  <size=80%><color=#cc9a3e><i>{(GameLocale.IsPortuguese ? "· em breve" : "· soon")}</i></color></size>"
                : "";
            ltmp.text = chip + $"<color=#a8a8a8>{line.Label}</color>" + soon;
            ltmp.fontSize = 15f;
            ltmp.color = Color.white;
            ltmp.raycastTarget = false;
            ltmp.enableWordWrapping = true;
            ltmp.richText = true;
        }
    }

    /// <summary>Card simples de mensagem (classe vanilla / sem entradas).</summary>
    private static void BuildMessageCard(Transform parent, TMP_FontAsset? font, string message)
    {
        var go = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.text = message;
        tmp.fontSize = 20f;
        tmp.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        go.AddComponent<LayoutElement>().minHeight = 40f;
    }

    /// <summary>Cabeçalho de seção (PERKS / DRAWBACKS) — rótulo esmaecido, maiúsculo, com tracking.</summary>
    private static void BuildSectionHeader(Transform parent, TMP_FontAsset? font, string text)
    {
        var go = new GameObject("Section", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.text = text;
        tmp.fontSize = 15f;
        tmp.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        tmp.characterSpacing = 6f;
        tmp.color = new Color(0.55f, 0.57f, 0.60f, 1f);
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        go.GetComponent<LayoutElement>().minHeight = 28f;

        // idéia 2: linha divisória fininha na base do rótulo.
        var line = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(go.transform, false);
        var lrt = (RectTransform)line.transform;
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot = new Vector2(0.5f, 0f);
        lrt.sizeDelta = new Vector2(0f, 1f);
        lrt.anchoredPosition = Vector2.zero;
        var limg = line.GetComponent<Image>();
        limg.color = new Color(1f, 1f, 1f, 0.08f);
        limg.raycastTarget = false;
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


    /// <summary>Destrói todos os filhos de um container (limpeza de coluna antes do rebuild).</summary>
    private static void ClearChildren(Transform t)
    {
        for (var i = t.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}

/// <summary>idéia 1: realce do card no hover (troca a cor do Image de fundo).</summary>
internal sealed class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image? Target;
    public Color Normal;
    public Color Hover;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Target != null)
        {
            Target.color = Hover;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Target != null)
        {
            Target.color = Normal;
        }
    }
}

/// <summary>
///     idéia 6: fade-in do painel (lerpa o alpha do CanvasGroup ao ativar). Tempo unscaled (menu).
///     CR-03-03: o Update se auto-desliga (enabled=false) ao terminar; o RefreshPanel chama Restart() em cada
///     exibição da aba pra re-disparar o fade (o SetActive não re-dispara OnEnable de um componente disabled).
/// </summary>
internal sealed class FadeIn : MonoBehaviour
{
    private const float Duration = 0.22f;
    private CanvasGroup? _cg;
    private float _t;

    private void OnEnable() => Restart();

    internal void Restart()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg != null)
        {
            _cg.alpha = 0f;
        }

        _t = 0f;
        enabled = true;
    }

    private void Update()
    {
        if (_cg == null)
        {
            enabled = false;
            return;
        }

        _t += Time.unscaledDeltaTime;
        _cg.alpha = Mathf.Clamp01(_t / Duration);
        if (_cg.alpha >= 1f)
        {
            enabled = false;   // CR-03-03: para o Update quando o fade termina
        }
    }
}
