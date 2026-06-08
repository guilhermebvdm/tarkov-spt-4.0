using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     Item 010: payload da rota /customclasses/skill-multipliers — fatores de XP + nome da classe.
///     O client usa <c>className</c> no tooltip da UI ("…devido à Classe &lt;Nome&gt;").
///     Substitui o payload do 005 (que era só o <c>Dictionary</c> na raiz).
/// </summary>
public sealed record SkillMultipliersResponse
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }

    [JsonPropertyName("multipliers")]
    public Dictionary<string, double>? Multipliers { get; init; }
}
