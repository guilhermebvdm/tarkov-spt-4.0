// © 2026 Lacyway All Rights Reserved

using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
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

            var hadDivergence = !fromAddress.Value.Equals(itemResult.Value.Parent);
            if (hadDivergence)
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

            // ref: CR-01-01 — broadcast só depois do Move confirmado bem-sucedido (antes disso
            // seria transmitido o estado PRÉ-Move, ou até um estado que nunca chegou a existir
            // se moveResult tivesse falhado). Só transmite quando a divergência foi detectada de
            // verdade (hadDivergence) na ORIGEM, nunca em toda operação bem-sucedida — exclui
            // naturalmente looting de rotina (corpo de bot etc.) sem precisar diferenciar tipo de
            // dono (ver review 03, PA-03-01).
            if (FikaBackendUtils.IsServer)
            {
                InventoryPatchDiagnostics.OwnerResolution? fromResolution = null;
                if (hadDivergence)
                {
                    fromResolution = InventoryPatchDiagnostics.ResolveOwner(fromAddress.Value);
                    // ref: 018-06-fix-01 — corpo de bot/jogador também conta como órfão, mas não
                    // deve passar pelo destruir/recriar (representação física incompatível).
                    if (fromResolution.Value.IsOrphaned && !fromResolution.Value.IsCorpse)
                    {
                        // ref: CR-01-03 — exclui quem originou a operação (já tem o estado certo
                        // localmente); fallback pra broadcastar pra todos se player não for FikaPlayer.
                        var excludePeer = (player is FikaPlayer fikaPlayer) ? Singleton<FikaServer>.Instance.GetPeerById(fikaPlayer.NetId) : null;
                        Singleton<FikaServer>.Instance.BroadcastOrphanedContainerSync(fromResolution.Value.RootItem, excludePeer);
                    }
                }

                // ref: CR-02-01 — o item também pode ter sido inserido num container órfão
                // (destino), não só removido de um (origem, acima). Checagem independente de
                // hadDivergence: inserir num container órfão é relevante mesmo quando a origem do
                // item não divergiu nada. Evita broadcast duplicado quando origem e destino são o
                // mesmo container órfão (reorganizar itens dentro do mesmo colete dropado).
                var toResolution = InventoryPatchDiagnostics.ResolveOwner(toAddress.Value);
                if (toResolution.IsOrphaned && !toResolution.IsCorpse && (fromResolution == null || !fromResolution.Value.IsOrphaned || fromResolution.Value.RootItem.Id != toResolution.RootItem.Id))
                {
                    var excludePeer = (player is FikaPlayer fikaPlayer) ? Singleton<FikaServer>.Instance.GetPeerById(fikaPlayer.NetId) : null;
                    Singleton<FikaServer>.Instance.BroadcastOrphanedContainerSync(toResolution.RootItem, excludePeer);
                }
            }

            __result = new MoveOperationClass(__instance.OperationId, player.InventoryController, moveResult.Value, null);
            return false;
        }

        return true;
    }
}
