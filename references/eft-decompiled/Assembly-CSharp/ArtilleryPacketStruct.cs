using UnityEngine;

public struct ArtilleryPacketStruct
{
	public int id;

	public Vector3 position;

	public bool explosion;

	public void Serialize(GInterface131 writer, ArtilleryPacketStruct previousPacket)
	{
		Serialize(writer, hasPreviousPacket: true, previousPacket);
	}

	public void Serialize(GInterface131 writer)
	{
		Serialize(writer, hasPreviousPacket: false, default(ArtilleryPacketStruct));
	}

	public void Serialize(GInterface131 writer, bool hasPreviousPacket, ArtilleryPacketStruct previousPacket)
	{
		writer.Write(id);
		writer.Write(position);
		writer.Write(explosion);
	}

	public void Deserialize(IDataReader reader, ArtilleryPacketStruct previousPacket)
	{
		method_0(reader, hasPreviousPacket: true, previousPacket);
	}

	public void Deserialize(IDataReader reader)
	{
		method_0(reader, hasPreviousPacket: false, default(ArtilleryPacketStruct));
	}

	public void method_0(IDataReader reader, bool hasPreviousPacket, ArtilleryPacketStruct previousPacket)
	{
		id = reader.ReadInt32();
		position = reader.ReadVector3();
		explosion = reader.ReadBool();
	}
}
