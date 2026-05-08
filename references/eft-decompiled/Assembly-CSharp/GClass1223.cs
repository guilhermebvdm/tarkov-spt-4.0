using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GClass1223<T>
{
	[NonSerialized]
	public Queue<T> Queue_0 = new Queue<T>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Enqueue(T val)
	{
		lock (Queue_0)
		{
			Queue_0.Enqueue(val);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		lock (Queue_0)
		{
			Queue_0.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Dequeue()
	{
		lock (Queue_0)
		{
			return Queue_0.Dequeue();
		}
	}
}
