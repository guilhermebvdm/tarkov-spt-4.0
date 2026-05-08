using System;

public class GClass1304<T>
{
	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	public int Length => Gparam_0.Length;

	public T this[int i]
	{
		get
		{
			return Gparam_0[i];
		}
		set
		{
			method_0(i + 1);
			Gparam_0[i] = value;
		}
	}

	public GClass1304(int size, int extendSize)
	{
		Int_0 = extendSize;
		Gparam_0 = new T[size];
	}

	public void method_0(int targetSize)
	{
		if (Gparam_0.Length < targetSize)
		{
			Array.Resize(ref Gparam_0, method_1(Gparam_0.Length, targetSize, Int_0));
		}
	}

	public int method_1(int size, int targetSize, int extendSize)
	{
		return size + ((targetSize - size) / extendSize + 1) * extendSize;
	}
}
