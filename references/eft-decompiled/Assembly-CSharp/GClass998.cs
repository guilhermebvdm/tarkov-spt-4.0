using System;
using System.Collections.Generic;

public class GClass998<T> : IDisposable
{
	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Queue<int> Queue_0;

	[NonSerialized]
	public List<int> List_0;

	public int Count => List_0.Count;

	public T[] Data => Gparam_0;

	public int Used => Int_0;

	public List<int> ActiveIndexes => List_0;

	public T this[int index]
	{
		get
		{
			int num = Index(index);
			return Gparam_0[num];
		}
		set
		{
			int num = Index(index);
			Gparam_0[num] = value;
		}
	}

	public GClass998(int size)
	{
		Gparam_0 = new T[size];
		Int_0 = 0;
		Queue_0 = new Queue<int>(size);
		List_0 = new List<int>(size);
	}

	public int Add(T item)
	{
		int num;
		if (Queue_0.Count > 0)
		{
			num = Queue_0.Dequeue();
		}
		else
		{
			num = Int_0;
			Int_0++;
		}
		List_0.Add(num);
		Gparam_0[num] = item;
		return num;
	}

	public int Index(int i)
	{
		return List_0[i];
	}

	public T GetDataByIndex(int index)
	{
		return Gparam_0[index];
	}

	public void SetDataByIndex(int index, T value)
	{
		Gparam_0[index] = value;
	}

	public void Remove(int index)
	{
		if (!Queue_0.Contains(index))
		{
			Queue_0.Enqueue(index);
		}
		Gparam_0[index] = default(T);
		List_0.Remove(index);
	}

	public void Dispose()
	{
		Gparam_0 = Array.Empty<T>();
		Int_0 = 0;
		Queue_0.Clear();
		List_0.Clear();
	}
}
