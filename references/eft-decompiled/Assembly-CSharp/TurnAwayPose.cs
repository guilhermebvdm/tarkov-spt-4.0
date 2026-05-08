using UnityEngine;

public class TurnAwayPose : ScriptableObject
{
	[Header("Order: Intensity FP, Intensity TP")]
	[GAttribute7("Curve 240;Component 40;Intensity 90;Intensity1 90;AltIntensity 90;AltIntensity1 90", false)]
	public TurnAwayEffector.AnimVal[] Pos;

	[GAttribute7("Curve 240;Component 40;Intensity 90;Intensity1 90;AltIntensity 90;AltIntensity1 90", false)]
	public TurnAwayEffector.AnimVal[] Rot;
}
