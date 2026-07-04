using BepInEx;
using BepInEx.Logging;
using TRLTraderPrices.Client.Patches;

namespace TRLTraderPrices.Client;

/// <summary>
///     B-3 Rota B (client half): patches the sell-to-trader price display so it reflects the SAME
///     <c>buy-overrides.json</c> config the paired server mod's Harmony backstop applies at
///     <c>TradeHelper.SellItem</c> — exhibited price and credited money always agree.
/// </summary>
[BepInPlugin("trltraderprices.trl.client", "TRLTraderPrices", "1.0.0")]
[BepInDependency("com.SPT.core", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource? Log;

    private void Awake()
    {
        Log = Logger;
        new TraderBuyPricePatch().Enable();
        Log.LogInfo("[TRLTraderPrices] client loaded (buy-price display patch).");
    }
}
