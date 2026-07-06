using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace MOARServer.Models;

public class SetPresetRequest : IRequestData
{
    [JsonPropertyName("Preset")]
    public string Preset { get; set; } = string.Empty;
}
