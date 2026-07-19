using System.Text.Json.Serialization;

namespace TRLItemsManagement.Pricing;

/// <summary>
///     B-6: per-tpl trader stock override. Two independent knobs, either may be absent (edit one without
///     the other):
///     <list type="bullet">
///     <item><c>Stock</c> = <c>Item.Upd.StackObjectsCount</c> — total lifetime stock at that trader. In
///     SPT 4.0 this is a HARD ceiling that erodes with purchases and does NOT restock per refresh cycle
///     (the 3.x pristine-snapshot store was dropped — see <see cref="StockApplier"/>).</item>
///     <item><c>BuyLimit</c> = <c>Item.Upd.BuyRestrictionMax</c> — the most a player may buy per refresh
///     cycle, which DOES reset each cycle.</item>
///     </list>
///     Lowercase JSON keys (SPT's JsonUtil binds case-sensitively — the same reason
///     <see cref="TraderOverride"/> needs the attributes).
/// </summary>
internal sealed record StockOverride
{
    [JsonPropertyName("stock")]    public double? Stock { get; init; }
    [JsonPropertyName("buyLimit")] public int? BuyLimit { get; init; }

    /// <summary>
    ///     When true, the trader stops selling this tpl entirely — <see cref="StockApplier"/> removes every
    ///     root offer of it (plus children, barter & loyalty entries) from the live assort. Takes precedence
    ///     over <see cref="Stock"/>/<see cref="BuyLimit"/> (a removed offer has nothing to cap).
    /// </summary>
    [JsonPropertyName("disabled")] public bool? Disabled { get; init; }
}
