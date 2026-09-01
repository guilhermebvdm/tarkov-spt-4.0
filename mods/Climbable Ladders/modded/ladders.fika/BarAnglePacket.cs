using Fika.Core.Networking.LiteNetLib.Utils;

namespace tarkin.ladders.fika
{
    public struct BarAnglePacket : INetSerializable
    {
        public int NetId;
        public float Angle;

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            Angle = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(Angle);
        }
    }
}
