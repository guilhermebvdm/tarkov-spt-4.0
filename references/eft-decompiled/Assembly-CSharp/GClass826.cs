using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using UnityEngine;

public class GClass826<T> : IDisposable
{
	[NonSerialized]
	public ObjectPool<T[]> ObjectPool_0;

	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	public int Size
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public int ReservedSize => Gparam_0.Length;

	public T this[int index]
	{
		get
		{
			if (index >= Size)
			{
				throw new IndexOutOfRangeException();
			}
			return Gparam_0[(Int_0 + index) % Gparam_0.Length];
		}
		set
		{
			if (index >= Size)
			{
				throw new IndexOutOfRangeException();
			}
			Gparam_0[(Int_0 + index) % Gparam_0.Length] = value;
		}
	}

	public GClass826(int length)
	{
		Gparam_0 = new T[length];
		Int_0 = 0;
		Int_1 = 0;
		Size = 0;
	}

	public GClass826(ObjectPool<T[]> pool)
	{
		ObjectPool_0 = pool;
		Gparam_0 = pool.Get();
		Int_0 = 0;
		Int_1 = 0;
		Size = 0;
	}

	public GClass826(IList<T> list)
		: this(list.Count)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Gparam_0[i] = list[i];
		}
		Int_1 = list.Count;
		Size = list.Count;
	}

	public virtual void Dispose()
	{
		ObjectPool<T[]> objectPool_ = ObjectPool_0;
		ObjectPool_0 = null;
		objectPool_?.Return(Gparam_0);
		Gparam_0 = null;
	}

	public virtual void Enqueue(T packet)
	{
		if (Size == Gparam_0.Length)
		{
			Debug.LogError("Can't enqueue data");
			throw new OutOfMemoryException("Can't enqueue data");
		}
		Gparam_0[Int_1] = packet;
		Int_1 = (Int_1 + 1) % Gparam_0.Length;
		Size++;
	}

	public virtual T Dequeue()
	{
		if (Size == 0)
		{
			throw new InvalidOperationException();
		}
		T result = Gparam_0[Int_0];
		Gparam_0[Int_0] = default(T);
		Int_0 = (Int_0 + 1) % Gparam_0.Length;
		Size--;
		return result;
	}

	public T Peek()
	{
		if (Size == 0)
		{
			throw new InvalidOperationException();
		}
		return Gparam_0[Int_0];
	}

	public T PeekBack()
	{
		if (Size == 0)
		{
			throw new InvalidOperationException();
		}
		return Gparam_0[(Int_1 - 1 + Gparam_0.Length) % Gparam_0.Length];
	}

	public bool TryPeekBack(out T value)
	{
		if (Size > 0)
		{
			value = Gparam_0[(Int_1 - 1 + Gparam_0.Length) % Gparam_0.Length];
			return true;
		}
		value = default(T);
		return false;
	}

	public virtual void RemoveValues(int count)
	{
		if (count > Size)
		{
			throw new IndexOutOfRangeException();
		}
		if (Int_0 < Int_1)
		{
			Array.Clear(Gparam_0, Int_0, count);
		}
		else if (Int_0 + count <= Gparam_0.Length)
		{
			Array.Clear(Gparam_0, Int_0, count);
		}
		else
		{
			int num = Gparam_0.Length - Int_0;
			Array.Clear(Gparam_0, Int_0, num);
			Array.Clear(Gparam_0, 0, count - num);
		}
		Int_0 = (Int_0 + count) % Gparam_0.Length;
		Size -= count;
	}

	public virtual void Clear()
	{
		if (Int_0 < Int_1)
		{
			Array.Clear(Gparam_0, Int_0, Size);
		}
		else
		{
			Array.Clear(Gparam_0, Int_0, Gparam_0.Length - Int_0);
			Array.Clear(Gparam_0, 0, Int_1);
		}
		Int_0 = 0;
		Int_1 = 0;
		Size = 0;
	}
}
