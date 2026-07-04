using System.Reflection;
using EFT.UI;       // ChatSpecialIcon
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     Item 011: monta/atualiza o "selo" da classe (ícone + nome colorido), idempotente.
///     Recebe dados já resolvidos (sem fetch) — consumido pelos patches do 012/013.
///     Reusa <see cref="ClassIconCache"/> para o sprite. Ícone ausente → mostra só o nome.
/// </summary>
internal static class ClassIdentityView
{
    /// <summary>Resolve uma cor hex "#RRGGBB" (item 011/012); usa o fallback se nula/inválida.</summary>
    public static Color ResolveColor(string? hex, Color fallback)
        => !string.IsNullOrWhiteSpace(hex) && ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;

    // (015 · 06-fix-02) Clareamento do topo do gradiente. Era 0.4 (40% branco) — clareava demais e o nome
    // destoava do glow/EXP (cor SÓLIDA da classe) no menu. Reduzido p/ 0.15: gradiente sutil, o nome
    // fica bem perto da cor base → praticamente a mesma tonalidade do glow, mantendo um leve efeito.
    private const float GradientLighten = 0.15f;

    /// <summary>
    ///     (015 — efeito CANÔNICO de cor): gradiente vertical SUTIL na cor da classe (topo = Lerp(cor,
    ///     branco, <see cref="GradientLighten"/>), base = cor). Usado em TODOS os nomes (menu, OVERALL,
    ///     deploy, confirmation, selo de Skills). O vertex gradient vence o <c>.color</c> que o EFT/Menu-Overhaul aplica.
    /// </summary>
    public static void ApplyGradient(TextMeshProUGUI tmp, Color baseColor)
    {
        var light = Color.Lerp(baseColor, Color.white, GradientLighten);
        // Fix 2026-07-04 ("texto bem mais escuro que o ícone", feedback com prints): o TMP MULTIPLICA o
        // vertex gradient pela cor base (tmp.color) — com AMBOS na cor da classe o render era cor×cor
        // (cinza 0.55 → 0.30). A cor vive SÓ no gradiente; a base fica branca (fallback: branco se o
        // gradient for ignorado). O ícone nunca sofreu disso (cor embutida na textura, vértice branco).
        tmp.color = Color.white;
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(light, light, baseColor, baseColor);   // TL, TR, BL, BR
    }

    /// <summary>(015) Aplica o gradiente da classe a um TMP (resolve a cor do hex). No-op se tmp null.</summary>
    public static void ApplyGradient(TextMeshProUGUI? tmp, string? nameColor, Color fallback)
    {
        if (tmp != null)
        {
            ApplyGradient(tmp, ResolveColor(nameColor, fallback));
        }
    }

    // (06-fix-02) Clareamento do topo do gradiente do ÍCONE. Mais forte que o dos nomes (0.15) porque o
    // ícone é pequeno — um degradê sutil seria imperceptível. O gradiente é EMBUTIDO na textura
    // (ClassIconCache.GetTinted), não via BaseMeshEffect (que falha em Image criada em runtime, ex.: menu).
    private const float IconGradientLighten = 0.35f;

    /// <summary>(06-fix-02) Par (topo clareado, base) do gradiente do ícone para a cor da classe.</summary>
    private static (Color top, Color bottom) IconGradient(Color baseColor)
        => (Color.Lerp(baseColor, Color.white, IconGradientLighten), baseColor);

    /// <summary>(06-fix-02) Remove o gradiente do ícone (célula reciclada / outro jogador) — volta ao vanilla.</summary>
    public static void RevertIconGradient(Image? icon)
    {
        if (icon == null)
        {
            return;
        }

        var grad = icon.GetComponent<ClassIconGradient>();
        if (grad != null)
        {
            grad.enabled = false;
        }

        icon.color = Color.white;
        icon.SetVerticesDirty();
    }

    /// <summary>
    ///     (015) Aplica o ícone da classe a um <see cref="Image"/>: sprite (cache) + tint na cor da classe
    ///     (ícones são silhuetas brancas → cor exata) + **tamanho absoluto** (px, consistente entre telas).
    ///     Fix do <c>EMemberCategory.Default</c>: o <c>ChatSpecialIcon.Show</c> retorna cedo p/ jogador normal
    ///     e não seta o sprite — então garantimos o GameObject ativo e o sprite aqui.
    /// </summary>
    public static void ApplyClassIcon(Image? icon, string? iconFile, string? nameColor, float size)
    {
        if (icon == null)
        {
            return;
        }

        // (06-fix-02) sprite com gradiente EMBUTIDO na textura (silhueta branca × degradê vertical) — robusto
        // onde o BaseMeshEffect falha (ícone do menu). icon.color = white pois a cor já está na textura.
        var (top, bottom) = IconGradient(ResolveColor(nameColor, Color.white));
        var sprite = ClassIconCache.GetTinted(iconFile, top, bottom);
        if (sprite == null)
        {
            return;   // sem ícone → mantém o vanilla
        }

        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.enabled = true;
        if (!icon.gameObject.activeSelf)
        {
            icon.gameObject.SetActive(true);   // fix do return-cedo no Default
        }

        // desabilita um gradiente de mesh antigo (build anterior) que possa ter ficado no mesmo GameObject.
        var oldGrad = icon.GetComponent<ClassIconGradient>();
        if (oldGrad != null)
        {
            oldGrad.enabled = false;
        }

        // tamanho ABSOLUTO consistente entre telas (sizeDelta + LayoutElement; reset da escala relativa).
        icon.rectTransform.sizeDelta = new Vector2(size, size);
        var le = icon.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.preferredWidth = size;
            le.preferredHeight = size;
        }

