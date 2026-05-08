using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKEffector
{
	public Transform bone;

	public Transform target;

	[Range(0f, 1f)]
	public float positionWeight;

	[Range(0f, 1f)]
	public float rotationWeight;

	public Vector3 position = Vector3.zero;

	public Quaternion rotation = Quaternion.identity;

	public Vector3 positionOffset;

	public bool effectChildNodes = true;

	[Range(0f, 1f)]
	public float maintainRelativePositionWeight;

	public Transform[] childBones = new Transform[0];

	public Transform planeBone1;

	public Transform planeBone2;

	public Transform planeBone3;

	public Quaternion planeRotationOffset = Quaternion.identity;

	[NonSerialized]
	public float PosW;

	[NonSerialized]
	public float RotW;

	[NonSerialized]
	public Vector3[] LocalPositions = new Vector3[0];

	[NonSerialized]
	public bool UsePlaneNodes;

	[NonSerialized]
	public Quaternion AnimatedPlaneRotation = Quaternion.identity;

	[NonSerialized]
	public Vector3 AnimatedPosition;

	[NonSerialized]
	public bool FirstUpdate;

	[NonSerialized]
	public int ChainIndex = -1;

	[NonSerialized]
	public int NodeIndex = -1;

	[NonSerialized]
	public int Plane1ChainIndex;

	[NonSerialized]
	public int Plane1NodeIndex = -1;

	[NonSerialized]
	public int Plane2ChainIndex = -1;

	[NonSerialized]
	public int Plane2NodeIndex = -1;

	[NonSerialized]
	public int Plane3ChainIndex = -1;

	[NonSerialized]
	public int Plane3NodeIndex = -1;

	[NonSerialized]
	public int[] ChildChainIndexes = new int[0];

	[NonSerialized]
	public int[] ChildNodeIndexes = new int[0];

	[field: NonSerialized]
	public bool isEndEffector { get; set; }

	public IKSolver.Node GetNode(IKSolverFullBody solver)
	{
		return solver.chain[ChainIndex].nodes[NodeIndex];
	}

	public void PinToBone(float positionWeight, float rotationWeight)
	{
		position = bone.position;
		this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotation = bone.rotation;
		this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
	}

	public IKEffector()
	{
	}

	public IKEffector(Transform bone, Transform[] childBones)
	{
		this.bone = bone;
		this.childBones = childBones;
	}

	public bool IsValid(IKSolver solver, ref string message)
	{
		if (bone == null)
		{
			message = "IK Effector bone is null.";
			return false;
		}
		if (solver.GetPoint(bone) == null)
		{
			message = "IK Effector is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.";
			return false;
		}
		Transform[] array = childBones;
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
			array = childBones;
			num = 0;
			Transform transform;
			while (true)
			{
				if (num < array.Length)
				{
					transform = array[num];
					if (solver.GetPoint(transform) == null)
					{
						break;
					}
					num++;
					continue;
				}
				if (planeBone1 != null && solver.GetPoint(planeBone1) == null)
				{
					message = "IK Effector is referencing to a bone '" + planeBone1.name + "' that does not excist in the Node Chain.";
					return false;
				}
				if (planeBone2 != null && solver.GetPoint(planeBone2) == null)
				{
					message = "IK Effector is referencing to a bone '" + planeBone2.name + "' that does not excist in the Node Chain.";
					return false;
				}
				if (planeBone3 != null && solver.GetPoint(planeBone3) == null)
				{
					message = "IK Effector is referencing to a bone '" + planeBone3.name + "' that does not excist in the Node Chain.";
					return false;
				}
				return true;
			}
			message = "IK Effector is referencing to a bone '" + transform.name + "' that does not excist in the Node Chain.";
			return false;
		}
		message = "IK Effector contains a null reference.";
		return false;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		position = bone.position;
		rotation = bone.rotation;
		AnimatedPlaneRotation = Quaternion.identity;
		solver.GetChainAndNodeIndexes(bone, out ChainIndex, out NodeIndex);
		ChildChainIndexes = new int[childBones.Length];
		ChildNodeIndexes = new int[childBones.Length];
		for (int i = 0; i < childBones.Length; i++)
		{
			solver.GetChainAndNodeIndexes(childBones[i], out ChildChainIndexes[i], out ChildNodeIndexes[i]);
		}
		LocalPositions = new Vector3[childBones.Length];
		UsePlaneNodes = false;
		if (planeBone1 != null)
		{
			solver.GetChainAndNodeIndexes(planeBone1, out Plane1ChainIndex, out Plane1NodeIndex);
			if (planeBone2 != null)
			{
				solver.GetChainAndNodeIndexes(planeBone2, out Plane2ChainIndex, out Plane2NodeIndex);
				if (planeBone3 != null)
				{
					solver.GetChainAndNodeIndexes(planeBone3, out Plane3ChainIndex, out Plane3NodeIndex);
					UsePlaneNodes = true;
				}
			}
			isEndEffector = true;
		}
		else
		{
			isEndEffector = false;
		}
	}

	public void ResetOffset(IKSolverFullBody solver)
	{
		solver.GetNode(ChainIndex, NodeIndex).offset = Vector3.zero;
		for (int i = 0; i < ChildChainIndexes.Length; i++)
		{
			solver.GetNode(ChildChainIndexes[i], ChildNodeIndexes[i]).offset = Vector3.zero;
		}
	}

	public void SetToTarget()
	{
		if (!(target == null))
		{
			position = target.position;
			rotation = target.rotation;
		}
	}

	public void OnPreSolve(IKSolverFullBody solver)
	{
		positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
		rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);
		PosW = positionWeight * solver.IKPositionWeight;
		RotW = rotationWeight * solver.IKPositionWeight;
		solver.GetNode(ChainIndex, NodeIndex).effectorPositionWeight = PosW;
		solver.GetNode(ChainIndex, NodeIndex).effectorRotationWeight = RotW;
		solver.GetNode(ChainIndex, NodeIndex).solverRotation = rotation;
		if (float.IsInfinity(positionOffset.x) || float.IsInfinity(positionOffset.y) || float.IsInfinity(positionOffset.z))
		{
			Debug.LogError("Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", bone);
		}
		if (float.IsNaN(positionOffset.x) || float.IsNaN(positionOffset.y) || float.IsNaN(positionOffset.z))
		{
			Debug.LogError("Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", bone);
		}
		if (positionOffset.sqrMagnitude > 1E+10f)
		{
			Debug.LogError("Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", bone);
		}
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			Debug.LogError("Invalid IKEffector.position (contains Infinity)!");
		}
		solver.GetNode(ChainIndex, NodeIndex).offset += positionOffset * solver.IKPositionWeight;
		if (effectChildNodes && solver.iterations > 0)
		{
			for (int i = 0; i < childBones.Length; i++)
			{
				LocalPositions[i] = childBones[i].transform.position - bone.transform.position;
				solver.GetNode(ChildChainIndexes[i], ChildNodeIndexes[i]).offset += positionOffset * solver.IKPositionWeight;
			}
		}
		if (UsePlaneNodes && maintainRelativePositionWeight > 0f)
		{
			AnimatedPlaneRotation = Quaternion.LookRotation(planeBone2.position - planeBone1.position, planeBone3.position - planeBone1.position);
		}
		FirstUpdate = true;
	}

	public void OnPostWrite()
	{
		positionOffset = Vector3.zero;
	}

	public Quaternion method_0(IKSolverFullBody solver)
	{
		Vector3 solverPosition = solver.GetNode(Plane1ChainIndex, Plane1NodeIndex).solverPosition;
		Vector3 solverPosition2 = solver.GetNode(Plane2ChainIndex, Plane2NodeIndex).solverPosition;
		Vector3 solverPosition3 = solver.GetNode(Plane3ChainIndex, Plane3NodeIndex).solverPosition;
		Vector3 vector = solverPosition2 - solverPosition;
		Vector3 upwards = solverPosition3 - solverPosition;
		if (vector == Vector3.zero)
		{
			GClass1465.Log("Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.", bone);
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(vector, upwards);
	}

	public void Update(IKSolverFullBody solver)
	{
		if (FirstUpdate)
		{
			AnimatedPosition = bone.position + solver.GetNode(ChainIndex, NodeIndex).offset;
			FirstUpdate = false;
		}
		solver.GetNode(ChainIndex, NodeIndex).solverPosition = Vector3.Lerp(method_1(solver, out planeRotationOffset), position, PosW);
		if (effectChildNodes)
		{
			for (int i = 0; i < childBones.Length; i++)
			{
				solver.GetNode(ChildChainIndexes[i], ChildNodeIndexes[i]).solverPosition = Vector3.Lerp(solver.GetNode(ChildChainIndexes[i], ChildNodeIndexes[i]).solverPosition, solver.GetNode(ChainIndex, NodeIndex).solverPosition + LocalPositions[i], PosW);
			}
		}
	}

	public Vector3 method_1(IKSolverFullBody solver, out Quaternion planeRotationOffset)
	{
		planeRotationOffset = Quaternion.identity;
		if (!isEndEffector)
		{
			return solver.GetNode(ChainIndex, NodeIndex).solverPosition;
		}
		if (maintainRelativePositionWeight <= 0f)
		{
			return AnimatedPosition;
		}
		Vector3 vector = bone.position;
		Vector3 vector2 = vector - planeBone1.position;
		planeRotationOffset = method_0(solver) * Quaternion.Inverse(AnimatedPlaneRotation);
		vector = solver.GetNode(Plane1ChainIndex, Plane1NodeIndex).solverPosition + planeRotationOffset * vector2;
		planeRotationOffset = Quaternion.Lerp(Quaternion.identity, planeRotationOffset, maintainRelativePositionWeight);
		return Vector3.Lerp(AnimatedPosition, vector + solver.GetNode(ChainIndex, NodeIndex).offset, maintainRelativePositionWeight);
	}
}
