using System;

public class GClass831
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public double[] Double_0;

	[NonSerialized]
	public double Double_1 = double.MinValue;

	[NonSerialized]
	public double Double_2 = double.MinValue;

	public int BufferSize => Int_0;

	public double Max => Double_2;

	public GClass831(int bufferSize)
	{
		Int_0 = bufferSize;
		Double_0 = new double[bufferSize];
	}

	public void AddValue(double value)
	{
		Int_1++;
		if (Double_0[Int_1 % Int_0] == Double_2)
		{
			Double_2 = Double_1;
		}
		Double_0[Int_1 % Int_0] = value;
		if (value >= Double_2)
		{
			Double_1 = Double_2;
			Double_2 = value;
		}
	}
}
