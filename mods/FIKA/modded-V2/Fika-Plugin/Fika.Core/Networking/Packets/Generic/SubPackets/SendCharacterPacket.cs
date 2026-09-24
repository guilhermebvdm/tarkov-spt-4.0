using System;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking.Pooling;

namespace Fika.Core.Networking.Packets.Generic.SubPackets;

public sealed class SendCharacterPacket : IPoolSubPacket
{
    private SendCharacterPacket() { }

    public static SendCharacterPacket CreateInstance()
    {
        return new();
    }

    public static SendCharacterPacket FromValue(PlayerInfoPacket playerInfoPacket, bool isAlive, bool isAi, Vector3 position, int netId)
    {
        var packet = GenericSubPacketPoolManager.Instance.GetPacket<SendCharacterPacket>(EGenericSubPacketType.SendCharacter);
        packet.PlayerInfoPacket = playerInfoPacket;
        packet.IsAlive = isAlive;
        packet.IsAI = isAi;
        packet.Position = position;
        packet.NetId = netId;
        return packet;
    }

    public PlayerInfoPacket PlayerInfoPacket;
    public bool IsAlive;
    public bool IsAI;
    public Vector3 Position;
    public int NetId;

    public void Execute(FikaPlayer player = null)
    {
        // DIAGNOSTIC (temporary — investigação do bug de itens de TacticalVest
        // acumulando entre corpos distintos ao invadir raid em andamento). Loga, no
        // instante em que o perfil de um corpo (bot morto) chega pela rede — ANTES de
        // qualquer renderização de UI — quantos itens existem no slot TacticalVest
        // dele e os IDs de cada um. Se o mesmo item ID aparecer em dois corpos
        // diferentes, ou a contagem for maior que o esperado, prova que a duplicação
        // já vem nos DADOS recebidos, não é só tela.
        if (!IsAlive)
        {
            try
            {
                var vestItems = PlayerInfoPacket.Profile?.Inventory?
                    .GetItemsInSlots([EquipmentSlot.TacticalVest])
                    .ToList();
                var ids = vestItems != null ? string.Join(", ", vestItems.Select(i => $"{i.TemplateId}:{i.Id}")) : "(sem inventário)";
                FikaGlobals.LogWarning($"[DIAG-CORPSE-VEST] Corpo recebido: {PlayerInfoPacket.Profile?.Nickname} ({PlayerInfoPacket.Profile?.ProfileId}) netId={NetId} | itens no TacticalVest ({vestItems?.Count ?? 0}): {ids}");
            }
            catch (Exception ex)
            {
                FikaGlobals.LogError($"[DIAG-CORPSE-VEST] Falhou ao inspecionar TacticalVest: {ex}");
            }
        }

        var handler = Singleton<IFikaNetworkManager>.Instance.CoopHandler;
        if (handler != null)
        {
            handler.QueueProfile(PlayerInfoPacket.Profile, PlayerInfoPacket.HealthByteArray, Position, NetId, IsAlive, IsAI,
                PlayerInfoPacket.ControllerId, PlayerInfoPacket.FirstOperationId, PlayerInfoPacket.IsZombie,
                PlayerInfoPacket.ItemId, PlayerInfoPacket.ControllerType);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerInfoPacket = reader.GetPlayerInfoPacket();
        IsAlive = reader.GetBool();
        IsAI = reader.GetBool();
        Position = reader.GetUnmanaged<Vector3>();
        NetId = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.PutPlayerInfoPacket(PlayerInfoPacket);
        writer.Put(IsAlive);
        writer.Put(IsAI);
        writer.PutUnmanaged(Position);
        writer.Put(NetId);
    }

    public void Dispose()
    {
        PlayerInfoPacket = default;
        IsAlive = false;
        IsAI = false;
        Position = default;
        NetId = 0;
    }
}

public struct PlayerInfoPacket
{
    public Profile Profile;
    public MongoID ControllerId;
    public MongoID? ItemId;

    public byte[] HealthByteArray;

    public ushort FirstOperationId;
    public EHandsControllerType ControllerType;

    public bool IsStationary;
    public bool IsZombie;
}
