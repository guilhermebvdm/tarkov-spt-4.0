using Fika.Core.Networking.LiteNetLib.Utils;

namespace tarkin.ladders.fika
{
    public struct LadderStatePacket : INetSerializable
    {
        public int PlayerId;
        public EStateType Type;
        public string LadderId;

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            Type = reader.GetEnum<EStateType>();
            LadderId = reader.GetString();
        }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
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
