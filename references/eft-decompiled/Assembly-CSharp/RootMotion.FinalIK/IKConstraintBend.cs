using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKConstraintBend
{
	public Transform bone1;

	public Transform bone2;

	public Transform bone3;

	public Transform bendGoal;

	public Vector3 direction = Vector3.right;

	public Quaternion rotationOffset;

	[Range(0f, 1f)]
	public float weight;

	public Vector3 defaultLocalDirection;

	public Vector3 defaultChildDirection;

	[NonSerialized]
	public float clampF = 0.505f;

	[NonSerialized]
	public int ChainIndex1;

	[NonSerialized]
	public int NodeIndex1;

	[NonSerialized]
	public int ChainIndex2;

	[NonSerialized]
	public int NodeIndex2;

	[NonSerialized]
	public int ChainIndex3;

	[NonSerialized]
	public int NodeIndex3;

	[field: NonSerialized]
	public bool initiated { get; set; }

	public bool IsValid(IKSolverFullBody solver, GClass1465.GDelegate45 logger)
	{
		if (!(bone1 == null) && !(bone2 == null) && !(bone3 == null))
		{
			if (solver.GetPoint(bone1) == null)
			{
				logger?.Invoke("Bend Constraint is referencing to a bone '" + bone1.name + "' that does not excist in the Node Chain.");
				return false;
			}
			if (solver.GetPoint(bone2) == null)
			{
				logger?.Invoke("Bend Constraint is referencing to a bone '" + bone2.name + "' that does not excist in the Node Chain.");
				return false;
			}
			if (solver.GetPoint(bone3) == null)
			{
				logger?.Invoke("Bend Constraint is referencing to a bone '" + bone3.name + "' that does not excist in the Node Chain.");
				return false;
			}
			return true;
		}
		logger?.Invoke("Bend Constraint contains a null reference.");
		return false;
	}

	public IKConstraintBend()
	{
	}

	public IKConstraintBend(Transform bone1, Transform bone2, Transform bone3)
	{
		SetBones(bone1, bone2, bone3);
	}

	public void SetBones(Transform bone1, Transform bone2, Transform bone3)
	{
		this.bone1 = bone1;
		this.bone2 = bone2;
		this.bone3 = bone3;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		solver.GetChainAndNodeIndexes(bone1, out ChainIndex1, out NodeIndex1);
		solver.GetChainAndNodeIndexes(bone2, out ChainIndex2, out NodeIndex2);
		solver.GetChainAndNodeIndexes(bone3, out ChainIndex3, out NodeIndex3);
		direction = method_1(solver, method_0(solver, bone2.position - bone1.position));
		defaultLocalDirection = Quaternion.Inverse(bone1.rotation) * direction;
		Vector3 vector = Vector3.Cross((bone3.position - bone1.position).normalized, direction);
		defaultChildDirection = Quaternion.Inverse(bone3.rotation) * vector;
		initiated = true;
	}

	public void SetLimbOrientation(Vector3 upper, Vector3 lower, Vector3 last)
	{
		if (upper == Vector3.zero)
		{
			Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
		}
		if (lower == Vector3.zero)
		{
			Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
		}
		if (last == Vector3.zero)
		{
			Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
		}
		defaultLocalDirection = upper.normalized;
		defaultChildDirection = last.normalized;
	}

	public void LimitBend(float solverWeight, float positionWeight)
	{
		if (initiated)
		{
			Vector3 vector = bone1.rotation * -defaultLocalDirection;
			Vector3 fromDirection = bone3.position - bone2.position;
			bool changed = false;
			Vector3 toDirection = GClass1464.ClampDirection(fromDirection, vector, clampF * solverWeight, 0, out changed);
			Quaternion rotation = bone3.rotation;
			if (changed)
			{
				Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
				bone2.rotation = quaternion * bone2.rotation;
			}
			if (positionWeight > 0f)
			{
				Vector3 normal = bone2.position - bone1.position;
				Vector3 tangent = bone3.position - bone2.position;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Quaternion quaternion2 = Quaternion.FromToRotation(tangent, vector);
				bone2.rotation = Quaternion.Lerp(bone2.rotation, quaternion2 * bone2.rotation, positionWeight * solverWeight);
			}
			if (changed || positionWeight > 0f)
			{
				bone3.rotation = rotation;
			}
		}
	}

	public Vector3 GetDir(IKSolverFullBody solver)
	{
		if (!initiated)
		{
			return Vector3.zero;
		}
		float num = weight * solver.IKPositionWeight;
		if (bendGoal != null)
		{
			Vector3 vector = bendGoal.position - solver.GetNode(ChainIndex1, NodeIndex1).solverPosition;
			if (vector != Vector3.zero)
			{
				direction = vector;
			}
		}
		if (num >= 1f)
		{
			return direction.normalized;
		}
		Vector3 vector2 = solver.GetNode(ChainIndex3, NodeIndex3).solverPosition - solver.GetNode(ChainIndex1, NodeIndex1).solverPosition;
		Vector3 vector3 = Quaternion.FromToRotation(bone3.position - bone1.position, vector2) * (bone2.position - bone1.position);
		if (solver.GetNode(ChainIndex3, NodeIndex3).effectorRotationWeight > 0f)
		{
			Vector3 b = -Vector3.Cross(vector2, solver.GetNode(ChainIndex3, NodeIndex3).solverRotation * defaultChildDirection);
			vector3 = Vector3.Lerp(vector3, b, solver.GetNode(ChainIndex3, NodeIndex3).effectorRotationWeight);
		}
		if (rotationOffset != Quaternion.identity)
		{
			vector3 = Quaternion.FromToRotation(rotationOffset * vector2, vector2) * rotationOffset * vector3;
		}
		if (num <= 0f)
		{
			return vector3;
		}
		return Vector3.Lerp(vector3, direction.normalized, num);
	}

	public Vector3 method_0(IKSolverFullBody solver, Vector3 tangent)
	{
		Vector3 normal = solver.GetNode(ChainIndex3, NodeIndex3).solverPosition - solver.GetNode(ChainIndex1, NodeIndex1).solverPosition;
		Vector3.OrthoNormalize(ref normal, ref tangent);
		return tangent;
	}

	public Vector3 method_1(IKSolverFullBody solver, Vector3 tangent)
	{
		Vector3 normal = solver.GetNode(ChainIndex2, NodeIndex2).solverPosition - solver.GetNode(ChainIndex1, NodeIndex1).solverPosition;
		Vector3.OrthoNormalize(ref normal, ref tangent);
		return tangent;
	}
}
