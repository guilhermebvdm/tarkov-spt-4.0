using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMappingBone : IKMapping
{
	public Transform bone;

	[Range(0f, 1f)]
	public float maintainRotationWeight = 1f;

	[NonSerialized]
	public new BoneMap BoneMap = new BoneMap();

	public override bool IsValid(IKSolver solver, ref string message)
	{
		if (!base.IsValid(solver, ref message))
		{
			return false;
		}
		if (bone == null)
		{
			message = "IKMappingBone's bone is null.";
			return false;
		}
		return true;
	}

	public IKMappingBone()
	{
	}

	public IKMappingBone(Transform bone)
	{
		this.bone = bone;
	}

	public void StoreDefaultLocalState()
	{
		BoneMap.StoreDefaultLocalState();
	}

	public void FixTransforms()
	{
		BoneMap.FixTransform(position: false);
	}

	public override void Initiate(IKSolverFullBody solver)
	{
		if (BoneMap == null)
		{
			BoneMap = new BoneMap();
		}
		BoneMap.Initiate(bone, solver);
	}

	public void ReadPose()
	{
		BoneMap.MaintainRotation();
	}

	public void WritePose(float solverWeight)
	{
		BoneMap.RotateToMaintain(solverWeight * maintainRotationWeight);
	}
}
