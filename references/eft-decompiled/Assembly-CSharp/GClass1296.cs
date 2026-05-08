using System;
using System.Collections.Concurrent;

public class GClass1296<T>
{
	[NonSerialized]
	public ConcurrentBag<T> ConcurrentBag_0 = new ConcurrentBag<T>();

	[NonSerialized]
	public Func<T> Func_0;

	public int Count => ConcurrentBag_0.Count;

	public GClass1296(Func<T> objectGenerator, int initialCapacity)
	{
		Func_0 = objectGenerator;
		for (int i = 0; i < initialCapacity; i++)
		{
			ConcurrentBag_0.Add(objectGenerator());
		}
	}

	public T Get()
	{
		if (!ConcurrentBag_0.TryTake(out var result))
		{
			return Func_0();
		}
		return result;
	}

	public void Return(T item)
	{
		ConcurrentBag_0.Add(item);
	}
}
