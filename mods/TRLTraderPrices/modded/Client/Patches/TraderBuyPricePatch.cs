using System.Reflection;
using EFT;                       // MongoID
using EFT.InventoryLogic;        // Item
using HarmonyLib;                // AccessTools
using SPT.Reflection.Patching;   // ModulePatch, PatchPostfix

namespace TRLTraderPrices.Client.Patches;

/// <summary>
///     B-3 Rota B — client display patch. Postfix on <c>TraderClass.GetUserItemPrice(Item)</c>
///     (Assembly-CSharp.dll, confirmed via ilspycmd on the live client DLL — returns
///     <c>TraderClass.GStruct300?</c>, the exact struct <c>GetBarterPricePatch</c> (Skills-Extended,
///     <c>SilentOps/Patches/GetBarterPricePatch.cs</c>) rewrites for its sibling method
///     <c>GetBarterPrice</c>). This is the ONLY place the sell-to-trader screen gets its displayed price
///     from — vanilla computes it entirely client-side (handbook × buy_price_coef × condition); the
///     server never sees a price until the player confirms the sale. The server-side backstop
///     (<c>modded/Server/TraderBuyPricePatch.cs</c>, Prefix on <c>TradeHelper.SellItem</c>) reads the
///     SAME <c>buy-overrides.json</c> config via <see cref="BuyPriceOverrides"/>, so what this patch
///     shows and what the player actually receives always match.
/// </summary>
public class TraderBuyPricePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TraderClass), nameof(TraderClass.GetUserItemPrice));
    }

    [PatchPostfix]
    private static void Postfix(TraderClass __instance, Item item, ref TraderClass.GStruct300? __result)
    {
        if (__instance == null || item == null)
        {
            return;
        }

        string tpl = item.TemplateId; // MongoID -> string, implicit conversion (EFT/MongoID.cs)
        if (BuyPriceOverrides.TryGetPrice(__instance.Id, tpl, out var currencyTplId, out var amount))
        {
            __result = new TraderClass.GStruct300(new MongoID(currencyTplId), amount);
        }
    }
}
