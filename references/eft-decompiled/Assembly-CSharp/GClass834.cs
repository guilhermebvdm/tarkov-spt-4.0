using System;
using System.Collections.Generic;

public class GClass834<T>
{
	[NonSerialized]
	public Queue<List<T>> Queue_0;

	public GClass834(int size, int listSize = 0)
	{
		Queue_0 = new Queue<List<T>>(size);
		for (int i = 0; i < size; i++)
		{
			List<T> item = new List<T>(listSize);
			Queue_0.Enqueue(item);
		}
	}

	public List<T> Withdraw()
	{
		if (Queue_0.Count > 0)
		{
			return Queue_0.Dequeue();
		}
		return new List<T>();
	}

	public void Return(List<T> list)
	{
		list.Clear();
		Queue_0.Enqueue(list);
	}
}
