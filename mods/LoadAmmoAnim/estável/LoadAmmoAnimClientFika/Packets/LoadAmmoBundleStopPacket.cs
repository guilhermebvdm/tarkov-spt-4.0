using Fika.Core.Networking.LiteNetLib.Utils;

namespace Manimal.LoadAmmoAnim.Fika.Packets
{
    // sent when the local player's session ends. observers play the put-away
    // and tear down the bundle controller. PlayPutAway=false is reserved for
    // future use (eg. a "snap end" path) but currently always true.
    public struct LoadAmmoBundleStopPacket : INetSerializable
    {
        public int NetId;
        public bool PlayPutAway;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(PlayPutAway);
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            PlayPutAway = reader.GetBool();
        }
    }
}
