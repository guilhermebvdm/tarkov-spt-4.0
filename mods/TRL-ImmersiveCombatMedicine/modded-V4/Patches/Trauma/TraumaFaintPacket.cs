using Fika.Core.Networking.LiteNetLib.Utils;
using TRLImmersiveCombatMedicine.Medical;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>
    /// ref: CR-01-02 — sincroniza o estado de desmaio entre peers (migrado do
    /// FikaPacketManager do TrueTrauma 3.11). Sem ele, o host nunca sabia que um
    /// client desmaiou e os bots continuavam atirando no desmaiado.
    /// Dono do estado → (rede) → host relay → todos.
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    /// </summary>
    public struct TraumaFaintPacketV2 : INetSerializable
    {
        public string ProfileId;
        public bool IsFainted;
        // ref: CR-02 — a duração viaja no pacote: o receptor NÃO pode usar a config
        // local (host com duração diferente do dono espelharia timers errados).
        public float DurationSeconds;
        public float GraceSeconds;

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — ver BandAidHealPacketV2.Valid.</summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(ProfileId ?? string.Empty);
            inner.Put(IsFainted);
            inner.Put(DurationSeconds);
            inner.Put(GraceSeconds);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            IsFainted = false;
            DurationSeconds = 0f;
            GraceSeconds = 0f;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out ProfileId)) return;
            if (!inner.TryGetBool(out IsFainted)) return;
            if (!inner.TryGetFloat(out DurationSeconds)) return;
            if (!inner.TryGetFloat(out GraceSeconds)) return;

            Valid = true;
        }
    }
}
