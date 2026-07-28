using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes.Packets;

public struct DismembermentPacket : INetSerializable
{
	public int playerID { get; set; }

	public string bone { get; set; }

	public string capAssetName { get; set; }

	public string[] assetNames { get; set; }

	public Vector3 Direction { get; set; }

	public EBodyPart bodyPartType { get; set; }

	public void Serialize(NetDataWriter writer)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		writer.Put(playerID);
		writer.Put(bone, 0);
		writer.Put(capAssetName, 0);
		writer.PutArray(assetNames);
		writer.PutUnmanaged<Vector3>(Direction);
		writer.Put((byte)bodyPartType);
	}

	public void Deserialize(NetDataReader reader)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		playerID = reader.GetInt();
		bone = reader.GetString();
		capAssetName = reader.GetString();
		assetNames = reader.GetStringArray();
		Direction = reader.GetUnmanaged<Vector3>();
		bodyPartType = (EBodyPart)reader.GetByte();
	}
}
