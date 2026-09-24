using System;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;
using Fika.Core.Main.ObservedClasses.HandsControllers;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking.Pooling;

namespace Fika.Core.Networking.Packets.FirearmController.SubPackets;

public sealed class ReloadMagPacket : IPoolSubPacket
{
    private ReloadMagPacket()
    {

    }

    public static ReloadMagPacket FromValue(MongoID magId, ItemAddress gridItemAddress)
    {
        var packet = FirearmSubPacketPoolManager.Instance.GetPacket<ReloadMagPacket>(EFirearmSubPacketType.ReloadMag);
        packet.MagId = magId;
        packet.GridItemAddress = gridItemAddress;
        return packet;
    }

    public static ReloadMagPacket CreateInstance()
    {
        return new();
    }

    public MongoID MagId;
    public ItemAddress GridItemAddress;
    public GClass1950 Descriptor;

    public void Execute(FikaPlayer player)
    {
        if (player.HandsController is ObservedFirearmController controller)
        {
            MagazineItemClass magazine = null;
            try
            {
                var result = player.FindItemById(MagId);
                if (!result.Succeeded)
                {
                    FikaGlobals.LogError(result.Error.ToString());
                    return;
                }
                if (result.Value is MagazineItemClass magazineClass)
                {
                    magazine = magazineClass;
                }
                else
                {
                    FikaGlobals.LogError($"ReloadMagPacket: Item was not MagazineClass, it was {result.Value.GetType()}");
                }
            }
            catch (Exception ex)
            {
                FikaGlobals.LogError(ex);
                FikaGlobals.LogError($"ReloadMagPacket: There is no item {MagId} in profile {player.ProfileId}");
                throw;
            }
            ItemAddress gridItemAddress = null;
            if (Descriptor != null)
            {
                try
                {
                    gridItemAddress = player.InventoryController.ToItemAddress(Descriptor);
                }
                catch (GException4 exception2)
                {
                    FikaGlobals.LogError(exception2);
                }
            }
            if (magazine != null)
            {
                controller.FastForwardCurrentState();

                // Auto-Cura de Slot no Headless: se a arma já tem um carregador sendo ejetado e foi fornecido um gridItemAddress
                if (gridItemAddress != null && controller.Item?.GetCurrentMagazine() is MagazineItemClass currentMagazine && player.InventoryController != null)
                {
                    var checkMoveResult = InteractionsHandlerClass.Move(currentMagazine, gridItemAddress, player.InventoryController, true);
                    if (checkMoveResult.Failed)
                    {
                        // O slot solicitado pelo cliente está ocupado/conflitante no Headless (desync de inventário).
                        // Tentamos encontrar qualquer outro slot livre no colete/bolsos para preservar o carregador no equipamento do jogador.
                        var findPlaceResult = InteractionsHandlerClass.QuickFindAppropriatePlace(
                            currentMagazine,
                            player.InventoryController,
                            player.InventoryController.Inventory.Equipment.ToEnumerable(),
                            InteractionsHandlerClass.EMoveItemOrder.UnloadAmmo,
                            true);

                        if (findPlaceResult.Succeeded && findPlaceResult.Value is GInterface428 moveOperation)
                        {
                            FikaGlobals.LogWarning($"[ReloadMagPacket] Slot solicitado ({gridItemAddress}) está indisponível/ocupado no Headless. Auto-Cura redirecionou para slot alternativo: {moveOperation.To}");
                            gridItemAddress = moveOperation.To;
                        }
                        else
                        {
                            FikaGlobals.LogWarning($"[ReloadMagPacket] Nenhum slot disponível no equipamento para descarte do carregador no Headless. Usando fallback de descarte nativo (drop no chão) para evitar cancelamento e congelamento da arma.");
                            gridItemAddress = null;
                        }
                    }
                }

                controller.ReloadMag(magazine, gridItemAddress, null);
            }
            else
            {
                FikaGlobals.LogError($"ReloadMagPacket: final variables were null! Mag: {magazine}, Address: {gridItemAddress}");
            }
        }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.PutMongoID(MagId);
        var exists = GridItemAddress != null;
        writer.Put(exists);
        if (exists)
        {
            writer.PutPolymorph(GridItemAddress.ToDescriptor());
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        MagId = reader.GetMongoID();
        var exists = reader.GetBool();
        if (exists)
        {
            Descriptor = reader.GetPolymorph<GClass1950>();
        }
    }

    public void Dispose()
    {
        MagId = default;
        GridItemAddress = null;
        Descriptor = null;
    }
}
