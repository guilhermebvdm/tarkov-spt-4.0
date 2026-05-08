using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
[PreferBinarySerialization]
public class CullingGroupData : IBinarySerializable
{
	public ushort[] Indices;

	public ushort RuntimeCullingGroupIdx;

	public HashSet<ushort> BuildTimeIndices;

	public void Read(BinaryReader reader)
	{
		RuntimeCullingGroupIdx = reader.ReadUInt16();
		Indices = GClass881.ReadUInt16Array(reader);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(RuntimeCullingGroupIdx);
		GClass881.Write(writer, Indices);
	}
}
