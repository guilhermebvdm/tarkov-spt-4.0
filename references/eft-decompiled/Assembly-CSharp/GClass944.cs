using System;
using UnityEngine;

public class GClass944<T> where T : class, new()
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_1;

	public GClass944(int poolSize, int increasePoolStep)
	{
		Int_0 = increasePoolStep;
		Gparam_0 = new T[poolSize];
		for (int i = 0; i < Gparam_0.Length; i++)
		{
			Gparam_0[i] = new T();
		}
	}

	public T GetObject()
	{
		int num = 0;
		while (true)
		{
			if (num < Gparam_0.Length)
			{
				if (Gparam_0[num] != null)
				{
					break;
				}
				num++;
				continue;
			}
			method_0(Gparam_0.Length + 1);
			T result = Gparam_0[Gparam_0.Length - 1];
			Gparam_0[Gparam_0.Length - 1] = null;
			return result;
		}
		T result2 = Gparam_0[num];
		Gparam_0[num] = null;
		return result2;
	}

	public void PutObject(T item)
	{
		int num = 0;
		while (true)
		{
			if (num < Gparam_0.Length)
			{
				if (Gparam_0[num] == null)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		Gparam_0[num] = item;
	}

	public void method_0(int volume)
	{
		if (volume > Gparam_0.Length)
		{
			int num = Gparam_0.Length;
			T[] gparam_ = Gparam_0;
			Gparam_0 = new T[Mathf.Max(Gparam_0.Length + Int_0, volume)];
			for (int i = 0; i < gparam_.Length; i++)
			{
				Gparam_0[i] = gparam_[i];
			}
			for (int j = num; j < Gparam_0.Length; j++)
			{
				Gparam_0[j] = new T();
			}
		}
	}
}
