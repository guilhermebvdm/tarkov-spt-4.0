using System;
using System.Collections.Generic;

public class GClass1207 : GInterface114
{
	public static readonly List<int> EmptyPixelList = new List<int>();

	[NonSerialized]
	public List<ushort>[] List_0;

	public int NumCells => List_0.Length;

	public List<ushort>[] RawCells => List_0;

	public List<int>[] RawPixels => null;

	public List<ushort> GetCellData(int index)
	{
		return List_0[index];
	}

	public List<int> GetPixelData(int index)
	{
		return null;
	}

	public void DisposeAt(int cellIndex)
	{
		List_0[cellIndex].Clear();
		List_0[cellIndex] = null;
	}

	public GStruct109 GetSample(int cellIndex)
	{
		return new GStruct109(List_0[cellIndex], GStruct109.Empty);
	}

	public GClass1207(int numCells)
	{
		List_0 = new List<ushort>[numCells];
		for (int i = 0; i < List_0.Length; i++)
		{
			List_0[i] = new List<ushort>();
		}
	}

	public void Dispose()
	{
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
