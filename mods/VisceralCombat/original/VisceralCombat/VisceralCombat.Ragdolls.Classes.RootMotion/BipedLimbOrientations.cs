using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[Serializable]
public class BipedLimbOrientations
{
	[Serializable]
	public class LimbOrientation
	{
		public Vector3 upperBoneForwardAxis;

		public Vector3 lowerBoneForwardAxis;

		public Vector3 lastBoneLeftAxis;

		public LimbOrientation(Vector3 upperBoneForwardAxis, Vector3 lowerBoneForwardAxis, Vector3 lastBoneLeftAxis)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			this.upperBoneForwardAxis = upperBoneForwardAxis;
			this.lowerBoneForwardAxis = lowerBoneForwardAxis;
			this.lastBoneLeftAxis = lastBoneLeftAxis;
		}
	}

	public LimbOrientation leftArm;

	public LimbOrientation rightArm;

	public LimbOrientation leftLeg;

	public LimbOrientation rightLeg;

	public static BipedLimbOrientations UMA => new BipedLimbOrientations(new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.forward), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.back), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.down), new LimbOrientation(Vector3.forward, Vector3.forward, Vector3.down));

	public static BipedLimbOrientations MaxBiped => new BipedLimbOrientations(new LimbOrientation(Vector3.down, Vector3.down, Vector3.down), new LimbOrientation(Vector3.down, Vector3.down, Vector3.up), new LimbOrientation(Vector3.up, Vector3.up, Vector3.back), new LimbOrientation(Vector3.up, Vector3.up, Vector3.back));

	public BipedLimbOrientations(LimbOrientation leftArm, LimbOrientation rightArm, LimbOrientation leftLeg, LimbOrientation rightLeg)
	{
		this.leftArm = leftArm;
		this.rightArm = rightArm;
		this.leftLeg = leftLeg;
		this.rightLeg = rightLeg;
	}
}
