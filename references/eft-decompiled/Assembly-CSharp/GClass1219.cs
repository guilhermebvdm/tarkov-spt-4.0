using System;
using System.Collections.Generic;

public class GClass1219 : GInterface114
{
	[NonSerialized]
	public List<ushort>[] List_0;

	[NonSerialized]
	public List<int>[] List_1;

	public int NumCells => List_0.Length;

	public List<ushort>[] RawCells => List_0;

	public List<int>[] RawPixels => List_1;

	public List<ushort> GetCellData(int index)
	{
		return List_0[index];
	}

	public List<int> GetPixelData(int index)
	{
		return List_1[index];
	}

	public void DisposeAt(int cellIndex)
	{
		List_0[cellIndex].Clear();
		List_0[cellIndex] = null;
		List_1[cellIndex].Clear();
		List_1[cellIndex] = null;
	}

	public GStruct109 GetSample(int cellIndex)
	{
		return new GStruct109(List_0[cellIndex], List_1[cellIndex]);
	}

	public GClass1219(int numCells)
	{
		List_0 = new List<ushort>[numCells];
		List_1 = new List<int>[numCells];
		for (int i = 0; i < List_0.Length; i++)
		{
			List_0[i] = new List<ushort>();
			List_1[i] = new List<int>();
		}
	}

	public void Dispose()
	{
		List_1 = null;
		List_0 = null;
	}

	public void Remap(GDelegate38 remapDelegate)
	{
		if (remapDelegate == null)
		{
			throw new ArgumentNullException("Remap delegate is null");
		}
		List<ushort>[] list_ = List_0;
		foreach (List<ushort> list in list_)
		{
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					list[j] = remapDelegate(list[j]);
				}
			}
		}
	}
}
