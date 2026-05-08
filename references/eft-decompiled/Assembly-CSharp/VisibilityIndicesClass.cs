using System;
using System.Runtime.CompilerServices;

public class VisibilityIndicesClass<T>
{
	public T[] Buffer;

	public volatile int Count;

	public static readonly global::VisibilityIndicesClass<T> Empty = new global::VisibilityIndicesClass<T>(0);

	public T this[int idx]
	{
		get
		{
			return Buffer[idx];
		}
		set
		{
			Buffer[idx] = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Dequeue()
	{
		Count--;
		return Buffer[Count];
	}

	public VisibilityIndicesClass(int cap)
	{
		Buffer = new T[cap];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T val)
	{
		Buffer[Count] = val;
		Count++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		Count = 0;
	}

	public T[] Clone()
	{
		if (Count == 0)
		{
			return Array.Empty<T>();
		}
		T[] array = new T[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = Buffer[i];
		}
		return array;
	}
}
