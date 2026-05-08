using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Class587<T> : IEnumerable<T>, IEnumerable
{
	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public int Length
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public T this[int i]
	{
		get
		{
			if (i < 0 || i >= Length)
			{
				throw new IndexOutOfRangeException();
			}
			return Gparam_0[method_0(i)];
		}
		set
		{
			if (i < 0 || i >= Length)
			{
				throw new IndexOutOfRangeException();
			}
			Gparam_0[method_0(i)] = value;
		}
	}

	public Class587(int length)
	{
		Length = length;
		Gparam_0 = new T[length];
	}

	public void Add(T value)
	{
		Int_0++;
		Int_0 %= Length;
		Gparam_0[Int_0] = value;
	}

	public int method_0(int i)
	{
		i += Int_0;
		return i % Length;
	}

	public int method_1(int i)
	{
		i = (Int_0 - i) % Length;
		if (i < 0)
		{
			i += Gparam_0.Length;
		}
		return i;
	}

	public void Clear()
	{
		Array.Clear(Gparam_0, 0, Gparam_0.Length);
		Int_0 = 0;
	}

	public T GetFromOldest(int i)
	{
		return Gparam_0[method_0(i)];
	}

	public void SetFromOldest(int i, T value)
	{
		Gparam_0[method_0(i)] = value;
	}

	public T GetFromNewest(int i)
	{
		return Gparam_0[method_1(i)];
	}

	public void SetFromNewest(int i, T value)
	{
		Gparam_0[method_1(i)] = value;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Length; i++)
		{
			yield return this[i];
		}
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
