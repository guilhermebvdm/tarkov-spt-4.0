using Fika.Core.Networking.LiteNetLib.Utils;

namespace VisceralCombat.Ragdolls.Classes.Packets;

public struct RagdollSyncPacket : INetSerializable
{
	public int PlayerID { get; set; }

	public EBodyPart BodyPart { get; set; }

	public int RandomChance { get; set; }

	public void Deserialize(NetDataReader reader)
	{
		PlayerID = reader.GetInt();
		BodyPart = (EBodyPart)reader.GetByte();
		RandomChance = reader.GetInt();
	}

	public void Serialize(NetDataWriter writer)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		writer.Put(PlayerID);
		writer.Put((byte)BodyPart);
		writer.Put(RandomChance);
	}
}
