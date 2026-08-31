using Fika.Core.Networking.LiteNetLib.Utils;

namespace tarkin.ladders.fika
{
    public struct LadderStatePacket : INetSerializable
    {
        public int NetId;
        public EStateType Type;
        public string LadderId;

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            Type = reader.GetEnum<EStateType>();
            LadderId = reader.GetString();
        }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.PutEnum(Type);
            writer.Put(LadderId);
        }

        public enum EStateType : byte
        {
            Enter,
            Exit
        }
    }
}
