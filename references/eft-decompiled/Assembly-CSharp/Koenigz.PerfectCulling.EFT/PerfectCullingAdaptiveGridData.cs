using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
[CreateAssetMenu(fileName = "New grid data", menuName = "Perfect Culling/Adaptive grid data", order = 1)]
[PreferBinarySerialization]
public class PerfectCullingAdaptiveGridData : ScriptableObject
{
	[SerializeField]
	public List<CullingCellData> CellData;

	[SerializeField]
	public List<GuidReference> GroupMapping;

	public const int INVALID_GROUP_INDEX = -1;

	public int NumCells => CellData.Count;

	public void ReadFromFile(string filePath, CancellationToken token, IProgress<float> progress = null)
	{
		using FileStream input = File.OpenRead(filePath);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		GroupMapping = new List<GuidReference>();
		for (int i = 0; i < num; i++)
		{
			int count = binaryReader.ReadInt32();
			byte[] guidBytes = binaryReader.ReadBytes(count);
			GroupMapping.Add(new GuidReference(guidBytes));
		}
		int num2 = binaryReader.ReadInt32();
		CellData = new List<CullingCellData>();
		for (int j = 0; j < num2; j++)
		{
			CullingCellData cullingCellData = new CullingCellData();
			cullingCellData.Read(binaryReader);
			CellData.Add(cullingCellData);
			if (j % 10000 == 0)
			{
				progress?.Report((float)j / (float)num2);
			}
			if (token.IsCancellationRequested)
			{
				break;
			}
		}
	}

	public int GetGroupIndex(GuidReference groupGuid)
	{
		int num = 0;
		while (true)
		{
			if (num < GroupMapping.Count)
			{
				if (GroupMapping[num] == groupGuid)
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}
}
