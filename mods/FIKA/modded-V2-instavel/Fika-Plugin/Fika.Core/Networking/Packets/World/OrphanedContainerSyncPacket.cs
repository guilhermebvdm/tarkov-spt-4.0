using EFT;

namespace Fika.Core.Networking.Packets.World;

/// <summary>
/// Dois usos (ref: item 018 do backlog, mods/FIKA/backlog/018-container-orfao-dropado-sem-refresh/):
/// IsRequest=true (peer -> host): "meu container X está divergente, manda o estado real" (sem Data).
/// IsRequest=false (host -> peer(s)): estado atual completo do container, proativo (após uma
/// operação de Move/Split reconciliada com sucesso num container órfão) ou em resposta a um
/// pedido. Reusa PutItemDescriptor/GetItemDescriptor (FikaSerializationExtensions.cs, corrigidos
/// no item 019 pra não deixar o buffer estático sujo em caso de exceção no meio da serialização).
/// </summary>
public struct OrphanedContainerSyncPacket : INetSerializable
{
    public bool IsRequest;
    public MongoID RootItemId;
    public byte[] Data;

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(IsRequest);
        writer.PutMongoID(RootItemId);
        if (!IsRequest)
        {
            writer.PutBytesWithLength(Data);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        IsRequest = reader.GetBool();
        RootItemId = reader.GetMongoID();
        if (!IsRequest)
        {
            // ref: CR-01-02 — TryGetBytesWithLength em vez de GetBytesWithLength: payload
            // truncado/malformado não pode lançar aqui (o try/catch do callback de recepção
            // não protege Deserialize — ver docs/technical/fika-packet-desync-prevention-plan.md).
            Data = reader.TryGetBytesWithLength(out var data) ? data : null;
        }
    }
}