        icon.transform.localScale = Vector3.one;
    }

    // (015) acesso cacheado ao Image privado de um ChatSpecialIcon (compartilhado pelos patches).
    private static readonly FieldInfo? ChatIconField =
        typeof(ChatSpecialIcon).GetField("_icon", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>(015) Overload p/ ChatSpecialIcon — resolve o Image privado (_icon) e aplica o ícone da classe.</summary>
    public static void ApplyClassIcon(ChatSpecialIcon? special, string? iconFile, string? nameColor, float size)
    {
        if (special != null && ChatIconField?.GetValue(special) is Image img)
        {
            ApplyClassIcon(img, iconFile, nameColor, size);
        }
    }

    // (006-fix) limites de segurança do tamanho do ícone (px) — evita ícone absurdo se o fontSize vier estranho.
    private const float MinIconPx = 14f;
    private const float MaxIconPx = 110f;

    /// <summary>
    ///     (006-fix calibragem) Tamanho do ícone (px) PROPORCIONAL à fonte do NOME daquela tela:
    ///     <c>iconPx = nameFontSize × ClassIconRatio</c> (com clamp). Como cada tela tem um fontSize
    ///     diferente, usar o fontSize de cada uma mantém a proporção ícone:fonte idêntica em todas
    ///     (menu, OVERALL, deploy, confirmation) — em vez de um px absoluto que variava visualmente.
    /// </summary>
    public static float IconSizeFor(TextMeshProUGUI? nameTmp)
    {
        var ratio = Plugin.ClassIconRatio?.Value ?? 1.35f;
        var fontSize = nameTmp != null && nameTmp.fontSize > 0f ? nameTmp.fontSize : 28f;
        return Mathf.Clamp(fontSize * ratio, MinIconPx, MaxIconPx);
    }

    /// <summary>(015) Texto do tooltip "This player is &lt;classe&gt;" (i18n EN/PT) com o nome da classe colorido.</summary>
    public static string BuildTooltip(string className, string? nameColor, Color fallback)
    {
        var hex = "#" + ColorUtility.ToHtmlStringRGB(ResolveColor(nameColor, fallback));
        var prefix = GameLocale.IsPortuguese ? "Este jogador é " : "This player is ";   // item 008: segue o idioma do EFT
        return $"{prefix}<color={hex}>{className}</color>";
    }

    /// <summary>Cria (1x) ou atualiza o selo como filho de <paramref name="parent"/>. Retorna o GameObject do container.</summary>
    public static GameObject BuildOrRefresh(Transform parent, string goName, string className,
        string? iconFile, Color color, TMP_FontAsset? font, float iconSize = 48f, float fontSize = 28f)
    {
        var existing = parent.Find(goName);
        // PA-01-03: se o container existir mas sem os filhos esperados, recria do zero (defensivo).
        var go = existing != null && existing.Find("Icon") != null && existing.Find("Label") != null
            ? existing.gameObject
            : CreateContainer(parent, goName, iconSize);

        var img = go.transform.Find("Icon").GetComponent<Image>();
        var (top, bottom) = IconGradient(color);
        var sprite = ClassIconCache.GetTinted(iconFile, top, bottom);   // (06-fix-02) gradiente embutido na textura
        img.gameObject.SetActive(sprite != null);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;   // não distorce PNG não-quadrado
            img.color = Color.white;     // a cor/gradiente está na textura
        }

        var tmp = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.text = className.ToUpperInvariant();   // CAPSLOCK (consistente com os nomes nas outras telas)
        tmp.enableVertexGradient = false;
        tmp.color = color;   // cor sólida da classe (mesma cor/efeito das outras telas — item 015)
        tmp.fontSize = fontSize;
        tmp.enableWordWrapping = false;
        return go;
    }

    private static GameObject CreateContainer(Transform parent, string goName, float iconSize)
    {
        // Se havia um container corrompido (sem filhos), remove antes de recriar.
        var stale = parent.Find(goName);
        if (stale != null)
        {
            Object.Destroy(stale.gameObject);
        }

        var go = new GameObject(goName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);

        var hl = go.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8f;
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        go.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(go.transform, false);
        var le = icon.AddComponent<LayoutElement>();
        le.preferredWidth = iconSize;    // PA-01-02: usa o iconSize parametrizado
        le.preferredHeight = iconSize;
        icon.GetComponent<Image>().raycastTarget = false;

        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(go.transform, false);
        label.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        return go;
    }
}
