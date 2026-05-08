using System;

public class GClass1362
{
	public readonly string GroupName;

	public readonly IFPSMeasureStatistics MeasureStatistics;

	[NonSerialized]
	public float Float_0;

	public GClass1362(string name, IFPSMeasureStatistics measureStatistics)
	{
		GroupName = name;
		MeasureStatistics = measureStatistics;
	}

	public void StartMeasure(float value)
	{
		Float_0 = value;
	}

	public void StopMeasure(float value)
	{
		float num = value - Float_0;
		MeasureStatistics.AddMeasurement(new GStruct132
		{
			Value = num,
			DateTime = DateTime.UtcNow
		});
	}
}
