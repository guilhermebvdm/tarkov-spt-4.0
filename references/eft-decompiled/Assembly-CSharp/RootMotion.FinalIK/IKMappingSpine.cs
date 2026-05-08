using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMappingSpine : IKMapping
{
	public Transform[] spineBones;

	public Transform leftUpperArmBone;

	public Transform rightUpperArmBone;

	public Transform leftThighBone;

	public Transform rightThighBone;

	[Range(1f, 3f)]
	public int iterations = 3;

	[Range(0f, 1f)]
	public float twistWeight = 1f;

	[NonSerialized]
	public int RootNodeIndex;

	[NonSerialized]
	public BoneMap[] Spine = new BoneMap[0];

	[NonSerialized]
	public BoneMap LeftUpperArm = new BoneMap();

	[NonSerialized]
	public BoneMap RightUpperArm = new BoneMap();

	[NonSerialized]
	public BoneMap LeftThigh = new BoneMap();

	[NonSerialized]
	public BoneMap RightThigh = new BoneMap();

	[NonSerialized]
	public bool UseFABRIK;

	public override bool IsValid(IKSolver solver, ref string message)
	{
		if (!base.IsValid(solver, ref message))
		{
			return false;
		}
		Transform[] array = spineBones;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (array[num] == null)
				{
					break;
				}
				num++;
				continue;
			}
			int num2 = 0;
			for (int i = 0; i < spineBones.Length; i++)
			{
				if (solver.GetPoint(spineBones[i]) != null)
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				message = "IKMappingSpine does not contain any nodes.";
				return false;
			}
			if (leftUpperArmBone == null)
			{
				message = "IKMappingSpine is missing the left upper arm bone.";
				return false;
			}
			if (rightUpperArmBone == null)
			{
				message = "IKMappingSpine is missing the right upper arm bone.";
				return false;
			}
			if (leftThighBone == null)
			{
				message = "IKMappingSpine is missing the left thigh bone.";
				return false;
			}
			if (rightThighBone == null)
			{
				message = "IKMappingSpine is missing the right thigh bone.";
				return false;
			}
			if (solver.GetPoint(leftUpperArmBone) == null)
			{
				message = "Full Body IK is missing the left upper arm node.";
				return false;
			}
			if (solver.GetPoint(rightUpperArmBone) == null)
			{
				message = "Full Body IK is missing the right upper arm node.";
				return false;
			}
			if (solver.GetPoint(leftThighBone) == null)
			{
				message = "Full Body IK is missing the left thigh node.";
				return false;
			}
			if (solver.GetPoint(rightThighBone) == null)
			{
				message = "Full Body IK is missing the right thigh node.";
				return false;
			}
			return true;
		}
		message = "Spine bones contains a null reference.";
		return false;
	}

	public IKMappingSpine()
	{
	}

	public IKMappingSpine(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
	{
		SetBones(spineBones, leftUpperArmBone, rightUpperArmBone, leftThighBone, rightThighBone);
	}

	public void SetBones(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone)
	{
		this.spineBones = spineBones;
		this.leftUpperArmBone = leftUpperArmBone;
		this.rightUpperArmBone = rightUpperArmBone;
		this.leftThighBone = leftThighBone;
		this.rightThighBone = rightThighBone;
	}

	public void StoreDefaultLocalState()
	{
		for (int i = 0; i < Spine.Length; i++)
		{
			Spine[i].StoreDefaultLocalState();
		}
	}

	public void FixTransforms()
	{
		for (int i = 0; i < Spine.Length; i++)
		{
			Spine[i].FixTransform(i == 0 || i == Spine.Length - 1);
		}
	}

	public override void Initiate(IKSolverFullBody solver)
	{
		if (iterations <= 0)
		{
			iterations = 3;
		}
		if (Spine == null || Spine.Length != spineBones.Length)
		{
			Spine = new BoneMap[spineBones.Length];
		}
		RootNodeIndex = -1;
		for (int i = 0; i < spineBones.Length; i++)
		{
			if (Spine[i] == null)
			{
				Spine[i] = new BoneMap();
			}
			Spine[i].Initiate(spineBones[i], solver);
			if (Spine[i].isNodeBone)
			{
				RootNodeIndex = i;
			}
		}
		if (LeftUpperArm == null)
		{
			LeftUpperArm = new BoneMap();
		}
		if (RightUpperArm == null)
		{
			RightUpperArm = new BoneMap();
		}
		if (LeftThigh == null)
		{
			LeftThigh = new BoneMap();
		}
		if (RightThigh == null)
		{
			RightThigh = new BoneMap();
		}
		LeftUpperArm.Initiate(leftUpperArmBone, solver);
		RightUpperArm.Initiate(rightUpperArmBone, solver);
		LeftThigh.Initiate(leftThighBone, solver);
		RightThigh.Initiate(rightThighBone, solver);
		for (int j = 0; j < Spine.Length; j++)
		{
			Spine[j].SetIKPosition();
		}
		Spine[0].SetPlane(solver, Spine[RootNodeIndex].transform, LeftThigh.transform, RightThigh.transform);
		for (int k = 0; k < Spine.Length - 1; k++)
		{
			Spine[k].SetLength(Spine[k + 1]);
			Spine[k].SetLocalSwingAxis(Spine[k + 1]);
			Spine[k].SetLocalTwistAxis(LeftUpperArm.transform.position - RightUpperArm.transform.position, Spine[k + 1].transform.position - Spine[k].transform.position);
		}
		Spine[Spine.Length - 1].SetPlane(solver, Spine[RootNodeIndex].transform, LeftUpperArm.transform, RightUpperArm.transform);
		Spine[Spine.Length - 1].SetLocalSwingAxis(LeftUpperArm, RightUpperArm);
		UseFABRIK = method_0();
	}

	public bool method_0()
	{
		if (Spine.Length > 3)
		{
			return true;
		}
		if (RootNodeIndex != 1)
		{
			return true;
		}
		return false;
	}

	public void ReadPose()
	{
		Spine[0].UpdatePlane(rotation: true, position: true);
		for (int i = 0; i < Spine.Length - 1; i++)
		{
			Spine[i].SetLength(Spine[i + 1]);
			Spine[i].SetLocalSwingAxis(Spine[i + 1]);
			Spine[i].SetLocalTwistAxis(LeftUpperArm.transform.position - RightUpperArm.transform.position, Spine[i + 1].transform.position - Spine[i].transform.position);
		}
		Spine[Spine.Length - 1].UpdatePlane(rotation: true, position: true);
		Spine[Spine.Length - 1].SetLocalSwingAxis(LeftUpperArm, RightUpperArm);
	}

	public void WritePose(IKSolverFullBody solver)
	{
		Vector3 planePosition = Spine[0].GetPlanePosition(solver);
		Vector3 solverPosition = solver.GetNode(Spine[RootNodeIndex].chainIndex, Spine[RootNodeIndex].nodeIndex).solverPosition;
		Vector3 planePosition2 = Spine[Spine.Length - 1].GetPlanePosition(solver);
		if (UseFABRIK)
		{
			Vector3 vector = solver.GetNode(Spine[RootNodeIndex].chainIndex, Spine[RootNodeIndex].nodeIndex).solverPosition - Spine[RootNodeIndex].transform.position;
			for (int i = 0; i < Spine.Length; i++)
			{
				Spine[i].ikPosition = Spine[i].transform.position + vector;
			}
			for (int j = 0; j < iterations; j++)
			{
				ForwardReach(planePosition2);
				method_1(planePosition);
				Spine[RootNodeIndex].ikPosition = solverPosition;
			}
		}
		else
		{
			Spine[0].ikPosition = planePosition;
			Spine[RootNodeIndex].ikPosition = solverPosition;
		}
		Spine[Spine.Length - 1].ikPosition = planePosition2;
		method_2(solver);
	}

	public void ForwardReach(Vector3 position)
	{
		Spine[spineBones.Length - 1].ikPosition = position;
		for (int num = Spine.Length - 2; num > -1; num--)
		{
			Spine[num].ikPosition = SolveFABRIKJoint(Spine[num].ikPosition, Spine[num + 1].ikPosition, Spine[num].length);
		}
	}

	public void method_1(Vector3 position)
	{
		Spine[0].ikPosition = position;
		for (int i = 1; i < Spine.Length; i++)
		{
			Spine[i].ikPosition = SolveFABRIKJoint(Spine[i].ikPosition, Spine[i - 1].ikPosition, Spine[i - 1].length);
		}
	}

	public void method_2(IKSolverFullBody solver)
	{
		Spine[0].SetToIKPosition();
		Spine[0].RotateToPlane(solver, 1f);
		for (int i = 1; i < Spine.Length - 1; i++)
		{
			Spine[i].Swing(Spine[i + 1].ikPosition, 1f);
			if (twistWeight > 0f)
			{
				float num = (float)i / ((float)Spine.Length - 2f);
				Vector3 solverPosition = solver.GetNode(LeftUpperArm.chainIndex, LeftUpperArm.nodeIndex).solverPosition;
				Vector3 solverPosition2 = solver.GetNode(RightUpperArm.chainIndex, RightUpperArm.nodeIndex).solverPosition;
				Spine[i].Twist(solverPosition - solverPosition2, Spine[i + 1].ikPosition - Spine[i].transform.position, num * twistWeight);
			}
		}
		Spine[Spine.Length - 1].SetToIKPosition();
		Spine[Spine.Length - 1].RotateToPlane(solver, 1f);
	}
}
