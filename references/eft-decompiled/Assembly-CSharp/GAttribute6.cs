using UnityEngine;

public class GAttribute6 : PropertyAttribute
{
	public readonly float Min;

	public readonly float Max;

	public readonly float Round;

	public readonly float InvRound;

	public readonly bool UseRound;

	public GAttribute6(float min, float max, float round = -1f)
	{
		Min = min;
		Max = max;
		UseRound = round > 0f;
		Round = 1f / round;
		InvRound = round;
	}
}
