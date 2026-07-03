using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 010: formatação central do buff/debuff (cor, %, marcador da linha, texto do tooltip).
///     Tudo que é string pt-BR fica aqui — o item 008 (i18n) troca só este arquivo.
/// </summary>
internal static class MultiplierFormat
{
    public const string GreenHex = "#9ad27a";   // buff   (mesmo verde do 005)
    public const string RedHex = "#d27a7a";     // debuff (mesmo vermelho do 005)

    public static readonly Color Green = new(0.604f, 0.824f, 0.478f, 1f);
    public static readonly Color Red = new(0.824f, 0.478f, 0.478f, 1f);

    // PA-01-01/03: cor laranja vanilla da borda de Elite (SkillIcon: new Color32(183,112,0,255)).
    public static readonly Color EliteBorder = new Color32(183, 112, 0, byte.MaxValue);

    public static bool IsActive(float factor) => Mathf.Abs(factor - 1f) > 1e-4f;
    public static int Percent(float factor) => Mathf.RoundToInt((factor - 1f) * 100f);
    public static Color BorderColor(float factor) => factor > 1f ? Green : Red;

    /// <summary>
    ///     Item 059 — token de valor de uma propriedade atômica: Percent "+30%"/"−10%" (sinal do mult, magnitude
    ///     |mult−1|·100), Multiplier "×0.85"/"×5", Flag "" (qualitativa). Cor/seção vêm do <c>PerkLine.IsPerk</c>.
    /// </summary>
    public static string ValueToken(PerksCatalog.PerkLine l) => l.Format switch
    {
        ValueFormat.Percent => (l.Multiplier > 1f ? "+" : "−") + Mathf.RoundToInt(Mathf.Abs(l.Multiplier - 1f) * 100f) + "%",
        ValueFormat.Multiplier => "×" + l.Multiplier.ToString("0.##"),
        _ => "",
    };

    /// <summary>
    ///     Marcador da linha: "▲ +50%" (verde) / "▼ -30%" (vermelho), rich text TMP.
    ///     PA-01-02: setas Unicode (U+25B2/U+25BC). Se a fonte TMP do EFT não tiver os glyphs
    ///     (aparecer "□"), trocar AQUI por só-texto (sem seta) ou ASCII — ponto único de mudança.
    /// </summary>
    public static string Marker(float factor)
    {
        var pct = Percent(factor);
        var up = factor > 1f;
        var hex = up ? GreenHex : RedHex;
        var arrow = up ? "▲" : "▼";
        var sign = pct >= 0 ? "+" : string.Empty;
        return $"<color={hex}>{arrow} {sign}{pct}%</color>";
    }

    /// <summary>
    ///     Frase do tooltip (rawText — preserva as tags): nome da classe em negrito + "+X% buff/debuff"
    ///     colorido. Idioma segue o EFT (item 008); fallback de nome quando ausente.
    /// </summary>
    public static string TooltipText(float factor, string? className)
    {
        var pct = Percent(factor);
        var up = factor > 1f;
        var hex = up ? GreenHex : RedHex;
        var word = up ? "buff" : "debuff";
        var sign = pct >= 0 ? "+" : string.Empty;

        if (GameLocale.IsPortuguese)   // item 008: segue o idioma do EFT
        {
            var amountPt = $"<color={hex}>{sign}{pct}% de {word}</color>";
            var clsPt = string.IsNullOrWhiteSpace(className) ? "sua Classe" : $"Classe <b>{className}</b>";
            return $"Você possui {amountPt} nessa skill devido à {clsPt}";
        }

        var amountEn = $"<color={hex}>{sign}{pct}% {word}</color>";
        var clsEn = string.IsNullOrWhiteSpace(className) ? "your Class" : $"Class <b>{className}</b>";
        return $"You have a {amountEn} on this skill from {clsEn}";
    }

    /// <summary>
    ///     Item 056 — tooltip do marcador de PESO (Pack Mule) na aba Health. Atribui o +X% de limite de carga à classe.
    ///     "piso/floor" porque o Pack Mule é aplicado como piso (não soma com a Strength).
    /// </summary>
    public static string CarryTooltip(float factor, string? className)
    {
        var pct = Percent(factor);
        var sign = pct >= 0 ? "+" : string.Empty;
        var amount = $"<color={GreenHex}>{sign}{pct}%</color>";
        if (GameLocale.IsPortuguese)
        {
            var cls = string.IsNullOrWhiteSpace(className) ? "sua Classe" : $"Classe <b>{className}</b>";
            return $"Limite de carga {amount} pela {cls} (Pack Mule, piso)";
        }

        var clsEn = string.IsNullOrWhiteSpace(className) ? "your Class" : $"Class <b>{className}</b>";
        return $"Carry limit {amount} from {clsEn} (Pack Mule, floor)";
    }
}
