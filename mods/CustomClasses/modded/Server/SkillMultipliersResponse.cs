using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     Item 010/011: payload da rota /customclasses/skill-multipliers — identidade da classe + fatores de XP.
///     O client usa <c>className</c> no tooltip da UI e <c>iconFile</c>/<c>nameColor</c> no "selo" (item 011/012).
/// </summary>
public sealed record SkillMultipliersResponse
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }   // legado: nome = edition key (PT). Mantido p/ fallback do client.

    // item 008 (i18n): nome localizado en/pt — o client resolve pelo idioma do EFT (po → pt, senão en).
    [JsonPropertyName("classNameEn")]
    public string? ClassNameEn { get; init; }

    [JsonPropertyName("classNamePt")]
    public string? ClassNamePt { get; init; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; init; }   // item 015: nickname do perfil local (p/ casar o ChatSpecialIcon)

    [JsonPropertyName("iconFile")]
    public string? IconFile { get; init; }   // item 011: nome do PNG do ícone da classe

    [JsonPropertyName("nameColor")]
    public string? NameColor { get; init; }   // item 011: cor do nome da classe (hex #RRGGBB)

    [JsonPropertyName("multipliers")]
    public Dictionary<string, double>? Multipliers { get; init; }
}
