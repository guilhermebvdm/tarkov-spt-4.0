using System;

public class GClass837
{
	[NonSerialized]
	public double[] Double_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1 = -1;

	[NonSerialized]
	public int Int_2 = -1;

	[NonSerialized]
	public int Int_3 = -1;

	public double RangeDiff
	{
		get
		{
			int num = (Int_3 - Int_1 + Int_0) % Int_0;
			int num2 = (Int_2 - Int_1 + Int_0) % Int_0;
			if (num < num2)
			{
				return Double_0[Int_2] - Double_0[Int_3];
			}
			return Double_0[Int_3] - Double_0[Int_2];
		}
	}

	public GClass837(int count)
	{
		if (count < 1)
		{
			count = 1;
		}
		Double_0 = new double[count];
		Int_0 = count;
	}

	public void AddValue(double val)
	{
		if (Int_1 < 0)
		{
			Int_1 = 0;
			method_1();
			method_0();
		}
		Double_0[Int_1] = val;
		if (val < Double_0[Int_3])
		{
			Int_3 = Int_1;
		}
		else if (val > Double_0[Int_2])
		{
			Int_2 = Int_1;
		}
		else
		{
			if (Int_1 == Int_3)
			{
				method_1();
			}
			if (Int_1 == Int_2)
			{
				method_0();
			}
		}
		Int_1 = (Int_1 + 1) % Int_0;
	}

	public void method_0()
	{
		double num = double.MinValue;
		for (int i = 0; i < Double_0.Length; i++)
		{
			double num2 = Double_0[i];
			if (num2 > num)
			{
				num = num2;
				Int_2 = i;
			}
		}
	}

	public void method_1()
	{
		double num = double.MaxValue;
		for (int i = 0; i < Double_0.Length; i++)
		{
			double num2 = Double_0[i];
			if (num2 < num)
			{
				num = num2;
				Int_3 = i;
			}
		}
	}
}
