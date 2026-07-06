using System.Text.Json.Serialization;

namespace MOARServer.Models;

public class SpawnPosition
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double Z { get; set; }
}

public class SpawnLocations : Dictionary<string, List<SpawnPosition>>
{
}
