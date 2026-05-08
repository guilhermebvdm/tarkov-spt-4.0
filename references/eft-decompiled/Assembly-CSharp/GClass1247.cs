using System;
using System.IO;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public class GClass1247
{
	[NonSerialized]
	public StreamingVolumeBakeDataIndex StreamingVolumeBakeDataIndex_0;

	[NonSerialized]
	public FileStream FileStream_0;

	[NonSerialized]
	public GClass1231 Gclass1231_0;

	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public byte[] Byte_1;

	public volatile int bytesRead;

	public volatile int readOps;

	[NonSerialized]
	public bool Bool_0;

	public StreamingVolumeBakeDataIndex StreamData => StreamingVolumeBakeDataIndex_0;

	public int GetRuntimeMemorySizeBytes()
	{
		int num = 0;
		if (Bool_0)
		{
			num += Byte_1.Length;
		}
		return num + StreamingVolumeBakeDataIndex_0.IndexSizeBytes;
	}

	public GClass1247(StreamingVolumeBakeDataIndex streamData, string filePath, PerfectCullingCrossSceneVolume.BakeDescriptor.LoadMode loadMode)
	{
		Bool_0 = loadMode == PerfectCullingCrossSceneVolume.BakeDescriptor.LoadMode.Memory;
		StreamingVolumeBakeDataIndex_0 = streamData;
		try
		{
			FileStream_0 = File.OpenRead(filePath);
		}
		catch (Exception)
		{
			Debug.LogWarning("Cant open file " + filePath);
			FileStream_0 = null;
		}
		Byte_0 = new byte[131070];
		Gclass1231_0 = new GClass1231();
		if (Bool_0)
		{
			Byte_1 = File.ReadAllBytes(filePath);
		}
	}

	public void method_0(byte[] compressed, int offset, int len, global::VisibilityIndicesClass<ushort> indices)
	{
		indices.Clear();
		Gclass1231_0.Reset(compressed);
		Gclass1231_0.SetOffset(offset);
		ushort num = 0;
		for (int i = 0; i < len; i++)
		{
			uint num2 = Gclass1231_0.Read(2);
			uint bits = PerfectCullingVolumeBakeData.BITS[num2];
			uint num3 = Gclass1231_0.Read((int)bits);
			num += (ushort)num3;
			if (num3 < 15)
			{
				if (indices.Count >= indices.Buffer.Length)
				{
					break;
				}
				indices.Add(num);
			}
		}
	}

	public void method_1(byte[] compressed, int len, global::VisibilityIndicesClass<ushort> indices)
	{
		indices.Clear();
		Gclass1231_0.Reset(compressed);
		ushort num = 0;
		for (int i = 0; i < len; i++)
		{
			uint num2 = Gclass1231_0.Read(2);
			uint bits = PerfectCullingVolumeBakeData.BITS[num2];
			uint num3 = Gclass1231_0.Read((int)bits);
			num += (ushort)num3;
			if (num3 < 15)
			{
				if (indices.Count >= indices.Buffer.Length)
				{
					break;
				}
				indices.Add(num);
			}
		}
	}

	public int SampleByIndexFromDisk(int index, global::VisibilityIndicesClass<ushort> buffer)
	{
		if (FileStream_0 == null)
		{
			return 0;
		}
		VisibilitySetIndex visibilitySetIndex = StreamingVolumeBakeDataIndex_0.indices[index];
		if (visibilitySetIndex.dataLength == 0)
		{
			return 0;
		}
		if (Bool_0)
		{
			method_0(Byte_1, visibilitySetIndex.offset, visibilitySetIndex.numDeltaValues, buffer);
			bytesRead += visibilitySetIndex.dataLength;
			return visibilitySetIndex.dataLength;
		}
		FileStream_0.Seek(visibilitySetIndex.offset, SeekOrigin.Begin);
		int num = 0;
		do
		{
			int num2 = FileStream_0.Read(Byte_0, num, visibilitySetIndex.dataLength - num);
			readOps++;
			if (num2 == 0)
			{
				break;
			}
			num += num2;
		}
		while (num != visibilitySetIndex.dataLength);
		method_1(Byte_0, visibilitySetIndex.numDeltaValues, buffer);
		bytesRead += visibilitySetIndex.dataLength;
		return visibilitySetIndex.dataLength;
	}

	public void Dispose()
	{
		if (FileStream_0 != null)
		{
			FileStream_0.Close();
			FileStream_0.Dispose();
			FileStream_0 = null;
		}
		Byte_1 = null;
		Gclass1231_0 = null;
		Byte_0 = null;
		StreamingVolumeBakeDataIndex_0 = null;
	}
}
