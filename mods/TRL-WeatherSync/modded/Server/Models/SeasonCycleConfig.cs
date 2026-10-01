using System.Text.Json.Serialization;

namespace TrlWeatherSync.SeasonCycle.Models;

public record SeasonCycleEntry
{
    // Um dos 6 nomes de Season.cs (SUMMER/AUTUMN/WINTER/SPRING/AUTUMN_LATE/SPRING_EARLY).
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/Season.cs — nunca "STORM" aqui.
    [JsonPropertyName("season")]
    public required string Season { get; set; }

    [JsonPropertyName("days")]
    public required double Days { get; set; }
}

public record SeasonCycleConfig
{
    // Timestamp Unix (UTC, segundos) fixo — imune a relógio do jogador alterado (corner case da spec funcional).
    [JsonPropertyName("referenceEpochUtc")]
    public required long ReferenceEpochUtc { get; set; }

    // Ordem + duração (dias) de cada sub-fase. Soma = duração do ciclo anual completo.
    [JsonPropertyName("cycleOrder")]
    public required List<SeasonCycleEntry> CycleOrder { get; set; }

    // null = segue o ciclo; senão trava nessa sub-fase (nome de Season). Corner case "modo estação fixa".
    [JsonPropertyName("fixedSeason")]
    public string? FixedSeason { get; set; }

    // Chave externa = nome de Season ("WINTER"); interna = nome de WeatherPreset ("SUNNY"/"RAINY"/"CLOUDY").
    // ref: references/spt-source/Libraries/SPTarkov.Server.Core/Generators/WeatherGenerator.cs:79 (chave = currentSeason.ToString())
    [JsonPropertyName("weatherPresetWeight")]
    public required Dictionary<string, Dictionary<string, double>> WeatherPresetWeight { get; set; }
}
