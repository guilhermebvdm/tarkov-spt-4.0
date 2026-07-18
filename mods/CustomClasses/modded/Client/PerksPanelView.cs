using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     059/055 — painel REUTILIZÁVEL de detalhe da classe: header (brasão + nome) + 2 colunas (perks à esquerda,
///     drawbacks à direita), UM card por EFEITO atômico (CLASS#3) com o ícone de buff da tela de Skills,
///     marca d'água e fade-in. Reusa o <see cref="PerksCatalog"/>
///     (dados derivados). Usado por dois pontos de entrada: a aba CLASS na tela de Skills (053/059,
///     <see cref="SkillsClassTabPatch"/>) e o loading da raid no FIKA (055, <see cref="ClassDetailLoadingPatch"/>).
///     Extraído do <see cref="SkillsClassTabPatch"/> no 055 (DRY). Só exibição. 057: parametrizado por
///     <see cref="ClassIdentities.Identity"/> (qualquer classe — per-player no loading); wrapper sem identidade
///     mantém os call-sites locais. Idempotência per-panel via <see cref="PanelState"/> (N painéis no loading).
/// </summary>
internal static class PerksPanelView
{
    internal const string PanelName = "CC_ClassPanel";

    /// <summary>
    ///     057 PA-01-07 — idempotência PER-PANEL (CR-01-03 do 059 era estático; no loading coexistem N painéis,
    ///     um por linha de player): guarda a última classe renderizada NESTE painel.
    /// </summary>
    internal sealed class PanelState : MonoBehaviour
    {
        public string? LastClass;
    }

    /// <summary>
    ///     Painel = caixa escura (fill) com VerticalLayoutGroup: [Header (brasão + nome)] [Columns (perks | drawbacks)]
    ///     + marca d'água (brasão apagado atrás) + fade-in. Começa escondido.
    /// </summary>
    internal static GameObject Build(Transform contentParent, TMP_FontAsset? font)
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

        // idéia 3: marca d'água — brasão apagado, atrás de tudo (ignora o layout). Sprite setado no Refresh.
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
        wm.SetActive(false);   // CR-03-01: sem sprite = quad branco → ativa só no Refresh quando houver brasão

        var vl = go.GetComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(28, 28, 20, 20);
        vl.spacing = 12f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.childAlignment = TextAnchor.UpperLeft;

        // Header horizontal: [ícone da classe] [nome + subtítulo]. Ícone + texto setados no Refresh.
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

        // 068: linha de descrição/mérito da classe (flavor), abaixo do header e acima das colunas. Texto vem do
        // .jsonc via a rota (identity.Description), setado no Refresh; começa oculta (só aparece se houver texto).
        var flavorGo = new GameObject("FlavorText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        flavorGo.transform.SetParent(go.transform, false);
        var flavorTmp = flavorGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            flavorTmp.font = font;
        }

        flavorTmp.fontSize = 15f;
        flavorTmp.richText = true;
        flavorTmp.raycastTarget = false;
        flavorTmp.enableWordWrapping = true;
        flavorTmp.fontStyle = FontStyles.Italic;
        flavorTmp.alignment = TextAlignmentOptions.TopLeft;
        flavorTmp.color = new Color(0.62f, 0.64f, 0.67f, 1f);
        flavorGo.SetActive(false);

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

        // 060 (RN-04): rodapé informativo da Weapon Mastery — texto setado no Refresh (valores vivos do F12).
        var footer = new GameObject("MasteryFooter", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        footer.transform.SetParent(go.transform, false);
        var ftmp = footer.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            ftmp.font = font;
        }

        ftmp.fontSize = 13f;
        ftmp.richText = true;
        ftmp.raycastTarget = false;
        ftmp.enableWordWrapping = true;
        ftmp.alignment = TextAlignmentOptions.BottomLeft;
        ftmp.color = new Color(0.45f, 0.47f, 0.50f, 1f);
        footer.GetComponent<LayoutElement>().minHeight = 22f;
        footer.SetActive(false);   // Refresh ativa quando WeaponMasteryEnabled

        go.SetActive(false);   // começa escondido
        return go;
    }

    /// <summary>
    ///     060 (RN-04) — popula o rodapé da Weapon Mastery com os valores VIVOS do F12 (a tela SKILLS não mostra
    ///     buff nenhum pras 4 maestrias do 058 — este rodapé é a transparência mínima do efeito por nível).
    /// </summary>
    private static void RefreshMasteryFooter(GameObject panel, bool show)
    {
        var footer = panel.transform.Find("MasteryFooter")?.GetComponent<TextMeshProUGUI>();
        if (footer == null)
        {
            return;
        }

        // (review CR-060-01/03) o footer descreve o F12 do CLIENT LOCAL (mecânica 058 é local) — no popover
        // per-player do deploy ele atribuiria a MINHA config ao teammate, além de apertar a altura fixa.
        // Só aparece no host da aba CLASS (show=true).
        var on = show && PerksConfig.WeaponMasteryEnabled?.Value == true;
        footer.gameObject.SetActive(on);
        if (!on)
        {
            return;
        }

        var rec = (PerksConfig.MasteryRecoilPerLevel?.Value ?? 0f) * 100f;
        var ergo = (PerksConfig.MasteryErgoPerLevel?.Value ?? 0f) * 100f;
        footer.text = GameLocale.IsPortuguese
            ? $"<b>WEAPON MASTERY</b>  <color=#8a8a8a>SMG · LMG · Lançador · Underbarrel:</color> −{rec:0.#}% recuo · +{ergo:0.#}% ergo <color=#8a8a8a>por nível da skill</color>"
            : $"<b>WEAPON MASTERY</b>  <color=#8a8a8a>SMG · LMG · Launcher · Underbarrel:</color> −{rec:0.#}% recoil · +{ergo:0.#}% ergo <color=#8a8a8a>per skill level</color>";
    }

    /// <summary>Reconstrói o painel a partir da classe LOCAL (call-sites 053/059). Idempotente por classe.</summary>
    internal static void Refresh(GameObject panel) => Refresh(panel, ClassIdentities.Local(), showMasteryFooter: true);

    /// <summary>
    ///     057 — reconstrói o painel (header + colunas) a partir de UMA identidade de classe (qualquer player).
    ///     <paramref name="identity"/> null → caminho vanilla (mensagem). Idempotente por classe VIA PanelState.
    ///     <paramref name="showMasteryFooter"/>: só a aba CLASS local (CR-060-01 — o footer é config do client local).
    /// </summary>
    internal static void Refresh(GameObject panel, ClassIdentities.Identity? identity, bool showMasteryFooter = false)
    {
        try
        {
            panel.GetComponent<FadeIn>()?.Restart();   // CR-03-03: re-dispara o fade a cada exibição
            RefreshMasteryFooter(panel, showMasteryFooter);   // 060: ANTES do guard de idempotência (toggle/valores do F12 refletem sempre)
            var headerTmp = panel.transform.Find("Header/HeaderText")?.GetComponent<TextMeshProUGUI>();
            var headerIcon = panel.transform.Find("Header/Icon")?.GetComponent<Image>();
            var perksCol = panel.transform.Find("Columns/PerksCol");
            var drawbacksCol = panel.transform.Find("Columns/DrawbacksCol");
            if (headerTmp == null || perksCol == null || drawbacksCol == null)
            {
                return;
            }

            var font = headerTmp.font;

            // CR-01-03 + PA-01-07 (057): só reconstrói quando a classe DESTE painel muda (estado per-panel —
            // no loading há N painéis; um estático contaminaria os vizinhos).
            var state = panel.GetComponent<PanelState>();
            if (state == null)
            {
                state = panel.AddComponent<PanelState>();
            }

            var cls = identity?.NameEn;
            if ((perksCol.childCount > 0 || drawbacksCol.childCount > 0) && state.LastClass == cls)
            {
                return;
            }

            state.LastClass = cls;

            // header: ícone da classe (brasão) + nome (cor da classe) + subtítulo esmaecido.
            ClassIdentityView.ApplyClassIcon(headerIcon, identity?.IconFile, identity?.NameColor, 40f);

            // idéia 3: marca d'água = brasão da classe, bem apagado.
            var watermark = panel.transform.Find("Watermark")?.GetComponent<Image>();
            if (watermark != null)
            {
                var wmColor = ClassIdentityView.ResolveColor(identity?.NameColor, Color.white);
                var wmSprite = ClassIconCache.GetTinted(identity?.IconFile, wmColor, wmColor);
                if (wmSprite != null)
                {
                    watermark.sprite = wmSprite;
                    watermark.color = new Color(1f, 1f, 1f, 0.05f);
                    watermark.gameObject.SetActive(true);   // CR-03-01: só ativa a marca d'água quando há brasão
                }
            }
            var name = identity?.DisplayName;
            var classHex = string.IsNullOrWhiteSpace(identity?.NameColor) ? "#ffffff" : identity!.NameColor;
            var sub = GameLocale.IsPortuguese ? "Perks e Drawbacks" : "Perks & Drawbacks";
            headerTmp.text = string.IsNullOrEmpty(name)
                ? sub
                : $"<b><color={classHex}>{name!.ToUpperInvariant()}</color></b>   <size=55%><color=#7a7a7a><i>{sub}</i></color></size>";

            // 068: descrição/mérito da classe (do .jsonc). Aparece pra qualquer classe com description; para o
            // Peladão (sem perks) é o conteúdo principal da aba, em vez de "classe sem perks".
            var flavorTmp = panel.transform.Find("FlavorText")?.GetComponent<TextMeshProUGUI>();
            if (flavorTmp != null)
            {
                var desc = identity?.Description;
                var hasDesc = !string.IsNullOrWhiteSpace(desc);
                flavorTmp.gameObject.SetActive(hasDesc);
                if (hasDesc)
                {
                    // <noparse>: description é prosa autorada — evita que um '<'/'>'/'&' vire tag/entidade TMP.
                    flavorTmp.text = $"<noparse>{desc}</noparse>";
                }
            }

            // limpa as duas colunas e reconstrói.
            ClearChildren(perksCol);
            ClearChildren(drawbacksCol);

            var groups = PerksCatalog.GroupsFor(identity?.NameEn);
            if (groups == null || groups.Length == 0)
            {
                // 068: distingue classe do MOD sem perks (Peladão — identidade raiz deliberada) de classe VANILLA
                // real (identity null). Se a classe do mod já tem descrição (mérito na FlavorText), NÃO repete um
                // card "sem perks" (redundante); só mostra a nota funcional quando não há descrição pra cobrir.
                if (identity == null)
                {
                    BuildMessageCard(perksCol, font, GameLocale.IsPortuguese
                        ? "Classe vanilla — sem perks/drawbacks."
                        : "Vanilla class — no perks/drawbacks.");
                }
                else if (string.IsNullOrWhiteSpace(identity.Description))
                {
                    BuildMessageCard(perksCol, font, GameLocale.IsPortuguese
                        ? "Sem perks nem drawbacks — e é essa a graça."
                        : "No perks, no drawbacks — that's the point.");
                }
            }
            else
            {
                // 059 CLASS#3: perks à ESQUERDA, drawbacks à DIREITA. UM card por EFEITO atômico (decisão
                // 2026-07-03); a coluna continua definida por group.IsPerk (grupos homogêneos).
                var perks = groups.Where(g => g.IsPerk).ToArray();
                var draws = groups.Where(g => !g.IsPerk).ToArray();
                if (perks.Length > 0)
                {
                    BuildSectionHeader(perksCol, font, "PERKS");
                    foreach (var g in perks)
                    {
                        foreach (var line in g.Lines)
                        {
                            BuildEffectCard(perksCol, g, line, font);
                        }
                    }
                }

                if (draws.Length > 0)
                {
                    BuildSectionHeader(drawbacksCol, font, "DRAWBACKS");
                    foreach (var g in draws)
                    {
                        foreach (var line in g.Lines)
                        {
                            BuildEffectCard(drawbacksCol, g, line, font);
                        }
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (059) refresh painel falhou: {ex.Message}");
        }
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
    ///     059 CLASS#3 — card de UM efeito atômico (<see cref="PerksCatalog.PerkLine"/>): acento + frame com o
    ///     ícone de buff da tela de Skills (<see cref="PerksCatalog.BuffSprite"/>; fallback = ícone do grupo) +
    ///     nome do GRUPO esmaecido em cima + chip do ValueToken e label do efeito em destaque. Efeito deferido →
    ///     "· em breve". Cor/acento saem de <c>line.IsPerk</c> (a COLUNA continua por <c>group.IsPerk</c>).
    /// </summary>
    private static void BuildEffectCard(Transform parent, PerksCatalog.PerkGroup group, PerksCatalog.PerkLine line,
        TMP_FontAsset? font)
    {
        var accent = line.Pending
            ? new Color(0.80f, 0.62f, 0.28f, 1f)
            : line.IsPerk ? MultiplierFormat.Green : MultiplierFormat.Red;

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        var cardBg = line.Pending
            ? new Color(0.10f, 0.09f, 0.055f, 0.55f)
            : line.IsPerk
                ? new Color(0.07f, 0.10f, 0.08f, 0.55f)
                : new Color(0.11f, 0.075f, 0.075f, 0.55f);
        cardImg.color = cardBg;

        var hover = card.AddComponent<CardHover>();   // idéia 1: realce no hover
        hover.Target = cardImg;
        hover.Normal = cardBg;
        hover.Hover = new Color(cardBg.r + 0.06f, cardBg.g + 0.06f, cardBg.b + 0.06f, Mathf.Min(1f, cardBg.a + 0.22f));
        var hl = card.GetComponent<HorizontalLayoutGroup>();
        hl.padding = new RectOffset(16, 16, 8, 8);
        hl.spacing = 13f;
        hl.childAlignment = TextAnchor.MiddleLeft;   // card compacto de 2 linhas → ícone centralizado
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
        fle.minWidth = 40f;
        fle.preferredWidth = 40f;
        fle.minHeight = 40f;
        fle.preferredHeight = 40f;

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
        // 059 CLASS#3: ícone DO EFEITO (mesmo sprite dos quadradinhos da tela de Skills); fallback = ícone do grupo.
        // == explícito (não ??): UnityEngine.Object sobrecarrega == pra "destroyed fake-null".
        var sprite = PerksCatalog.BuffSprite(line);
        if (sprite == null)
        {
            sprite = PerksCatalog.IconSprite(group);
        }
        if (sprite != null)
        {
            iimg.sprite = sprite;
            iimg.color = Color.white;
        }
        else
        {
            iimg.enabled = false;
        }

        // coluna de texto: [NOME DO GRUPO esmaecido] + [chip + label do efeito em destaque].
        var col = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup));
        col.transform.SetParent(card.transform, false);
        var cvl = col.GetComponent<VerticalLayoutGroup>();
        cvl.spacing = 2f;
        cvl.childControlWidth = true;
        cvl.childControlHeight = true;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        cvl.childAlignment = TextAnchor.MiddleLeft;
        col.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var groupGo = new GameObject("Group", typeof(RectTransform), typeof(TextMeshProUGUI));
        groupGo.transform.SetParent(col.transform, false);
        var gtmp = groupGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            gtmp.font = font;
        }

        gtmp.text = line.Title;   // fix in-game 2026-07-03: título ÚNICO por efeito (não repete o nome do grupo)
        gtmp.fontSize = 12.5f;
        gtmp.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        gtmp.characterSpacing = 3f;
        gtmp.color = new Color(accent.r, accent.g, accent.b, 0.75f);   // esmaecido na cor do acento
        gtmp.raycastTarget = false;
        gtmp.enableWordWrapping = false;
        gtmp.overflowMode = TextOverflowModes.Overflow;

        var lineGo = new GameObject("Line", typeof(RectTransform), typeof(TextMeshProUGUI));
        lineGo.transform.SetParent(col.transform, false);
        var ltmp = lineGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            ltmp.font = font;
        }

        var hex = line.Pending ? "#cc9a3e" : (line.IsPerk ? MultiplierFormat.GreenHex : MultiplierFormat.RedHex);
        var chip = line.ValueToken.Length > 0 ? $"<b><color={hex}>{line.ValueToken}</color></b> " : "";
        var soon = line.Pending
            ? $"  <size=75%><color=#cc9a3e><i>{(GameLocale.IsPortuguese ? "· em breve" : "· soon")}</i></color></size>"
            : "";
        ltmp.text = chip + $"<color=#d8d8d8>{line.Label}</color>" + soon;
        ltmp.fontSize = 16.5f;
        ltmp.color = Color.white;
        ltmp.raycastTarget = false;
        ltmp.enableWordWrapping = true;   // 059-CR: label quebra linha em coluna estreita (evita transbordar o card)
        ltmp.richText = true;
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
        tmp.margin = new Vector4(0f, 0f, 0f, 8f);   // fix in-game 2026-07-03: respiro entre o rótulo e a divisória
        go.GetComponent<LayoutElement>().minHeight = 34f;

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
///     CR-03-03: o Update se auto-desliga (enabled=false) ao terminar; o Refresh chama Restart() em cada
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
