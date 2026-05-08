using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMappingLimb : IKMapping
{
	[Serializable]
	public enum BoneMapType
	{
		Parent,
		Bone1,
		Bone2,
		Bone3
	}

	public Transform parentBone;

	public Transform bone1;

	public Transform bone2;

	public Transform bone3;

	[Range(0f, 1f)]
	public float maintainRotationWeight;

	[Range(0f, 1f)]
	public float weight = 1f;

	[NonSerialized]
	public BoneMap BoneMapParent = new BoneMap();

	[NonSerialized]
	public BoneMap BoneMap1 = new BoneMap();

	[NonSerialized]
	public BoneMap BoneMap2 = new BoneMap();

	[NonSerialized]
	public BoneMap BoneMap3 = new BoneMap();

	public override bool IsValid(IKSolver solver, ref string message)
	{
		if (!base.IsValid(solver, ref message))
		{
			return false;
		}
		if (!BoneIsValid(bone1, solver, ref message))
		{
			return false;
		}
		if (!BoneIsValid(bone2, solver, ref message))
		{
			return false;
		}
		if (!BoneIsValid(bone3, solver, ref message))
		{
			return false;
		}
		return true;
	}

	public BoneMap GetBoneMap(BoneMapType boneMap)
	{
		switch (boneMap)
		{
		default:
			return BoneMap3;
		case BoneMapType.Parent:
			if (parentBone == null)
			{
				GClass1465.Log("This limb does not have a parent (shoulder) bone", bone1);
			}
			return BoneMapParent;
		case BoneMapType.Bone1:
			return BoneMap1;
		case BoneMapType.Bone2:
			return BoneMap2;
		}
	}

	public void SetLimbOrientation(Vector3 upper, Vector3 lower)
	{
		BoneMap1.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone1.rotation) * Quaternion.LookRotation(bone2.position - bone1.position, bone1.rotation * -upper));
		BoneMap2.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone2.rotation) * Quaternion.LookRotation(bone3.position - bone2.position, bone2.rotation * -lower));
	}

	public IKMappingLimb()
	{
	}

	public IKMappingLimb(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
	{
		SetBones(bone1, bone2, bone3, parentBone);
	}

	public void SetBones(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
	{
		this.bone1 = bone1;
		this.bone2 = bone2;
		this.bone3 = bone3;
		this.parentBone = parentBone;
	}

	public void StoreDefaultLocalState()
	{
		if (parentBone != null)
		{
			BoneMapParent.StoreDefaultLocalState();
		}
		BoneMap1.StoreDefaultLocalState();
		BoneMap2.StoreDefaultLocalState();
		BoneMap3.StoreDefaultLocalState();
	}

	public void FixTransforms()
	{
		if (parentBone != null)
		{
			BoneMapParent.FixTransform(position: false);
		}
		BoneMap1.FixTransform(position: true);
		BoneMap2.FixTransform(position: false);
		BoneMap3.FixTransform(position: false);
	}

	public override void Initiate(IKSolverFullBody solver)
	{
		if (BoneMapParent == null)
		{
			BoneMapParent = new BoneMap();
		}
		if (BoneMap1 == null)
		{
			BoneMap1 = new BoneMap();
		}
		if (BoneMap2 == null)
		{
			BoneMap2 = new BoneMap();
		}
		if (BoneMap3 == null)
		{
			BoneMap3 = new BoneMap();
		}
		if (parentBone != null)
		{
			BoneMapParent.Initiate(parentBone, solver);
		}
		BoneMap1.Initiate(bone1, solver);
		BoneMap2.Initiate(bone2, solver);
		BoneMap3.Initiate(bone3, solver);
		BoneMap1.SetPlane(solver, BoneMap1.transform, BoneMap2.transform, BoneMap3.transform);
		BoneMap2.SetPlane(solver, BoneMap2.transform, BoneMap3.transform, BoneMap1.transform);
		if (parentBone != null)
		{
			BoneMapParent.SetLocalSwingAxis(BoneMap1);
		}
	}

	public void ReadPose()
	{
		BoneMap1.UpdatePlane(rotation: true, position: true);
		BoneMap2.UpdatePlane(rotation: true, position: false);
		weight = Mathf.Clamp(weight, 0f, 1f);
		BoneMap3.MaintainRotation();
	}

	public void WritePose(IKSolverFullBody solver, bool fullBody)
	{
		if (!(weight <= 0f))
		{
			if (fullBody && parentBone != null)
			{
				BoneMapParent.Swing(solver.GetNode(BoneMap1.chainIndex, BoneMap1.nodeIndex).solverPosition, weight);
			}
			BoneMap1.RotateToPlane(solver, weight);
			BoneMap2.RotateToPlane(solver, weight);
			BoneMap3.RotateToMaintain(maintainRotationWeight * weight * solver.IKPositionWeight);
			BoneMap3.RotateToEffector(solver, weight);
		}
	}
}
