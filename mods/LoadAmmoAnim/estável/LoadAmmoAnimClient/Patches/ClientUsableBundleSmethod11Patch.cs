using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPT.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ClientUsableItemController.smethod_11, the static async factory the
    // engine uses when building a controller from an item id during a hand swap.
    // vanilla only handles PortableRangeFinderItemClass; for our type the cast
    // returns null and the engine ends up with a controller that has no Item.
    // we route to smethod_7 with our type as the generic arg.
    internal sealed class ClientUsableBundleSmethod11Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ClientUsableItemController).GetMethod(
                "smethod_11",
                BindingFlags.Public | BindingFlags.Static);

        [PatchPrefix]
        private static bool Prefix(
            ref Task<ClientUsableItemController> __result,
            ClientPlayer player,
            string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return true;

            var item = player.InventoryController.FindItem<LoadAmmoBundleItem>(itemId);
            if (item != null)
            {
                __result = Player.UsableItemController.smethod_7<ClientUsableItemController>(player, item);
                return false;
            }
            return true;
        }
    }
}
