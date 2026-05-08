using System;
using System.IO;

namespace Audio.SpatialSystem;

[Serializable]
public struct AudioRouteData : IBinarySerializable
{
	public short PortalDataLength;

	public float HeuristicCost;

	public ushort StartIndex;

	public ushort NodeStartIndex;

	public void Write(BinaryWriter writer)
	{
		writer.Write(PortalDataLength);
		writer.Write(HeuristicCost);
		writer.Write(StartIndex);
		writer.Write(NodeStartIndex);
	}

	public void Read(BinaryReader reader)
	{
		PortalDataLength = reader.ReadInt16();
		HeuristicCost = reader.ReadSingle();
		StartIndex = reader.ReadUInt16();
		NodeStartIndex = reader.ReadUInt16();
	}
}
