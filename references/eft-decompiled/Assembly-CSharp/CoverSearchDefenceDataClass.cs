using System;

public class CoverSearchDefenceDataClass
{
	[Obsolete]
	public float minDefenceLevel;

	[Obsolete]
	public int maxDangerLevel;

	public CoverSearchDefenceDataClass(float minDefenceLevel, int maxDangerLevel)
	{
		this.minDefenceLevel = minDefenceLevel;
		this.maxDangerLevel = maxDangerLevel;
	}

	public CoverSearchDefenceDataClass(float minDefenceLevel)
	{
		this.minDefenceLevel = minDefenceLevel;
		maxDangerLevel = 999;
	}

	public override string ToString()
	{
		return $"DefenceData min:{minDefenceLevel}  max:{maxDangerLevel}";
	}
}
