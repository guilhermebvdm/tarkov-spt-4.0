using System;
using System.Collections.Generic;
using System.Linq;

public class GClass368<T> where T : IComparable<T>
{
	[NonSerialized]
	public List<T> List_0 = new List<T>();

	public int Count => List_0.Count;

	public void Add(T e)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				T other = List_0[num];
				if (e.CompareTo(other) == -1)
				{
					break;
				}
				num++;
				continue;
			}
			List_0.Add(e);
			return;
		}
		List_0.Insert(num, e);
	}

	public void Clear()
	{
		List_0.Clear();
	}

	public List<T> AllData()
	{
		return List_0;
	}

	public bool Remove(T floatCnt)
	{
		return List_0.Remove(floatCnt);
	}

	public T FirstOrDefault()
	{
		return List_0.FirstOrDefault();
	}

	public T TakeFirstAndRemove()
	{
		T val = List_0.FirstOrDefault();
		List_0.Remove(val);
		return val;
	}
}
