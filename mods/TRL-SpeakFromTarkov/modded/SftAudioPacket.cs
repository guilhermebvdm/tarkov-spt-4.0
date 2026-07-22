using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov
{
    public struct SftAudioPacket : INetSerializable
    {
        public string ProfileId;
        public byte Channel;
        public byte[] AudioData;
        public float VoiceLevel;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId ?? string.Empty);
            writer.Put(Channel);
            writer.PutBytesWithLength(AudioData);
            writer.Put(VoiceLevel);
        }

        public void Deserialize(NetDataReader reader)
        {
            try
            {
                ProfileId = reader.GetString();
                Channel = reader.GetByte();
                AudioData = reader.GetBytesWithLength();
                VoiceLevel = reader.GetFloat();
            }
            catch
            {
                ProfileId = string.Empty;
                Channel = 0;
                AudioData = null;
                VoiceLevel = 0f;
            }
        }
    }
}
