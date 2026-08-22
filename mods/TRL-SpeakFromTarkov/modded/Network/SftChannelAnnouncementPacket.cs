using System;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov.Network
{
    public struct SftChannelAnnouncementPacket : INetSerializable
    {
        public byte ChannelId;
#pragma warning disable CS8618
        public string ChannelName;
        public string HostProfileId;
        public string HostNickname;
        public string TargetProfileId;
        public byte Action; // 0 = Announce, 1 = Close, 2 = Join, 3 = Leave, 4 = Kick, 5 = Ban

        [ThreadStatic] private static NetDataWriter? _innerWriter;
#pragma warning restore CS8618

        public void Serialize(NetDataWriter writer)
        {
            var inner = _innerWriter ??= new NetDataWriter(true, 1024);
            inner.Reset();

            inner.Put(ChannelId);
            inner.Put(ChannelName ?? string.Empty);
            inner.Put(HostProfileId ?? string.Empty);
            inner.Put(HostNickname ?? string.Empty);
            inner.Put(TargetProfileId ?? string.Empty);
            inner.Put(Action);

            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            ChannelId = 0;
            ChannelName = string.Empty;
            HostProfileId = string.Empty;
            HostNickname = string.Empty;
            TargetProfileId = string.Empty;
            Action = 0;

            if (!reader.TryGetBytesWithLength(out var payload)) return;

            var inner = new NetDataReader(payload);
            inner.TryGetByte(out ChannelId);
            inner.TryGetString(out ChannelName);
            inner.TryGetString(out HostProfileId);
            inner.TryGetString(out HostNickname);
            inner.TryGetString(out TargetProfileId);
            inner.TryGetByte(out Action);
        }
    }
}
