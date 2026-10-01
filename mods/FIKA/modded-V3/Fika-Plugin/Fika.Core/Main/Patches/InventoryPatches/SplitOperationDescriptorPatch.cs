// © 2026 Lacyway All Rights Reserved

using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Corrige o desync de CloneId ao dividir ou descarregar munições/itens no Fika Headless/Server.
/// No EFT original, SplitDescriptorClass.ToInventoryOperation só instancia Class1371(CloneId)
/// se idGenerator is GClass3386 (o ObservedPlayerInventoryController vanilla do Live).
/// Como o Fika utiliza ObservedInventoryController (que herda de Player.PlayerInventoryController
/// e implementa Interface18), essa verificação falhava e o servidor gerava um novo ID aleatório,
/// fazendo com que operações subsequentes (como TransferOperationClass na segunda bala) falhassem
/// com "Cannot transfer to different templates" ou "Cloned item ID desync".
/// </summary>
public class SplitOperationDescriptorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SplitDescriptorClass), nameof(SplitDescriptorClass.ToInventoryOperation));
    }

    [PatchPrefix]
    public static bool Prefix(SplitDescriptorClass __instance, IPlayer player, ref GStruct152<BaseInventoryOperationClass> __result)
    {
        if (player.InventoryController is Interface18)
        {
            var fromAddress = Player.ToItemAddress(__instance.From);
            if (fromAddress.Failed)
            {
                __result = fromAddress.Error;
                return false;
            }

            var toAddress = Player.ToItemAddress(__instance.To);
            if (toAddress.Failed)
            {
                __result = toAddress.Error;
                return false;
            }

            var itemResult = player.FindItemById(__instance.ItemId);
            if (itemResult.Failed)
            {
                __result = itemResult.Error;
                return false;
            }

            if (!fromAddress.Value.Equals(itemResult.Value.Parent))
            {
                __result = new Player.GClass1538(itemResult.Value, fromAddress.Value);
                return false;
            }

            IIdGenerator idGenerator = new SplitDescriptorClass.Class1371(__instance.CloneId);
            var splitResult = InteractionsHandlerClass.SplitExact(
                itemResult.Value,
                __instance.Count,
                toAddress.Value,
                player.InventoryController,
                idGenerator,
                simulate: true);

            if (splitResult.Failed)
            {
                __result = splitResult.Error;
                return false;
            }

            if (splitResult.Value.ResultItem.Id != __instance.CloneId)
            {
                __result = new GClass1522($"Cloned item ID desync. Expected ID: {__instance.CloneId}, real ID: {splitResult.Value.ResultItem.Id}");
                return false;
            }

            __result = new SplitOperationClass(__instance.OperationId, player.InventoryController, splitResult.Value);
            return false;
        }

        return true;
    }
}
