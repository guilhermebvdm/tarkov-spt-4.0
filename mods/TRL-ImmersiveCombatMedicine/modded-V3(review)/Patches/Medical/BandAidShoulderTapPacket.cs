using Fika.Core.Networking.LiteNetLib.Utils;
using Fika.Core.Networking;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Toque no ombro: sinaliza a um aliado que o remetente quer tratá-lo.
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    /// </summary>
    public struct BandAidShoulderTapPacketV2 : INetSerializable
    {
        public string SenderProfileId;
        public string SenderNickname;
        public string TargetProfileId;

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — ver BandAidHealPacketV2.Valid.</summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(SenderProfileId ?? string.Empty);
            inner.Put(SenderNickname ?? string.Empty);
            inner.Put(TargetProfileId ?? string.Empty);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            SenderProfileId = string.Empty;
            SenderNickname = string.Empty;
            TargetProfileId = string.Empty;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out SenderProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out SenderNickname)) return;
            if (!PacketEnvelope.TryReadString(inner, out TargetProfileId)) return;

            Valid = true;
        }
    }
}
