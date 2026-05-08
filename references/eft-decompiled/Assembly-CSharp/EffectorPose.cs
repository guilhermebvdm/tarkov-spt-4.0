using EFT.Animations;
using UnityEngine;

public class EffectorPose : ScriptableObject
{
	[GAttribute7("Curve 240;Component 80;Intensity 120;AltIntensity 120", false)]
	public AnimVal[] Pos;

	[GAttribute7("Curve 240;Component 80;Intensity 120;AltIntensity 120", false)]
	public AnimVal[] Rot;
}
