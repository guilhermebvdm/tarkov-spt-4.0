using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Dismemberment.Classes.Packets;

public struct HitEffectPacket : INetSerializable
{
	public int PlayerID { get; set; }

	public Vector3 HitPoint { get; set; }

	public Vector3 Direction { get; set; }

	public bool IsAlive { get; set; }

	public string colliderName { get; set; }

	public void Deserialize(NetDataReader reader)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		PlayerID = reader.GetInt();
		HitPoint = reader.GetUnmanaged<Vector3>();
		Direction = reader.GetUnmanaged<Vector3>();
		IsAlive = reader.GetBool();
		colliderName = reader.GetString();
	}

	public void Serialize(NetDataWriter writer)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		writer.Put(PlayerID);
		writer.PutUnmanaged<Vector3>(HitPoint);
		writer.PutUnmanaged<Vector3>(Direction);
		writer.Put(IsAlive);
		writer.Put(colliderName, 0);
	}
}
