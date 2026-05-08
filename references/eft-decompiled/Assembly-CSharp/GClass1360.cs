using System;

public class GClass1360
{
	public readonly string GroupName;

	public readonly IFPSMeasureStatistics MeasureStatistics;

	[NonSerialized]
	public double Double_0 = double.MinValue;

	public GClass1360(string name, IFPSMeasureStatistics measureStatistics)
	{
		GroupName = name;
		MeasureStatistics = measureStatistics;
	}

	public void AddValue(double value)
	{
		if (value > Double_0)
		{
			Double_0 = value;
		}
	}

	public void Commit()
	{
		if (Double_0 > double.MinValue)
		{
			MeasureStatistics.AddMeasurement(new GStruct132
			{
				Value = Double_0,
				DateTime = DateTime.UtcNow
			});
			Double_0 = double.MinValue;
		}
	}
}
