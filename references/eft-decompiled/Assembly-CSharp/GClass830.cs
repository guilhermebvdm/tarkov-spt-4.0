using System;

public class GClass830
{
	public double Alpha = 0.6000000238418579;

	public double Beta = 0.800000011920929;

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public double Double_1;

	public double Average => Double_0;

	public void AddValue(double value)
	{
		double num = Alpha * value + (1.0 - Alpha) * Double_1;
		Double_1 = Beta * (num - Double_0) + (1.0 - Beta) * Double_1;
		Double_0 = num;
	}
}
