using System;

public class GClass828
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public double[] Double_1;

	[NonSerialized]
	public double Double_2;

	public int BufferSize => Int_0;

	public double Average => Double_2;

	public GClass828(int bufferSize)
	{
		Int_0 = bufferSize;
		Double_0 = bufferSize;
		Double_1 = new double[bufferSize];
	}

	public void AddValue(double value)
	{
		Int_1++;
		double num = Double_1[Int_1 % Int_0];
		Double_1[Int_1 % Int_0] = value;
		Double_2 -= num / Double_0;
		Double_2 += value / Double_0;
	}

	public void SetAll(double value)
	{
		for (int i = 0; i < Int_0; i++)
		{
			AddValue(value);
		}
	}
}
