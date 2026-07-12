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

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(IsFainted);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            IsFainted = reader.GetBool();
        }
    }
}
