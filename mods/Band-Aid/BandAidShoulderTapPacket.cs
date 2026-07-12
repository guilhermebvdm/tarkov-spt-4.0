using LiteNetLib.Utils;
using Fika.Core.Networking;

namespace Band_Aid
{
    public struct BandAidShoulderTapPacket : INetSerializable
    {
        public string SenderProfileId;
        public string SenderNickname;
        public string TargetProfileId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SenderProfileId);
            writer.Put(SenderNickname);
            writer.Put(TargetProfileId);
        }

        public void Deserialize(NetDataReader reader)
        {
            SenderProfileId = reader.GetString();
            SenderNickname = reader.GetString();
            TargetProfileId = reader.GetString();
        }
    }
}
