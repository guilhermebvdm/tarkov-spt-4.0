using System;
using System.Collections.Generic;

public class GClass835<T> : IDisposable
{
	[NonSerialized]
	public Queue<T> Queue_0;

	[NonSerialized]
	public Func<T> Func_0;

	[NonSerialized]
	public Action<T> Action_0;

	[NonSerialized]
	public Action<T> Action_1;

	public GClass835(int size, Func<T> constructor, Action<T> onReturnInPool = null, Action<T> destructor = null)
	{
		Queue_0 = new Queue<T>(size);
		Func_0 = constructor;
		Action_0 = onReturnInPool;
		Action_1 = destructor;
		for (int i = 0; i < size; i++)
		{
			T item = Func_0();
			Queue_0.Enqueue(item);
		}
	}

	public T Withdraw()
	{
		if (Queue_0.Count > 0)
		{
			return Queue_0.Dequeue();
		}
		return Func_0();
	}

	public void Return(T t)
	{
		Action_0?.Invoke(t);
		Queue_0.Enqueue(t);
	}

	public void Dispose()
	{
		if (Action_1 != null)
		{
			foreach (T item in Queue_0)
			{
				Action_1(item);
			}
		}
		Queue_0.Clear();
	}
}
