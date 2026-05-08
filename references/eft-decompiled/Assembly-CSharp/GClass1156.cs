using System;

public class GClass1156
{
	[NonSerialized]
	public const float Float_0 = 3f;

	public float Compute(float heightDiff, float floorHeightSetting, LoggerClass logger)
	{
		if (floorHeightSetting > 0f)
		{
			return heightDiff / floorHeightSetting;
		}
		logger.LogWarn($"Incorrect floorHeight value ({floorHeightSetting}), use base value {3f}");
		return heightDiff / 3f;
	}
}
