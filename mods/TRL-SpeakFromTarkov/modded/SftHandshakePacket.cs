using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov
{
    public struct SftHandshakePacket : INetSerializable
    {
        public byte PlayerNetId;
        public string ProfileId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerNetId);
            writer.Put(ProfileId ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            try
            {
                PlayerNetId = reader.GetByte();
                ProfileId = reader.GetString();
            }
            catch
            {
                PlayerNetId = 0;
                ProfileId = string.Empty;
            }
        }
    }
}
