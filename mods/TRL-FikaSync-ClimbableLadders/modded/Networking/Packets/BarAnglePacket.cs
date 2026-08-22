using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL.FikaSync.ClimbableLadders.Networking.Packets
{
    public struct BarAnglePacket : INetSerializable
    {
        public int NetId;
        public float Angle;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(Angle);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            Angle = reader.GetFloat();
        }
    }
}
