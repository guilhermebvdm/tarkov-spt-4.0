using System;

public struct GStruct124(int n)
{
	[NonSerialized]
	public float Float_0 = 2f / (float)(n + 1);

	[NonSerialized]
	public bool Bool_0 = false;

	public double Value = 0.0;

	public double Variance = 0.0;

	public double StandardDeviation = 0.0;

	public void SetAlpha(int n)
	{
		Float_0 = 2f / (float)(n + 1);
	}

	public void Add(double newValue)
	{
		if (Bool_0)
		{
			double num = newValue - Value;
			Value += (double)Float_0 * num;
			Variance = (double)(1f - Float_0) * (Variance + (double)Float_0 * num * num);
			StandardDeviation = Math.Sqrt(Variance);
		}
		else
		{
			Value = newValue;
			Bool_0 = true;
		}
	}
}
