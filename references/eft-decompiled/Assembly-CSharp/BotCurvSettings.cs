using UnityEngine;

public class BotCurvSettings : ScriptableObject
{
	private static BotCurvSettings botCurvSettings_0;

	public AnimationCurve NightVisionSettings;

	public AnimationCurve StandartVisionSettings;

	public AnimationCurve VisionAngCoef;

	public AnimationCurve AimTime2Dist;

	public AnimationCurve AimAngCoef;

	public AnimationCurve AgsAimUp;

	public AnimationCurve MoveStartCurv;

	public static BotCurvSettings Instance => botCurvSettings_0 ?? (botCurvSettings_0 = GClass861.Load<BotCurvSettings>("botCurvSettings"));

	public BotCurvSettings Copy()
	{
		return Object.Instantiate(this);
	}
}
