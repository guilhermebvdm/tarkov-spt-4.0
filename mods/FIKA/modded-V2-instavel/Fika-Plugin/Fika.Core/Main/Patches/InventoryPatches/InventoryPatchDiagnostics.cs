// © 2026 Lacyway All Rights Reserved

using System;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Resolve quem é o dono real (ou se não há dono nenhum) do container referenciado por um
/// ItemAddress, comparando contra os netId ainda ativos em CoopHandler.Players. Nasceu como
/// diagnóstico (item de rig "fantasma") durante a investigação do bug de container dropado
/// não sincronizando entre host/convidado — ver
/// mods/FIKA/backlog/018-container-orfao-dropado-sem-refresh/. `ResolveOwner` (programático)
/// agora também alimenta o broadcast/pedido de resync desse item; `DescribeGhostOwner`
/// (string de log) reaproveita a mesma resolução.
/// </summary>
internal static class InventoryPatchDiagnostics
{
    // ref: item 018 do backlog — struct comum em vez de record struct: netstandard2.1 não traz
    // System.Runtime.CompilerServices.IsExternalInit (exigido por propriedades init-only dos
    // records), causando CS0518 nesse target framework.
    public readonly struct OwnerResolution(Item rootItem, bool isOrphaned, string ownerNetId, bool isCorpse = false)
    {
        public Item RootItem { get; } = rootItem;
        public bool IsOrphaned { get; } = isOrphaned;
        public string OwnerNetId { get; } = ownerNetId;
        // ref: 018-06-fix-01 — Corpse (bot/jogador morto) também conta como "órfão" (nunca tem
        // netId ativo em CoopHandler.Players), mas NÃO deve disparar o mecanismo de destruir/
        // recriar do item 018 — corpo tem representação física própria (ragdoll/malha), incompatível
        // com CreateLootPrefab/CreateLootWithRigidbody genéricos. Chamadores devem checar
        // `IsOrphaned && !IsCorpse` antes de disparar broadcast/pedido de resync.
        public bool IsCorpse { get; } = isCorpse;
    }

    public static OwnerResolution ResolveOwner(ItemAddress address)
    {
        var containerItem = address?.Container?.ParentItem;
        var rootItem = containerItem?.Owner?.RootItem;
        if (rootItem == null)
        {
            // Sem árvore resolvível — não é o mesmo caso de "órfão" (que exige uma árvore real,
            // só sem dono ativo). Tratar como "não órfão" evita disparar o resync indevidamente.
            return new(rootItem, false, null);
        }

        var players = Singleton<IFikaNetworkManager>.Instance?.CoopHandler?.Players;
        if (players != null)
        {
            foreach (var kvp in players)
            {
                var candidateRoot = kvp.Value?.InventoryController?.RootItem;
                if (candidateRoot != null && candidateRoot.Id == rootItem.Id)
                {
                    return new(rootItem, false, kvp.Key.ToString());
                }
            }
        }

        var isCorpse = Singleton<GameWorld>.Instantiated
            && Singleton<GameWorld>.Instance.LootList.OfType<Corpse>().Any(c => c.Item?.Id == rootItem.Id);
        return new(rootItem, true, null, isCorpse);
    }

    public static string DescribeGhostOwner(ItemAddress ghostAddress)
    {
        try
        {
            var containerItem = ghostAddress?.Container?.ParentItem;
            if (containerItem == null)
            {
                return "container esperado não tem ParentItem (sem item de rig/contêiner associado)";
            }

            var resolution = ResolveOwner(ghostAddress);
            if (resolution.RootItem == null)
            {
                return $"container esperado (item {containerItem.Id}, template {containerItem.TemplateId}) não tem Owner/RootItem resolvível — item desconectado da árvore de inventário";
            }

            if (!resolution.IsOrphaned)
            {
                var players = Singleton<IFikaNetworkManager>.Instance?.CoopHandler?.Players;
                var nickname = "?";
                if (players != null && int.TryParse(resolution.OwnerNetId, out var netId) && players.TryGetValue(netId, out var owningPlayer))
                {
                    nickname = owningPlayer?.Profile?.Nickname ?? "?";
                }
                return $"netId {resolution.OwnerNetId} ({nickname}) — dono AINDA presente em CoopHandler.Players (root item {resolution.RootItem.Id})";
            }

            return $"root item {resolution.RootItem.Id} (owner type: {resolution.RootItem.Owner?.OwnerType}) NÃO corresponde a nenhum netId ativo em CoopHandler.Players — dono órfão/desconectado";
        }
        catch (Exception ex)
        {
            return $"falha ao resolver dono: {ex}";
        }
    }
}
