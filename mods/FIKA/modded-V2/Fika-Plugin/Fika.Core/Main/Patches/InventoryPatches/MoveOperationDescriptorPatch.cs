// © 2026 Lacyway All Rights Reserved

using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Auto-reconciliação reativa para movimentação de itens no Fika Headless/Server.
/// No EFT original, MoveDescriptorClass.ToInventoryOperation rejeita com GClass1538 se o endereço
/// de origem 'From' enviado pelo cliente divergir da localização real 'item.Parent' no servidor.
/// Quando ocorre uma dessincronização transitória de slots (ex.: após descarte/reload de magazine),
/// essa rejeição rígida impedia o jogador de mover o item para sempre.
/// Este patch auto-reconcilia a origem a partir da posição real do item no inventário do jogador,
/// permitindo que o Move execute até o destino solicitado.
/// </summary>
public class MoveOperationDescriptorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MoveDescriptorClass), nameof(MoveDescriptorClass.ToInventoryOperation));
    }

    [PatchPrefix]
    public static bool Prefix(MoveDescriptorClass __instance, IPlayer player, ref GStruct152<BaseInventoryOperationClass> __result)
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
                // Auto-reconciliação de origem: o cliente achava que o item estava em 'fromAddress',
                // mas no servidor o item está em 'itemResult.Value.Parent'. Como a ação é do próprio dono do inventário,
                // permitimos que o Move ocorra a partir da localização real do item no host.
                FikaGlobals.LogWarning($"[MoveOperationDescriptorPatch] Desvio de localização detectado para item {itemResult.Value.Id} (esperado: {fromAddress.Value}, real no host: {itemResult.Value.Parent}). Dono do container esperado: {InventoryPatchDiagnostics.DescribeGhostOwner(fromAddress.Value)}. Auto-reconciliando origem.");
            }

            var moveResult = InteractionsHandlerClass.Move(
                itemResult.Value,
                toAddress.Value,
                player.InventoryController,
                simulate: true);

            if (moveResult.Failed)
            {
                __result = moveResult.Error;
                return false;
            }

            __result = new MoveOperationClass(__instance.OperationId, player.InventoryController, moveResult.Value, null);
            return false;
        }

        return true;
    }
}
