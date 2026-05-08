using System;
using BitPacking;
using UnityEngine;

public struct LootSyncStruct
{
	public int Id;

	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Velocity;

	public Vector3 AngularVelocity;

	public bool Done;

	[NonSerialized]
	public static GClass1373 Gclass1373_0 = new GClass1373(1f / 64f, BitPackingTag.LootSyncPacketRotationQuantizer);

	[NonSerialized]
	public static GClass1378 Gclass1378_0 = new GClass1378(-30f, 30f, 0.125f, -30f, 10f, 0.125f, -30f, 30f, 0.125f, checkBounds: true);

	[NonSerialized]
	public static GClass1378 Gclass1378_1 = new GClass1378(-160f, 160f, 0.125f, -160f, 160f, 0.125f, -160f, 160f, 0.125f, checkBounds: true);

	public void Serialize(GInterface131 writer)
	{
		MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Write(writer, Position);
		Gclass1373_0.Write(writer, Rotation);
		writer.Write(Done);
		if (!Done)
		{
			Gclass1378_0.Write(writer, Velocity);
			Gclass1378_1.Write(writer, AngularVelocity);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Read(reader, out Position);
		Gclass1373_0.Read(reader, out Rotation);
		Done = reader.ReadBool();
		if (!Done)
		{
			Gclass1378_0.Read(reader, out Velocity);
			Gclass1378_1.Read(reader, out AngularVelocity);
		}
	}
}
