using Fika.Core.Networking.LiteNetLib.Utils;

namespace Band_Aid
{
    /// <summary>
    /// ref: CR-01-02 — sincroniza o estado de desmaio entre peers (migrado do
    /// FikaPacketManager do TrueTrauma 3.11). Sem ele, o host nunca sabia que um
    /// client desmaiou e os bots continuavam atirando no desmaiado.
    /// Dono do estado → (rede) → host relay → todos.
    /// </summary>
    public struct TraumaFaintPacket : INetSerializable
    {
        public string ProfileId;
        public bool IsFainted;
        // ref: CR-02 — a duração viaja no pacote: o receptor NÃO pode usar a config
        // local (host com duração diferente do dono espelharia timers errados).
        public float DurationSeconds;
        public float GraceSeconds;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(IsFainted);
            writer.Put(DurationSeconds);
            writer.Put(GraceSeconds);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            IsFainted = reader.GetBool();
            DurationSeconds = reader.GetFloat();
            GraceSeconds = reader.GetFloat();
        }
    }
}
