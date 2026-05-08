using System;

public class GClass829
{
	public double Alpha;

	[NonSerialized]
	public double Double_0;

	public double Average => Double_0;

	public void AddValue(double value)
	{
		Double_0 = Alpha * value + (1.0 - Alpha) * Double_0;
	}
}
