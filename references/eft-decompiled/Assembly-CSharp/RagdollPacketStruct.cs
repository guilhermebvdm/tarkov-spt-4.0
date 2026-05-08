using System;
using BitPacking;
using UnityEngine;

public struct RagdollPacketStruct
{
	[NonSerialized]
	public static GClass1373 Gclass1373_0 = new GClass1373(1f / 64f, BitPackingTag.CorpseSyncPacketRotationQuantizer);

	[NonSerialized]
	public const int Int_0 = 12;

	public int Id;

	public Vector3 Position;

	public bool Done;

	public GStruct138[] TransformSyncs;

	public bool IsNotValidPosition;

	public void Serialize(GInterface131 writer)
	{
		MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Write(writer, Position);
		writer.Write(Done);
		if (Done)
		{
			for (int i = 0; i < 12; i++)
			{
				GStruct138 gStruct = TransformSyncs[i];
				MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Write(writer, gStruct.Position);
				Gclass1373_0.Write(writer, gStruct.Rotation);
			}
		}
	}

	public void Deserialize(IDataReader reader)
	{
		MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Read(reader, out Position);
		Done = reader.ReadBool();
		if (Done)
		{
			TransformSyncs = new GStruct138[12];
			for (int i = 0; i < 12; i++)
			{
				GStruct138 gStruct = default(GStruct138);
				MovementInfoPacketStruct.USUAL_POSITION_QUANTIZER.Read(reader, out gStruct.Position);
				Gclass1373_0.Read(reader, out gStruct.Rotation);
				TransformSyncs[i] = gStruct;
			}
		}
	}
}
