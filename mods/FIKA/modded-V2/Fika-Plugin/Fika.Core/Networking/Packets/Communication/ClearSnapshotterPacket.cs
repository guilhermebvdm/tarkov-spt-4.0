using UnityEngine;

namespace Fika.Core.Networking.Packets.Communication;

public struct ClearSnapshotterPacket : INetSerializable
{
    public int NetId;
    public Vector3 Position;
    public Vector2 Rotation;

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(NetId);
        writer.PutUnmanaged(Position);
        writer.PutUnmanaged(Rotation);
    }

    public void Deserialize(NetDataReader reader)
    {
        NetId = reader.GetInt();
        Position = reader.GetUnmanaged<Vector3>();
        Rotation = reader.GetUnmanaged<Vector2>();
    }
}
