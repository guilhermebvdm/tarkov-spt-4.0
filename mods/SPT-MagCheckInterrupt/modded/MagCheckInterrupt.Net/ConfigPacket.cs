using Fika.Core.Networking.LiteNetLib.Utils;

namespace MagCheckInterrupt.Net;

public struct ConfigPacket(string[] config) : INetSerializable
{
    public string[] Config = config;

    public void Deserialize(NetDataReader reader)
    {
        Config = reader.GetStringArray();
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.PutArray(Config);
    }
}
