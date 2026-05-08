using System;

public class GClass1353 : IFPSMeasureStatistics
{
	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public GClass801 Gclass801_0;

	[NonSerialized]
	public string String_0;

	public string Name => String_0;

	public double Average => Gclass801_0.Average;

	public double LastValue => Double_0;

	public GClass1353(string name)
		: this(name, 60)
	{
	}

	public GClass1353(string name, int valuesForAverageCalculation)
	{
		String_0 = name;
		Gclass801_0 = new GClass801(valuesForAverageCalculation);
	}

	public void AddMeasurement(GStruct132 measurementData)
	{
		Gclass801_0.AddValue(measurementData.Value);
		Double_0 = measurementData.Value;
	}
}
