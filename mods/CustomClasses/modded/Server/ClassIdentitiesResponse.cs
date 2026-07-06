using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     Item 057: payload da rota /customclasses/class-identities — identidade de classe de TODOS os perfis do
///     server, chaveada por nickname. O client (ClassIdentities) usa o mapa pra resolver a classe de cada
///     player na tela de loading do FIKA (popover + tint da linha). Molde: SkillMultipliersResponse.
/// </summary>
public sealed record ClassIdentitiesResponse
{
    [JsonPropertyName("players")]
    public List<PlayerClassIdentity> Players { get; init; } = [];
}

/// <summary>Um perfil com classe do mod: nickname do PMC + identidade visual da classe (nome en/pt, ícone, cor).</summary>
public sealed record PlayerClassIdentity
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; init; }

    [JsonPropertyName("classNameEn")]
    public string? ClassNameEn { get; init; }   // chave EN estável — client resolve PerksCatalog.GroupsFor por ela

    [JsonPropertyName("classNamePt")]
    public string? ClassNamePt { get; init; }

    [JsonPropertyName("iconFile")]
    public string? IconFile { get; init; }   // PNG local do client (BepInEx/plugins/CustomClasses/icons/)

    [JsonPropertyName("nameColor")]
    public string? NameColor { get; init; }   // hex #RRGGBB
}
