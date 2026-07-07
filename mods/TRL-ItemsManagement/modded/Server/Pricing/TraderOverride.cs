using System.Text.Json.Serialization;

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Per-tpl override: the native <c>Count</c> the game charges/pays and the <c>Currency</c> it is
///     denominated in ("RUB" | "USD" | "EUR" | "GP"). Count is a double because BarterScheme.Count is
///     <c>double?</c>. Currency may be absent/empty, in which case the override applies to any money entry
///     (sell side only — see <see cref="SellPriceApplier"/>). Shared shape between the sell
///     (<c>overrides.json</c>) and buy (<c>buy-overrides.json</c>) configs.
/// </summary>
internal sealed record TraderOverride
{
    // The config JSON keys are lowercase ("count"/"currency"); SPT's JsonUtil binds case-sensitively,
    // so without these attributes Count would deserialize to 0 (free items!) and Currency to null.
    [JsonPropertyName("count")]    public double Count { get; init; }
    [JsonPropertyName("currency")] public string? Currency { get; init; }
}
