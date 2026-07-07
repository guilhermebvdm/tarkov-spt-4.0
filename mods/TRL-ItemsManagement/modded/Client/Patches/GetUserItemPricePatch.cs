using System;
using System.Reflection;
using EFT.InventoryLogic;        // Item
using HarmonyLib;                // AccessTools
using SPT.Reflection.Patching;   // ModulePatch, PatchPostfix

namespace TRLItemsManagement.Client.Patches;

/// <summary>
///     Client display patch. Postfix on <c>TraderClass.GetUserItemPrice(Item)</c> (Assembly-CSharp.dll,
///     confirmed via ilspycmd on the live client DLL — returns <c>TraderClass.GStruct300?</c>). This is
///     the ONLY place the sell-to-trader screen gets its displayed price from — vanilla computes it
///     entirely client-side (handbook × buy_price_coef × condition); the server never sees a price until
///     the player confirms the sale. The server-side backstop
///     (<c>modded/Server/Pricing/SellItemPatch.cs</c>, Prefix on <c>TradeHelper.SellItem</c>) reads the
///     SAME buy-overrides via <see cref="BuyPriceOverrides"/>, so what this patch shows and what the
///     player actually receives always match.
///     <para>
///     Renamed from <c>TraderBuyPricePatch.cs</c> (TRLTraderPrices) — the server half's Harmony patch
///     (<c>Server/Pricing/SellItemPatch.cs</c>) was also renamed, for the same reason: two files with the
///     identical name in different halves of the same mod invited confusion during the merge.
///     </para>
/// </summary>
public class GetUserItemPricePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TraderClass), nameof(TraderClass.GetUserItemPrice));
    }

    [PatchPostfix]
    private static void Postfix(TraderClass __instance, Item item, ref TraderClass.GStruct300? __result)
    {
        // Only apply an override if its currency matches the TRADER'S ACTUAL currency — mirrors the
        // server backstop's own currency-match guard. The vanilla __result (Postfix runs AFTER the
        // original call) already carries that real currency as CurrencyId, computed by the game from the
        // trader's own settings — comparing against it (instead of trusting whatever currency string sits
        // in the config) means a mismatched/manually-edited config entry can never make the display lie
        // about the currency.
        if (__instance == null || item == null || __result is not { } vanilla || vanilla.CurrencyId is not { } vanillaCurrency)
        {
            return;
        }

        string tpl = item.TemplateId; // MongoID -> string, implicit conversion (EFT/MongoID.cs)
        if (!BuyPriceOverrides.TryGetPrice(__instance.Id, tpl, out var currencyTplId, out var amount))
        {
            return;
        }

        if (!string.Equals(vanillaCurrency, currencyTplId, StringComparison.OrdinalIgnoreCase))
        {
            return; // override's currency doesn't match this trader's real currency -> leave vanilla
        }

        __result = new TraderClass.GStruct300(vanillaCurrency, amount);
    }
}
