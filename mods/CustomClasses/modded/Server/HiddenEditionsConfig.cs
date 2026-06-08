using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>Config do item 009: edições (por nome/Key) a ocultar na criação de perfil do launcher.</summary>
public sealed record HiddenEditionsConfig
{
    [JsonPropertyName("hide")]
    public List<string>? Hide { get; init; }
}
