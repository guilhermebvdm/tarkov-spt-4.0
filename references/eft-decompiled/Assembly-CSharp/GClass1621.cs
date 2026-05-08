using System;

public class GClass1621
{
	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public int Int_0;

	public double Average
	{
		get
		{
			if (Int_0 <= 0)
			{
				return 0.0;
			}
			return Double_0 / (double)Int_0;
		}
	}

	public double Sum => Double_0;

	public double Count => Int_0;

	public void AddValue(double value)
	{
		Double_0 += value;
		Int_0++;
	}
}
