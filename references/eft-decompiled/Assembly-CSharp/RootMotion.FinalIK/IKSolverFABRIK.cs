using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFABRIK : IKSolverHeuristic
{
	public GDelegate48 OnPreIteration;

	[NonSerialized]
	public bool[] LimitedBones = new bool[0];

	[NonSerialized]
	public Vector3[] SolverLocalPositions = new Vector3[0];

	public override bool boneLengthCanBeZero => false;

	public void SolveForward(Vector3 position)
	{
		if (!base.initiated)
		{
			if (!GClass1465.logged)
			{
				LogWarning("Trying to solve uninitiated FABRIK chain.");
			}
		}
		else
		{
			method_2();
			method_5(position);
		}
	}

	public void SolveBackward(Vector3 position)
	{
		if (!base.initiated)
		{
			if (!GClass1465.logged)
			{
				LogWarning("Trying to solve uninitiated FABRIK chain.");
			}
		}
		else
		{
			method_14(position);
			method_3();
		}
	}

	public override Vector3 GetIKPosition()
	{
		if (target != null)
		{
			return target.position;
		}
		return IKPosition;
	}

	public override void OnInitiate()
	{
		if (FirstInitiation || !Application.isPlaying)
		{
			IKPosition = bones[bones.Length - 1].transform.position;
		}
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].solverPosition = bones[i].transform.position;
			bones[i].solverRotation = bones[i].transform.rotation;
		}
		LimitedBones = new bool[bones.Length];
		SolverLocalPositions = new Vector3[bones.Length];
		InitiateBones();
		for (int j = 0; j < bones.Length; j++)
		{
			SolverLocalPositions[j] = Quaternion.Inverse(method_10(j)) * (bones[j].transform.position - method_11(j));
		}
	}

	public override void OnUpdate()
	{
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		method_2();
		if (target != null)
		{
			IKPosition = target.position;
		}
		if (XY)
		{
			IKPosition.z = bones[0].transform.position.z;
		}
		Vector3 vector = ((maxIterations > 1) ? GetSingularityOffset() : Vector3.zero);
		for (int i = 0; i < maxIterations && (!(vector == Vector3.zero) || i < 1 || !(tolerance > 0f) || !(base.positionOffset < tolerance * tolerance)); i++)
		{
			LastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			method_4(IKPosition + ((i == 0) ? vector : Vector3.zero));
		}
		method_3();
	}

	public Vector3 method_1(Vector3 pos1, Vector3 pos2, float length)
	{
		if (XY)
		{
			pos1.z = pos2.z;
		}
		return pos2 + (pos1 - pos2).normalized * length;
	}

	public void method_2()
	{
		ChainLength = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].solverPosition = bones[i].transform.position;
			bones[i].solverRotation = bones[i].transform.rotation;
			if (i < bones.Length - 1)
			{
				bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[i + 1].transform.position - bones[i].transform.position);
				ChainLength += bones[i].length;
			}
			if (useRotationLimits)
			{
				SolverLocalPositions[i] = Quaternion.Inverse(method_10(i)) * (bones[i].transform.position - method_11(i));
			}
		}
	}

	public void method_3()
	{
		if (!useRotationLimits)
		{
			method_17();
		}
		else
		{
			method_18();
		}
		LastLocalDirection = localDirection;
	}

	public void method_4(Vector3 targetPosition)
	{
		method_5(targetPosition);
		method_14(bones[0].transform.position);
	}

	public void method_5(Vector3 position)
	{
		bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, position, IKPositionWeight);
		for (int i = 0; i < LimitedBones.Length; i++)
		{
			LimitedBones[i] = false;
		}
		for (int num = bones.Length - 2; num > -1; num--)
		{
			bones[num].solverPosition = method_1(bones[num].solverPosition, bones[num + 1].solverPosition, bones[num].length);
			method_13(num, num + 1);
		}
		method_13(0, 0);
	}

	public void method_6(int index, Vector3 offset)
	{
		for (int i = index; i < bones.Length; i++)
		{
			bones[i].solverPosition += offset;
		}
	}

	public void method_7(int index, Quaternion rotation, bool recursive)
	{
		for (int i = index; i < bones.Length; i++)
		{
			bones[i].solverRotation = rotation * bones[i].solverRotation;
			if (!recursive)
			{
				break;
			}
		}
	}

	public void method_8(int index, Quaternion rotation)
	{
		for (int i = index + 1; i < bones.Length; i++)
		{
			bones[i].solverRotation = rotation * bones[i].solverRotation;
		}
	}

	public void method_9(int index, Quaternion rotation)
	{
		for (int i = index + 1; i < bones.Length; i++)
		{
			Vector3 vector = bones[i].solverPosition - bones[index].solverPosition;
			bones[i].solverPosition = bones[index].solverPosition + rotation * vector;
		}
	}

	public Quaternion method_10(int index)
	{
		if (index > 0)
		{
			return bones[index - 1].solverRotation;
		}
		if (bones[0].transform.parent == null)
		{
			return Quaternion.identity;
		}
		return bones[0].transform.parent.rotation;
	}

	public Vector3 method_11(int index)
	{
		if (index > 0)
		{
			return bones[index - 1].solverPosition;
		}
		if (bones[0].transform.parent == null)
		{
			return Vector3.zero;
		}
		return bones[0].transform.parent.position;
	}

	public Quaternion method_12(int index, Quaternion q, out bool changed)
	{
		changed = false;
		Quaternion quaternion = method_10(index);
		Quaternion localRotation = Quaternion.Inverse(quaternion) * q;
		Quaternion limitedLocalRotation = bones[index].rotationLimit.GetLimitedLocalRotation(localRotation, out changed);
		if (!changed)
		{
			return q;
		}
		return quaternion * limitedLocalRotation;
	}

	public void method_13(int rotateBone, int limitBone)
	{
		if (!useRotationLimits || bones[limitBone].rotationLimit == null)
		{
			return;
		}
		Vector3 solverPosition = bones[bones.Length - 1].solverPosition;
		for (int i = rotateBone; i < bones.Length - 1 && !LimitedBones[i]; i++)
		{
			Quaternion rotation = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, bones[i + 1].solverPosition - bones[i].solverPosition);
			method_7(i, rotation, recursive: false);
		}
		bool changed = false;
		Quaternion quaternion = method_12(limitBone, bones[limitBone].solverRotation, out changed);
		if (changed)
		{
			if (limitBone < bones.Length - 1)
			{
				Quaternion rotation2 = GClass1463.FromToRotation(bones[limitBone].solverRotation, quaternion);
				bones[limitBone].solverRotation = quaternion;
				method_8(limitBone, rotation2);
				method_9(limitBone, rotation2);
				Quaternion rotation3 = Quaternion.FromToRotation(bones[bones.Length - 1].solverPosition - bones[rotateBone].solverPosition, solverPosition - bones[rotateBone].solverPosition);
				method_7(rotateBone, rotation3, recursive: true);
				method_9(rotateBone, rotation3);
				method_6(rotateBone, solverPosition - bones[bones.Length - 1].solverPosition);
			}
			else
			{
				bones[limitBone].solverRotation = quaternion;
			}
		}
		LimitedBones[limitBone] = true;
	}

	public void method_14(Vector3 position)
	{
		if (useRotationLimits)
		{
			method_16(position);
		}
		else
		{
			method_15(position);
		}
	}

	public void method_15(Vector3 position)
	{
		bones[0].solverPosition = position;
		for (int i = 1; i < bones.Length; i++)
		{
			bones[i].solverPosition = method_1(bones[i].solverPosition, bones[i - 1].solverPosition, bones[i - 1].length);
		}
	}

	public void method_16(Vector3 position)
	{
		bones[0].solverPosition = position;
		for (int i = 0; i < bones.Length - 1; i++)
		{
			Vector3 vector = method_1(bones[i + 1].solverPosition, bones[i].solverPosition, bones[i].length);
			Quaternion quaternion = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, vector - bones[i].solverPosition) * bones[i].solverRotation;
			if (bones[i].rotationLimit != null)
			{
				bool changed = false;
				quaternion = method_12(i, quaternion, out changed);
			}
			Quaternion rotation = GClass1463.FromToRotation(bones[i].solverRotation, quaternion);
			bones[i].solverRotation = quaternion;
			method_8(i, rotation);
			bones[i + 1].solverPosition = bones[i].solverPosition + bones[i].solverRotation * SolverLocalPositions[i + 1];
		}
		for (int j = 0; j < bones.Length; j++)
		{
			bones[j].solverRotation = Quaternion.LookRotation(bones[j].solverRotation * Vector3.forward, bones[j].solverRotation * Vector3.up);
		}
	}

	public void method_17()
	{
		bones[0].transform.position = bones[0].solverPosition;
		for (int i = 0; i < bones.Length - 1; i++)
		{
			if (XY)
			{
				bones[i].Swing2D(bones[i + 1].solverPosition);
			}
			else
			{
				bones[i].Swing(bones[i + 1].solverPosition);
			}
		}
	}

	public void method_18()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].transform.position = bones[i].solverPosition;
			if (i < bones.Length - 1)
			{
				bones[i].transform.rotation = bones[i].solverRotation;
			}
		}
	}
}
