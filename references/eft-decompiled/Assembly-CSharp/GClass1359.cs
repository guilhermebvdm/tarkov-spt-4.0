using System;

public class GClass1359
{
	public readonly string GroupName;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public bool Bool_0;

	public readonly IFPSMeasureStatistics MeasureStatistics;

	public GClass1359(string groupName, IFPSMeasureStatistics measureStatistics)
	{
		GroupName = groupName;
		MeasureStatistics = measureStatistics;
		Bool_0 = false;
	}

	public void StartCounting()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
		}
	}

	public void Increment()
	{
		if (Bool_0)
		{
			Int_0++;
		}
	}

	public void StopCounting()
	{
		if (Bool_0)
		{
			MeasureStatistics.AddMeasurement(new GStruct132
			{
				DateTime = DateTime.UtcNow,
				Value = Int_0
			});
			Int_0 = 0;
		}
	}
}
