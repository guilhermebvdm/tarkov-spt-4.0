using Fika.Core.Networking.LiteNetLib.Utils;

namespace MagCheckInterrupt.Net;

public struct ReloadCalledPacket(int netId) : INetSerializable
{
    public int NetId = netId;

    public void Deserialize(NetDataReader reader)
    {
        NetId = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(NetId);
    }
}
