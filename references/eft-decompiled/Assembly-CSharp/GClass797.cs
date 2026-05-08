using System;

public abstract class GClass797<T>
{
	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public T Gparam_1;

	[NonSerialized]
	public T Gparam_2;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public bool Bool_0;

	public T Average => Gparam_2;

	public GClass797(int count)
	{
		Int_0 = count;
		Gparam_0 = new T[Int_0];
	}

	public virtual void AddValue(T val)
	{
		Gparam_1 = Difference(Gparam_1, Gparam_0[Int_1]);
		Gparam_0[Int_1] = val;
		Gparam_1 = Summation(Gparam_1, val);
		Int_1 = (Int_1 + 1) % Int_0;
		if (!Bool_0 && Int_1 == 0)
		{
			Bool_0 = true;
		}
		Gparam_2 = Division(Gparam_1, Bool_0 ? Int_0 : Int_1);
	}

	public abstract T Difference(T minuend, T subtrahend);

	public abstract T Summation(T summandL, T summandR);

	public abstract T Division(T dividend, int divisor);
}
