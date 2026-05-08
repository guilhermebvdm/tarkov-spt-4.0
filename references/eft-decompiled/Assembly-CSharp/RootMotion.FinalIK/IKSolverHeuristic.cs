using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverHeuristic : IKSolver
{
	public Transform target;

	public float tolerance;

	public int maxIterations = 4;

	public bool useRotationLimits = true;

	public bool XY;

	public Bone[] bones = new Bone[0];

	[NonSerialized]
	public Vector3 LastLocalDirection;

	[NonSerialized]
	public float ChainLength;

	public virtual int minBones => 2;

	public virtual bool boneLengthCanBeZero => true;

	public virtual bool allowCommonParent => false;

	public virtual Vector3 localDirection => bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.position - bones[0].transform.position);

	public float positionOffset => Vector3.SqrMagnitude(localDirection - LastLocalDirection);

	public bool SetChain(Transform[] hierarchy, Transform root)
	{
		if (bones == null || bones.Length != hierarchy.Length)
		{
			bones = new Bone[hierarchy.Length];
		}
		for (int i = 0; i < hierarchy.Length; i++)
		{
			if (bones[i] == null)
			{
				bones[i] = new Bone();
			}
			bones[i].transform = hierarchy[i];
		}
		Initiate(root);
		return base.initiated;
	}

	public void AddBone(Transform bone)
	{
		Transform[] array = new Transform[bones.Length + 1];
		for (int i = 0; i < bones.Length; i++)
		{
			array[i] = bones[i].transform;
		}
		array[^1] = bone;
		SetChain(array, root);
	}

	public override void StoreDefaultLocalState()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		if (!(IKPositionWeight <= 0f))
		{
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].FixTransform();
			}
		}
	}

	public override bool IsValid(ref string message)
	{
		if (bones.Length == 0)
		{
			message = "IK chain has no Bones.";
			return false;
		}
		if (bones.Length < minBones)
		{
			message = "IK chain has less than " + minBones + " Bones.";
			return false;
		}
		Bone[] array = bones;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (array[num].transform == null)
				{
					break;
				}
				num++;
				continue;
			}
			Transform transform = IKSolver.ContainsDuplicateBone(bones);
			if (transform != null)
			{
				message = transform.name + " is represented multiple times in the Bones.";
				return false;
			}
			if (!allowCommonParent && !IKSolver.HierarchyIsValid(bones))
			{
				message = "Invalid bone hierarchy detected. IK requires for it's bones to be parented to each other in descending order.";
				return false;
			}
			if (!boneLengthCanBeZero)
			{
				for (int i = 0; i < bones.Length - 1; i++)
				{
					if ((bones[i].transform.position - bones[i + 1].transform.position).magnitude == 0f)
					{
						message = "Bone " + i + " length is zero.";
						return false;
					}
				}
			}
			return true;
		}
		message = "One of the Bones is null.";
		return false;
	}

	public override Point[] GetPoints()
	{
		return bones;
	}

	public override Point GetPoint(Transform transform)
	{
		int num = 0;
		while (true)
		{
			if (num < bones.Length)
			{
				if (bones[num].transform == transform)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return bones[num];
	}

	public override void OnInitiate()
	{
	}

	public override void OnUpdate()
	{
	}

	public void InitiateBones()
	{
		ChainLength = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			if (i < bones.Length - 1)
			{
				bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
				ChainLength += bones[i].length;
				Vector3 position = bones[i + 1].transform.position;
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (position - bones[i].transform.position);
				if (bones[i].rotationLimit != null)
				{
					if (XY && !(bones[i].rotationLimit is RotationLimitHinge))
					{
						GClass1465.Log("Only Hinge Rotation Limits should be used on 2D IK solvers.", bones[i].transform);
					}
					bones[i].rotationLimit.Disable();
				}
			}
			else
			{
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[bones.Length - 1].transform.position - bones[0].transform.position);
			}
		}
	}

	public Vector3 GetSingularityOffset()
	{
		if (!method_0())
		{
			return Vector3.zero;
		}
		Vector3 normalized = (IKPosition - bones[0].transform.position).normalized;
		Vector3 rhs = new Vector3(normalized.y, normalized.z, normalized.x);
		if (useRotationLimits && bones[bones.Length - 2].rotationLimit != null && bones[bones.Length - 2].rotationLimit is RotationLimitHinge)
		{
			rhs = bones[bones.Length - 2].transform.rotation * bones[bones.Length - 2].rotationLimit.axis;
		}
		return Vector3.Cross(normalized, rhs) * bones[bones.Length - 2].length * 0.5f;
	}

	public bool method_0()
	{
		if (!base.initiated)
		{
			return false;
		}
		Vector3 vector = bones[bones.Length - 1].transform.position - bones[0].transform.position;
		Vector3 vector2 = IKPosition - bones[0].transform.position;
		float magnitude = vector.magnitude;
		float magnitude2 = vector2.magnitude;
		if (magnitude < magnitude2)
		{
			return false;
		}
		if (magnitude < ChainLength - bones[bones.Length - 2].length * 0.1f)
		{
			return false;
		}
		if (magnitude == 0f)
		{
			return false;
		}
		if (magnitude2 == 0f)
		{
			return false;
		}
		if (magnitude2 > magnitude)
		{
			return false;
		}
		if (Vector3.Dot(vector / magnitude, vector2 / magnitude2) < 0.999f)
		{
			return false;
		}
		return true;
	}
}
