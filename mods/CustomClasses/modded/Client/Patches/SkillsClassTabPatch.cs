using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

            // label da aba = NOME DA CLASSE (ex.: TANK), não "CLASS" genérico — pedido do usuário.
            SkillMultipliers.EnsureLoaded();
            var tabLabel = string.IsNullOrWhiteSpace(SkillMultipliers.ClassName)
                ? (GameLocale.IsPortuguese ? "CLASSE" : "CLASS")
                : SkillMultipliers.ClassName!.ToUpperInvariant();

            // (a) clona a aba MASTERING (estado NORMAL no Postfix → clone limpo, sem a prancha "selecionada" da SKILLS).
            var classTab = UnityEngine.Object.Instantiate(masteringTab.gameObject, masteringTab.transform.parent).GetComponent<Tab>();
            classTab.name = TabName;
            classTab.transform.SetSiblingIndex(0);

            // label + ícone: LocalizedText sobrescreveria o texto → remove. Depois StyleClassTab aplica o label nas
            // DUAS versões (normal/selected) + o ícone da classe. Reaplico em cada troca de seleção (o selected vinha vazio).
            foreach (var loc in classTab.GetComponentsInChildren<LocalizedText>(true))
            {
                UnityEngine.Object.Destroy(loc);
            }

            classTab.LocalizedText = null;
            StyleClassTab(classTab, tabLabel);
            classTab.OnSelectionChanged += (tab, _) => StyleClassTab(tab, tabLabel);

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
                var spacing = mRt.anchoredPosition.x - sRt.anchoredPosition.x;   // gap natural SKILLS→MASTERING
                cRt.anchoredPosition = new Vector2(sRt.anchoredPosition.x - spacing, sRt.anchoredPosition.y);   // CLASS 1 slot à esquerda
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
            var list = panel.transform.Find("List");
            if (headerTmp == null || list == null)
            {
                return;
            }

            var font = headerTmp.font;

            // CR-01-03: só reconstrói header+cards quando a classe muda (evita flicker de Destroy-deferido e trabalho redundante).
            var cls = SkillMultipliers.ClassNameEn;
            if (list.childCount > 0 && _lastPanelClass == cls)
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

            // limpa cards antigos e reconstrói.
            for (var i = list.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(list.GetChild(i).gameObject);
            }

            var entries = PerksCatalog.LocalEntries();
            if (entries == null || entries.Length == 0)
            {
                BuildMessageCard(list, font, GameLocale.IsPortuguese
                    ? "Classe vanilla — sem perks/drawbacks."
                    : "Vanilla class — no perks/drawbacks.");
            }
            else
            {
                // req 3: agrupado em seções PERKS / DRAWBACKS com cabeçalho.
                var perks = entries.Where(e => e.IsPerk).ToArray();
                var draws = entries.Where(e => !e.IsPerk).ToArray();
                if (perks.Length > 0)
                {
                    BuildSectionHeader(list, font, "PERKS");
                    foreach (var e in perks)
                    {
                        BuildCard(list, e, font);
                    }
                }

                if (draws.Length > 0)
                {
                    BuildSectionHeader(list, font, "DRAWBACKS");
                    foreach (var e in draws)
                    {
                        BuildCard(list, e, font);
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

        // List — seções + cards. childControlHeight=true lê o preferred-height do HLG de cada card.
        var list = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
        list.transform.SetParent(go.transform, false);
        var lvl = list.GetComponent<VerticalLayoutGroup>();
        lvl.spacing = 6f;
        lvl.childControlWidth = true;
        lvl.childControlHeight = true;
        lvl.childForceExpandWidth = true;
        lvl.childForceExpandHeight = false;
        lvl.childAlignment = TextAnchor.UpperLeft;
        list.AddComponent<LayoutElement>().flexibleHeight = 1f;

        go.SetActive(false);   // começa escondido (SKILLS é a aba inicial)
        return go;
    }

    /// <summary>Card de 1 perk/drawback: acento vertical + ícone (sprite de skill) + nome (branco) + efeito (cinza).</summary>
    private static void BuildCard(Transform parent, PerksCatalog.Entry entry, TMP_FontAsset? font)
    {
        // CR-02-01/02: perk deferido (não implementado) → acento âmbar "em breve" (nem verde-ativo nem vermelho-drawback).
        var pending = entry.Pending;
        var accent = pending
            ? new Color(0.80f, 0.62f, 0.28f, 1f)
            : entry.IsPerk ? MultiplierFormat.Green : MultiplierFormat.Red;

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        var cardBg = pending
            ? new Color(0.10f, 0.09f, 0.055f, 0.55f)   // âmbar escuro
            : entry.IsPerk
                ? new Color(0.07f, 0.10f, 0.08f, 0.55f)
                : new Color(0.11f, 0.075f, 0.075f, 0.55f);
        cardImg.color = cardBg;

        // idéia 1: realce no hover (clareia o card, como as rows nativas).
        var hover = card.AddComponent<CardHover>();
        hover.Target = cardImg;
        hover.Normal = cardBg;
        hover.Hover = new Color(cardBg.r + 0.06f, cardBg.g + 0.06f, cardBg.b + 0.06f, Mathf.Min(1f, cardBg.a + 0.22f));
        var hl = card.GetComponent<HorizontalLayoutGroup>();
        hl.padding = new RectOffset(16, 16, 9, 9);
        hl.spacing = 13f;
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childControlWidth = true;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        // acento vertical — FORA do layout (ignoreLayout) + ancorado à esquerda esticando na vertical. Assim NÃO
        // propaga flexibleHeight pro card (era ISSO que fazia o List espalhar os cards com gaps enormes).
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

        // idéia 4: frame "slot" premium — borda na cor do acento + inset escuro + ícone por cima.
        var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(card.transform, false);
        var frameImg = frame.GetComponent<Image>();
        frameImg.color = new Color(accent.r, accent.g, accent.b, 0.85f);
        frameImg.raycastTarget = false;
        var fle = frame.AddComponent<LayoutElement>();
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
        var sprite = PerksCatalog.IconSprite(entry);
        if (sprite != null)
        {
            iimg.sprite = sprite;
            iimg.color = Color.white;
        }
        else
        {
            iimg.enabled = false;   // sem sprite → só o inset
        }

        // coluna de texto (nome + efeito).
        var col = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup));
        col.transform.SetParent(card.transform, false);
        var cvl = col.GetComponent<VerticalLayoutGroup>();
        cvl.spacing = 3f;
        cvl.childControlWidth = true;
        cvl.childControlHeight = true;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        cvl.childAlignment = TextAnchor.MiddleLeft;
        col.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(col.transform, false);
        var ttmp = titleGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            ttmp.font = font;
        }

        ttmp.text = pending
            ? entry.Name + $"  <size=60%><color=#cc9a3e><i>{(GameLocale.IsPortuguese ? "· em breve" : "· soon")}</i></color></size>"
            : entry.Name;
        ttmp.fontSize = 21f;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.color = Color.white;
        ttmp.raycastTarget = false;
        ttmp.enableWordWrapping = false;
        ttmp.overflowMode = TextOverflowModes.Overflow;

        if (!string.IsNullOrEmpty(entry.Effect))
        {
            var effGo = new GameObject("Effect", typeof(RectTransform), typeof(TextMeshProUGUI));
            effGo.transform.SetParent(col.transform, false);
            var etmp = effGo.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                etmp.font = font;
            }

            etmp.text = PillifyValues(entry.Effect);
            etmp.fontSize = 15.5f;
            etmp.color = new Color(0.66f, 0.66f, 0.66f, 1f);
            etmp.raycastTarget = false;
            etmp.enableWordWrapping = true;
            etmp.richText = true;
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
    ///     Req 1/2 — aplica o label da aba CLASS nas DUAS versões (normal/selected; o selected vinha sem texto) e
    ///     troca o ícone (💡 da SKILLS → brasão da classe). Reaplicável (chamado também na troca de seleção).
    ///     Só toca Images cujo GameObject se chama "*icon*" (não mexe no fundo/hover); loga os nomes p/ ajuste fino.
    /// </summary>
    private static void StyleClassTab(Tab tab, string label)
    {
        try
        {
            foreach (var tmp in tab.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.text = label;
            }

            var color = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);
            var sprite = ClassIconCache.GetTinted(SkillMultipliers.IconFile, color, color);
            if (sprite == null)
            {
                return;
            }

            var targetImage = AccessTools.Field(typeof(Tab), "_targetImage")?.GetValue(tab) as Image;
            var images = tab.GetComponentsInChildren<Image>(true);
            var iconImgs = images
                .Where(i => i != targetImage && i.gameObject.name.ToLowerInvariant().Contains("icon"))
                .ToArray();

            if (!_loggedTabImages)
            {
                _loggedTabImages = true;
                Plugin.Log?.LogInfo($"[CustomClasses][053-tabicon] images=[{string.Join(", ", images.Select(i => i.gameObject.name))}] | iconMatches={iconImgs.Length}");
            }

            // CR-03-04: o re-apply a cada seleção é intencional (o selected reseta o texto ao ativar); aqui só pulo
            // o sprite já aplicado pra evitar trabalho redundante.
            foreach (var img in iconImgs)
            {
                if (img.sprite == sprite)
                {
                    continue;
                }

                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) style tab: {ex.Message}");
        }
    }

    private static readonly Regex ValueRegex = new(@"([+\-−]?\d+%|×\d+(?:[.,]\d+)?)", RegexOptions.Compiled);

    /// <summary>
    ///     idéia 5: envolve valores (±NN%, ×N.N) num "chip" (mark + cor clara) pra virar pílula.
    ///     CR-03-06: padding com no-break space ( ) — não gera espaço-duplo com o texto ao redor nem quebra o chip.
    /// </summary>
    private static string PillifyValues(string effect)
    {
        return ValueRegex.Replace(effect, m => $"<mark=#ffffff14><b><color=#e6e6e6> {m.Value} </color></b></mark>");
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
