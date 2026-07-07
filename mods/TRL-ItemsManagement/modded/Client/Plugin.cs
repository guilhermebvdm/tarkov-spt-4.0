using BepInEx;
using BepInEx.Logging;
using TRLItemsManagement.Client.Patches;

namespace TRLItemsManagement.Client;

/// <summary>
///     Client half: patches the sell-to-trader price display so it reflects the SAME
///     <c>buy-overrides.json</c> config the paired server mod's Harmony backstop applies at
///     <c>TradeHelper.SellItem</c> — exhibited price and credited money always agree.
/// </summary>
[BepInPlugin("trlitemsmanagement.trl.client", "TRL Items Management", "1.0.0")]
[BepInDependency("com.SPT.core", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource? Log;

    private void Awake()
    {
        Log = Logger;
        new GetUserItemPricePatch().Enable();
        Log.LogInfo("[TRLItemsManagement] client loaded (buy-price display patch).");
    }
}
