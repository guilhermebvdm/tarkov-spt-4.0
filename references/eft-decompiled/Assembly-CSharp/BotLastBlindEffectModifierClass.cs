public class BotLastBlindEffectModifierClass
{
	public float PrecicingSpeedCoef = 1f;

	public float AccuratySpeedCoef = 1f;

	public float LayChanceDangerCoef = 1f;

	public float VisibleDistCoef = 1f;

	public float RuntimeVisionEffectK = 1f;

	public float ScatteringCoef = 1f;

	public float PriorityScatteringCoef = 1f;

	public float HearingDistCoef = 1f;

	public float TriggerDownDelay = 1f;

	public float WaitInCoverCoef = 1f;

	public bool IsApplyed;

	public BotLastBlindEffectModifierClass()
	{
	}

	public BotLastBlindEffectModifierClass(float precicingSpeedCoef, float accuratySpeedCoef, float layChanceDangerCoef, float visibleDistCoef, float runtimeVisionEffectK, float scatteringCoef, float hearingDistCoef, float priorityScatteringCoef, float triggerDownDelay)
	{
		PriorityScatteringCoef = priorityScatteringCoef;
		PrecicingSpeedCoef = precicingSpeedCoef;
		AccuratySpeedCoef = accuratySpeedCoef;
		LayChanceDangerCoef = layChanceDangerCoef;
		VisibleDistCoef = visibleDistCoef;
		RuntimeVisionEffectK = runtimeVisionEffectK;
		ScatteringCoef = scatteringCoef;
		HearingDistCoef = hearingDistCoef;
		TriggerDownDelay = triggerDownDelay;
	}
}
