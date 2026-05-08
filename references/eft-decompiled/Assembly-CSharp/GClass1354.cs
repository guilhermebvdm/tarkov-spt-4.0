using System;

public class GClass1354
{
	public readonly string GroupName;

	public readonly IFPSMeasureStatistics MeasureStatistics;

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public int Int_0;

	public GClass1354(string name, IFPSMeasureStatistics measureStatistics)
	{
		GroupName = name;
		MeasureStatistics = measureStatistics;
	}

	public void AddValue(double value)
	{
		Double_0 += value;
		Int_0++;
	}

	public void Commit()
	{
		if (Int_0 > 0)
		{
			double value = Double_0 / (double)Int_0;
			MeasureStatistics.AddMeasurement(new GStruct132
			{
				Value = value,
				DateTime = DateTime.UtcNow
			});
			Double_0 = 0.0;
			Int_0 = 0;
		}
	}
}
