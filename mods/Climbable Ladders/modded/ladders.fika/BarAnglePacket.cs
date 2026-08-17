using Fika.Core.Networking.LiteNetLib.Utils;

namespace tarkin.ladders.fika
{
    public struct BarAnglePacket : INetSerializable
    {
        public int PlayerId;
        public float Angle;

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Angle = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Angle);
        }
    }
}
