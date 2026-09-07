using Fika.Core.Networking.LiteNetLib.Utils;

namespace Manimal.LoadAmmoAnim.Fika.Packets
{
    // sent when the local player starts a mag-loading session. observers spawn
    // the bundle controller on their copy of the source player and play the
    // draw + use loop with dynamic magazine and ammo models.
    public struct LoadAmmoBundleStartPacket : INetSerializable
    {
        public int NetId;
        public string MagTemplateId;
        public float LoadOneAmmoSpeed;
        public string AmmoTemplateId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(MagTemplateId ?? string.Empty);
            writer.Put(LoadOneAmmoSpeed);
            writer.Put(AmmoTemplateId ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            MagTemplateId = reader.GetString();
            LoadOneAmmoSpeed = reader.GetFloat();
            AmmoTemplateId = !reader.EndOfData ? reader.GetString() : string.Empty;
        }
    }
}
