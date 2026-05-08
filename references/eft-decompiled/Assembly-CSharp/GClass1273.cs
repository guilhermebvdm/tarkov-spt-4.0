using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1273<T> where T : GClass1258
{
	public int cellRowAndCollumnCountPerTerrain;

	public List<T> activeCellList;

	[NonSerialized]
	public Dictionary<int, T> Dictionary_0;

	[NonSerialized]
	public List<T> List_0;

	public GClass1273()
	{
		activeCellList = new List<T>();
		Dictionary_0 = new Dictionary<int, T>();
		List_0 = new List<T>();
	}

	public void AddCell(T cell)
	{
		Dictionary_0.Add(cell.CalculateHash(), cell);
		List_0.Add(cell);
	}

	public bool GetCell(int hash, out T cell)
	{
		return Dictionary_0.TryGetValue(hash, out cell);
	}

	public List<T> GetCellList()
	{
		return List_0;
	}

	public void GetCell(T cell)
	{
		Dictionary_0.Add(cell.CalculateHash(), cell);
		List_0.Add(cell);
	}

	public bool IsActiveCellUpdateRequired(Vector3 position)
	{
		return CalculateActiveCells(position);
	}

	public bool CalculateActiveCells(Vector3 position)
	{
		bool result = false;
		foreach (T item in List_0)
		{
			if (item.cellBounds.Contains(position))
			{
				if (!item.isActive)
				{
					item.isActive = true;
					activeCellList.Add(item);
					result = true;
				}
			}
			else if (item.isActive)
			{
				item.isActive = false;
				GClass1274.ReleaseSPCell(item);
				activeCellList.Remove(item);
				result = true;
			}
		}
		return result;
	}
}
