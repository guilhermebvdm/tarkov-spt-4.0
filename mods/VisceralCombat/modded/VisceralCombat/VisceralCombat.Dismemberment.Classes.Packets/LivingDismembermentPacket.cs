using EFT;
using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes.Packets;

public struct LivingDismembermentPacket : INetSerializable
{
	public int PlayerID { get; set; }
	public EBodyPart Leg { get; set; }
	public Vector3 Direction { get; set; }
	public string Bone { get; set; }
	public string CapAssetName { get; set; }
	public string[] AssetNames { get; set; }

	public void Serialize(NetDataWriter writer)
	{
		writer.Put(PlayerID);
		writer.Put((byte)Leg);
		writer.PutUnmanaged<Vector3>(Direction);
		writer.Put(Bone ?? "", 0);
		writer.Put(CapAssetName ?? "", 0);
		writer.PutArray(AssetNames ?? System.Array.Empty<string>());
	}

	public void Deserialize(NetDataReader reader)
	{
		PlayerID = reader.GetInt();
		Leg = (EBodyPart)reader.GetByte();
		Direction = reader.GetUnmanaged<Vector3>();
		Bone = reader.GetString();
		CapAssetName = reader.GetString();
		AssetNames = reader.GetStringArray();
	}
}
