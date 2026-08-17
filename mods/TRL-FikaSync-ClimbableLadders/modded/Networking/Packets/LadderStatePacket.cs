using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL.FikaSync.ClimbableLadders.Networking.Packets
{
    public struct LadderStatePacket : INetSerializable
    {
        public enum EStateType : byte
        {
            Enter = 0,
            Exit = 1
        }

        public int NetId;
        public string LadderId;
        public EStateType Type;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(LadderId ?? string.Empty);
            writer.Put((byte)Type);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            LadderId = reader.GetString();
            Type = (EStateType)reader.GetByte();
        }
    }
}
