using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class BipedIKSolvers
{
	public IKSolverLimb leftFoot = new IKSolverLimb(AvatarIKGoal.LeftFoot);

	public IKSolverLimb rightFoot = new IKSolverLimb(AvatarIKGoal.RightFoot);

	public IKSolverLimb leftHand = new IKSolverLimb(AvatarIKGoal.LeftHand);

	public IKSolverLimb rightHand = new IKSolverLimb(AvatarIKGoal.RightHand);

	public IKSolverFABRIK spine = new IKSolverFABRIK();

	public IKSolverLookAt lookAt = new IKSolverLookAt();

	public IKSolverAim aim = new IKSolverAim();

	public Constraints pelvis = new Constraints();

	[NonSerialized]
	public IKSolverLimb[] Limbs;

	[NonSerialized]
	public IKSolver[] IkSolvers;

	public IKSolverLimb[] limbs
	{
		get
		{
			if (Limbs == null || (Limbs != null && Limbs.Length != 4))
			{
				Limbs = new IKSolverLimb[4] { leftFoot, rightFoot, leftHand, rightHand };
			}
			return Limbs;
		}
	}

	public IKSolver[] ikSolvers
	{
		get
		{
			if (IkSolvers == null || (IkSolvers != null && IkSolvers.Length != 7))
			{
				IkSolvers = new IKSolver[7] { leftFoot, rightFoot, leftHand, rightHand, spine, lookAt, aim };
			}
			return IkSolvers;
		}
	}

	public void AssignReferences(BipedReferences references)
	{
		leftHand.SetChain(references.leftUpperArm, references.leftForearm, references.leftHand, references.root);
		rightHand.SetChain(references.rightUpperArm, references.rightForearm, references.rightHand, references.root);
		leftFoot.SetChain(references.leftThigh, references.leftCalf, references.leftFoot, references.root);
		rightFoot.SetChain(references.rightThigh, references.rightCalf, references.rightFoot, references.root);
		spine.SetChain(references.spine, references.root);
		lookAt.SetChain(references.spine, references.head, references.eyes, references.root);
		aim.SetChain(references.spine, references.root);
		leftFoot.goal = AvatarIKGoal.LeftFoot;
		rightFoot.goal = AvatarIKGoal.RightFoot;
		leftHand.goal = AvatarIKGoal.LeftHand;
		rightHand.goal = AvatarIKGoal.RightHand;
	}
}
