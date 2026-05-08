using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Finger
{
	[Tooltip("Master Weight for the finger.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The first bone of the finger.")]
	public Transform bone1;

	[Tooltip("The second bone of the finger.")]
	public Transform bone2;

	[Tooltip("The (optional) third bone of the finger. This can be ignored for thumbs.")]
	public Transform bone3;

	[Tooltip("The fingertip object. If your character doesn't have tip bones, you can create an empty GameObject and parent it to the last bone in the finger. Place it to the tip of the finger.")]
	public Transform tip;

	[Tooltip("The IK target (optional, can use IKPosition and IKRotation directly).")]
	public Transform target;

	[NonSerialized]
	public IKSolverLimb Solver;

	[NonSerialized]
	public Quaternion Bone3RelativeToTarget;

	[NonSerialized]
	public Vector3 Bone3DefaultLocalPosition;

	[NonSerialized]
	public Quaternion Bone3DefaultLocalRotation;

	[field: NonSerialized]
	public bool initiated { get; set; }

	public Vector3 IKPosition
	{
		get
		{
			return Solver.IKPosition;
		}
		set
		{
			Solver.IKPosition = value;
		}
	}

	public Quaternion IKRotation
	{
		get
		{
			return Solver.IKRotation;
		}
		set
		{
			Solver.IKRotation = value;
		}
	}

	public bool IsValid(ref string errorMessage)
	{
		if (!(bone1 == null) && !(bone2 == null) && !(tip == null))
		{
			return true;
		}
		errorMessage = "One of the bones in the Finger Rig is null, can not initiate solvers.";
		return false;
	}

	public void Initiate(Transform hand, int index)
	{
		initiated = false;
		string errorMessage = string.Empty;
		if (!IsValid(ref errorMessage))
		{
			GClass1465.Log(errorMessage, hand);
			return;
		}
		Solver = new IKSolverLimb();
		Solver.IKPositionWeight = weight;
		Solver.bendModifier = IKSolverLimb.BendModifier.Target;
		Solver.bendModifierWeight = 1f;
		IKPosition = tip.position;
		IKRotation = tip.rotation;
		if (bone3 != null)
		{
			Bone3RelativeToTarget = Quaternion.Inverse(IKRotation) * bone3.rotation;
			Bone3DefaultLocalPosition = bone3.localPosition;
			Bone3DefaultLocalRotation = bone3.localRotation;
		}
		Solver.SetChain(bone1, bone2, tip, hand);
		Solver.Initiate(hand);
		initiated = true;
	}

	public void FixTransforms()
	{
		if (initiated)
		{
			Solver.FixTransforms();
			if (bone3 != null)
			{
				bone3.localPosition = Bone3DefaultLocalPosition;
				bone3.localRotation = Bone3DefaultLocalRotation;
			}
		}
	}

	public void Update(float masterWeight)
	{
		if (!initiated)
		{
			return;
		}
		float num = weight * masterWeight;
		if (num <= 0f)
		{
			return;
		}
		Solver.target = target;
		if (target != null)
		{
			IKPosition = target.position;
			IKRotation = target.rotation;
		}
		if (bone3 != null)
		{
			if (num >= 1f)
			{
				bone3.rotation = IKRotation * Bone3RelativeToTarget;
			}
			else
			{
				bone3.rotation = Quaternion.Lerp(bone3.rotation, IKRotation * Bone3RelativeToTarget, num);
			}
		}
		Solver.IKPositionWeight = num;
		Solver.Update();
	}
}
