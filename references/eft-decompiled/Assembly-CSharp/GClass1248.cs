using System;
using System.Collections.Generic;
using System.IO;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public abstract class GClass1248
{
	public static bool smethod_0(PerfectCullingVolumeBakeData volumeBakeData)
	{
		return true;
	}

	public static List<VisibilitySetIndex> WriteBakeData(PerfectCullingVolumeBakeData volumeBakeData, string outputFile)
	{
		if (!smethod_0(volumeBakeData))
		{
			return null;
		}
		List<VisibilitySetIndex> list = new List<VisibilitySetIndex>();
		try
		{
			using BinaryWriter binaryWriter = new BinaryWriter(File.Create(outputFile));
			binaryWriter.Write(volumeBakeData.data.Length);
			PerfectCullingVolumeBakeData.VisibilitySet[] data = volumeBakeData.data;
			VisibilitySetIndex item = default(VisibilitySetIndex);
			for (int i = 0; i < data.Length; i++)
			{
				PerfectCullingVolumeBakeData.VisibilitySet visibilitySet = data[i];
				item.numDeltaValues = visibilitySet.numDeltaValues;
				item.offset = (int)binaryWriter.BaseStream.Position;
				if (visibilitySet.compressed != null)
				{
					if (visibilitySet.compressed.Length > 65535)
					{
						throw new InvalidOperationException("Data doesnt fits into max buffer size. Reduce number of groups.");
					}
					item.dataLength = (ushort)visibilitySet.compressed.Length;
				}
				else
				{
					item.dataLength = 0;
				}
				list.Add(item);
				if (item.numDeltaValues > 0)
				{
					binaryWriter.Write(visibilitySet.compressed);
				}
			}
			binaryWriter.Close();
			return list;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
			return null;
		}
	}
}
