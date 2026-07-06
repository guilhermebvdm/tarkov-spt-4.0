using System.Text.Json.Serialization;

using SPTarkov.Server.Core.Models.Utils;

namespace MOARServer.Models;

public class AddBotSpawnRequest : IRequestData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
    
    [JsonPropertyName("z")]
    public double Z { get; set; }
    
    [JsonPropertyName("map")]
    public string Map { get; set; } = string.Empty;
}
