using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     Item 058: um elemento do array servido por <c>GET /customclasses/classes</c> — contrato SP0
///     congelado 2026-07-03 (launcher/Launcher4.0beta/backlog/004-classes-dados-reais §"Contrato SP0").
///     Consumido pelo launcher TRL (tela de seleção de classe, pré-registro) e pelo futuro item 057
///     (identidade de classe per-player em coop).
///     Nota: o JsonUtil do SPT serializa com WhenWritingNull — campos null são OMITIDOS do JSON
///     (o launcher trata campo ausente como null).
/// </summary>
public sealed record ClassListItem
{
    /// <summary>Chave EXATA da edition no ProfileTemplates (language=pt → displayName.pt, ex. "Caçador").
    /// É o valor que o launcher envia verbatim no POST /launcher/profile/register.</summary>
    [JsonPropertyName("editionKey")]
    public required string EditionKey { get; init; }

    [JsonPropertyName("displayName")]
    public required LocalizedPair DisplayName { get; init; }

    [JsonPropertyName("description")]
    public required LocalizedPair Description { get; init; }

    /// <summary>URL relativa ao backend (/CustomClasses-Server/icons/&lt;iconFile&gt;; o mount wwwroot do
    /// IModWebMetadata já serve) — null/omitida quando a classe não tem iconFile.</summary>
    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; init; }

    /// <summary>Cor do nome da classe na UI, hex "#RRGGBB" (null/omitida sem cor).</summary>
    [JsonPropertyName("nameColor")]
    public string? NameColor { get; init; }

    /// <summary>Skill → nível inicial (cru do arquivo de classe; {} sem skills).</summary>
    [JsonPropertyName("skills")]
    public required Dictionary<string, int> Skills { get; init; }

    /// <summary>Skill → fator de XP EM VIGOR (normalizado enum-cased + clamp ≥ 0 pelo registrar; {} sem fatores).</summary>
    [JsonPropertyName("skillMultipliers")]
    public required Dictionary<string, double> SkillMultipliers { get; init; }

    /// <summary>
    ///     Item 029: perks/drawbacks da classe (snapshot nominal de <see cref="PerksCatalogData"/>). OPCIONAL —
    ///     null/omitido (WhenWritingNull) = classe sem perks. Já vem com o token pré-formatado; o launcher só renderiza.
    /// </summary>
    [JsonPropertyName("effects")]
    public IReadOnlyList<ClassEffect>? Effects { get; init; }
}

/// <summary>Item 029: um perk (isPerk=true) ou drawback (isPerk=false) servido em <c>effects</c>.</summary>
public sealed record ClassEffect
{
    /// <summary>true = perk (verde, coluna esquerda) · false = drawback (vermelho, direita).</summary>
    [JsonPropertyName("isPerk")]
    public required bool IsPerk { get; init; }

    /// <summary>true = efeito definido mas ainda não ativo in-game ("em breve", âmbar).</summary>
    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    /// <summary>Título único do efeito (ex.: Sharpshooter / Atirador).</summary>
    [JsonPropertyName("title")]
    public required LocalizedPair Title { get; init; }

    /// <summary>Descrição curta do efeito (ex.: "aim (ADS) time" / "mira (ADS)").</summary>
    [JsonPropertyName("label")]
    public required LocalizedPair Label { get; init; }

    /// <summary>Token pré-formatado: "+30%" / "−15%" / "×0.85" / "✓" / "✗".</summary>
    [JsonPropertyName("valueToken")]
    public required string ValueToken { get; init; }
}

/// <summary>
///     Par en/pt SEMPRE serializado como objeto <c>{ "en": ..., "pt": ... }</c>. NÃO usar
///     <see cref="LocalizedText"/> aqui: o LocalizedTextConverter colapsa <c>Pt==null</c> para string
///     simples (forma legada de arquivo), o que quebraria o shape do contrato SP0.
/// </summary>
public sealed record LocalizedPair
{
    [JsonPropertyName("en")]
    public string? En { get; init; }

    [JsonPropertyName("pt")]
    public string? Pt { get; init; }
}
