// © 2026 Lacyway All Rights Reserved

using System;
using Comfort.Common;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// DIAGNÓSTICO TEMPORÁRIO — investigação do bug de itens de rig "fantasma" (item aparenta
/// pertencer a um container de um corpo diferente do que ele realmente está, capturado por
/// MoveOperationDescriptorPatch/SplitOperationDescriptorPatch). Resolve quem é o dono real
/// (ou se não há dono nenhum) do container referenciado pelo endereço "esperado" (stale) que
/// o cliente enviou, comparando contra os netId ainda ativos em CoopHandler.Players.
/// Ver mods/FIKA/backlog (investigação de rig acumulando entre corpos) — remover após
/// identificar a causa raiz.
/// </summary>
internal static class InventoryPatchDiagnostics
{
    public static string DescribeGhostOwner(ItemAddress ghostAddress)
    {
        try
        {
            var containerItem = ghostAddress?.Container?.ParentItem;
            if (containerItem == null)
            {
                return "container esperado não tem ParentItem (sem item de rig/contêiner associado)";
            }

            var owner = containerItem.Owner;
            var rootItem = owner?.RootItem;
            if (rootItem == null)
            {
                return $"container esperado (item {containerItem.Id}, template {containerItem.TemplateId}) não tem Owner/RootItem resolvível — item desconectado da árvore de inventário";
            }

            var players = Singleton<IFikaNetworkManager>.Instance?.CoopHandler?.Players;
            if (players != null)
            {
                foreach (var kvp in players)
                {
                    var candidateRoot = kvp.Value?.InventoryController?.RootItem;
                    if (candidateRoot != null && candidateRoot.Id == rootItem.Id)
                    {
                        return $"netId {kvp.Key} ({kvp.Value.Profile?.Nickname ?? "?"}) — dono AINDA presente em CoopHandler.Players (root item {rootItem.Id})";
                    }
                }
            }

            return $"root item {rootItem.Id} (owner type: {owner.OwnerType}) NÃO corresponde a nenhum netId ativo em CoopHandler.Players — dono órfão/desconectado";
        }
        catch (Exception ex)
        {
            return $"falha ao resolver dono: {ex}";
        }
    }
}
