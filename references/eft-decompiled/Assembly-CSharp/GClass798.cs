using System;

public abstract class GClass798<T> : GClass797<T>
{
	[NonSerialized]
	public int Int_2 = -1;

	[NonSerialized]
	public int Int_3 = -1;

	public T Min => Gparam_0[Int_3];

	public T Max => Gparam_0[Int_2];

	public GClass798(int count)
		: base(count)
	{
	}

	public override void AddValue(T val)
	{
		bool flag = false;
		if (Int_2 >= 0 && !IsGreaterOrEqual(val, Max))
		{
			if (Int_1 == Int_2)
			{
				flag = true;
			}
		}
		else
		{
			Int_2 = Int_1;
		}
		bool flag2 = false;
		if (Int_3 >= 0 && !IsLessOrEqual(val, Min))
		{
			if (Int_1 == Int_3)
			{
				flag2 = true;
			}
		}
		else
		{
			Int_3 = Int_1;
		}
		base.AddValue(val);
		if (flag)
		{
			int num = (Bool_0 ? Int_0 : Int_1);
			Int_2 = 0;
			for (int i = 1; i < num; i++)
			{
				if (IsGreaterOrEqual(Gparam_0[i], Max))
				{
					Int_2 = i;
				}
			}
		}
		if (!flag2)
		{
			return;
		}
		int num2 = (Bool_0 ? Int_0 : Int_1);
		Int_3 = 0;
		for (int j = 1; j < num2; j++)
		{
			if (IsLessOrEqual(Gparam_0[j], Min))
			{
				Int_3 = j;
			}
		}
	}

	public abstract bool IsGreaterOrEqual(T rhs, T lhs);

	public abstract bool IsLessOrEqual(T rhs, T lhs);
}
