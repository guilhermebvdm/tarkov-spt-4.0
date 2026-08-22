using Fika.Core.Networking.LiteNetLib.Utils;

namespace Manimal.LoadAmmoAnim.Fika.Packets
{
    // sent during a chained mag swap (CLA path). observers run a put-away,
    // swap the visible mesh, then play the draw on the same controller.
    public struct LoadAmmoBundleSwapMeshPacket : INetSerializable
    {
        public int NetId;
        public string NewMagTemplateId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(NewMagTemplateId ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            NewMagTemplateId = reader.GetString();
        }
    }
}
