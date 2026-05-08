using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GClass1300<T>
{
	[NonSerialized]
	public Stack<T> Stack_0 = new Stack<T>();

	[NonSerialized]
	public Func<T> Func_0;

	public int Count => Stack_0.Count;

	public GClass1300(Func<T> objectGenerator, int initialCapacity)
	{
		Func_0 = objectGenerator;
		for (int i = 0; i < initialCapacity; i++)
		{
			Stack_0.Push(objectGenerator());
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Get()
	{
		if (Stack_0.Count <= 0)
		{
			return Func_0();
		}
		return Stack_0.Pop();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Return(T item)
	{
		Stack_0.Push(item);
	}
}
