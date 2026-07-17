using Fika.Core.Networking.LiteNetLib.Utils;

namespace CameraRotationMod.Networking
{
    public struct StanceSyncPacket : INetSerializable
    {
        public string ProfileId;
        public int Stance;
        public bool IsAiming;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(Stance);
            writer.Put(IsAiming);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            Stance = reader.GetInt();
            IsAiming = reader.GetBool();
        }
    }
}
