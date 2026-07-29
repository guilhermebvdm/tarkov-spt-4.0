using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov
{
    public struct SftAudioPacket : INetSerializable
    {
        public const uint MAGIC_HEADER = 0x56544653; // ASCII "SFTV" em little-endian (0x53, 0x46, 0x54, 0x56)

        public uint Magic;
        public byte PlayerNetId;
        public byte Channel;
        public byte VoiceLevelScaled; // 0 a 255 (mapeado de 0.0f a 1.0f)
        public ushort AudioLength;
        public byte[] AudioData;

        public float VoiceLevel
        {
            get => VoiceLevelScaled / 255.0f;
            set => VoiceLevelScaled = (byte)(System.Math.Min(System.Math.Max(value, 0f), 1f) * 255f);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MAGIC_HEADER);
            writer.Put(PlayerNetId);
            writer.Put(Channel);
            writer.Put(VoiceLevelScaled);
            ushort len = (ushort)(AudioData != null ? AudioData.Length : 0);
            writer.Put(len);
            if (AudioData != null && len > 0)
            {
                writer.Put(AudioData, 0, len);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            try
            {
                Magic = reader.GetUInt();
                if (Magic != MAGIC_HEADER)
                {
                    PlayerNetId = 0;
                    AudioData = null;
                    return;
                }

                PlayerNetId = reader.GetByte();
                Channel = reader.GetByte();
                VoiceLevelScaled = reader.GetByte();
                AudioLength = reader.GetUShort();
                if (AudioLength > 0 && reader.AvailableBytes >= AudioLength)
                {
                    AudioData = new byte[AudioLength];
                    reader.GetBytes(AudioData, AudioLength);
                }
                else
                {
                    AudioData = null;
                }
            }
            catch
            {
                Magic = 0;
                PlayerNetId = 0;
                Channel = 0;
                VoiceLevelScaled = 0;
                AudioLength = 0;
                AudioData = null;
            }
        }
    }
}
