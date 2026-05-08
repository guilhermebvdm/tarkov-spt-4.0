using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLimb : IKSolverTrigonometric
{
	[Serializable]
	public enum BendModifier
	{
		Animation,
		Target,
		Parent,
		Arm,
		Goal,
		Custom
	}

	[Serializable]
	public struct AxisDirection(Vector3 direction, Vector3 axis)
	{
		public Vector3 direction = direction.normalized;

		public Vector3 axis = axis.normalized;

		public float dot = 0f;
	}

	public AvatarIKGoal goal;

	public BendModifier bendModifier;

	[Range(0f, 1f)]
	public float maintainRotationWeight;

	[Range(0f, 1f)]
	public float bendModifierWeight = 1f;

	public Transform bendGoal;

	[NonSerialized]
	public bool MaintainBendFor1Frame;

	[NonSerialized]
	public bool MaintainRotationFor1Frame;

	[NonSerialized]
	public Quaternion DefaultRootRotation;

	[NonSerialized]
	public Quaternion ParentDefaultRotation;

	[NonSerialized]
	public Quaternion Bone3RotationBeforeSolve;

	[NonSerialized]
	public Quaternion MaintainRotation;

	[NonSerialized]
	public Quaternion Bone3DefaultRotation;

	[NonSerialized]
	public Vector3 BendNormal;

	[NonSerialized]
	public Vector3 AnimationNormal;

	[NonSerialized]
	public AxisDirection[] AxisDirectionsLeft = new AxisDirection[4];

	[NonSerialized]
	public AxisDirection[] AxisDirectionsRight = new AxisDirection[4];

	public AxisDirection[] AxisDirection_0
	{
		get
		{
			if (goal == AvatarIKGoal.LeftHand)
			{
				return AxisDirectionsLeft;
			}
			return AxisDirectionsRight;
		}
	}

	public void MaintainRotation()
	{
		if (base.initiated)
		{
			this.MaintainRotation = bone3.transform.rotation;
			MaintainRotationFor1Frame = true;
		}
	}

	public void MaintainBend()
	{
		if (base.initiated)
		{
			AnimationNormal = bone1.GetBendNormalFromCurrentRotation();
			MaintainBendFor1Frame = true;
		}
	}

	public override void OnInitiateVirtual()
	{
		DefaultRootRotation = root.rotation;
		if (bone1.transform.parent != null)
		{
			ParentDefaultRotation = Quaternion.Inverse(DefaultRootRotation) * bone1.transform.parent.rotation;
		}
		if (bone3.rotationLimit != null)
		{
			bone3.rotationLimit.Disable();
		}
		Bone3DefaultRotation = bone3.transform.rotation;
		Vector3 vector = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
		if (vector != Vector3.zero)
		{
			bendNormal = vector;
		}
		AnimationNormal = bendNormal;
		method_2(ref AxisDirectionsLeft);
		method_2(ref AxisDirectionsRight);
	}

	public override void OnUpdateVirtual()
	{
		if (IKPositionWeight > 0f)
		{
			bendModifierWeight = Mathf.Clamp(bendModifierWeight, 0f, 1f);
			maintainRotationWeight = Mathf.Clamp(maintainRotationWeight, 0f, 1f);
			BendNormal = bendNormal;
			bendNormal = method_3();
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			Bone3RotationBeforeSolve = (MaintainRotationFor1Frame ? this.MaintainRotation : bone3.transform.rotation);
			MaintainRotationFor1Frame = false;
		}
	}

	public override void OnPostSolveVirtual()
	{
		if (IKPositionWeight > 0f)
		{
			bendNormal = BendNormal;
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, Bone3RotationBeforeSolve, maintainRotationWeight * IKPositionWeight);
		}
	}

	public IKSolverLimb()
	{
	}

	public IKSolverLimb(AvatarIKGoal goal)
	{
		this.goal = goal;
	}

	public void method_2(ref AxisDirection[] axisDirections)
	{
		axisDirections[0] = new AxisDirection(Vector3.zero, new Vector3(-1f, 0f, 0f));
		axisDirections[1] = new AxisDirection(new Vector3(0.5f, 0f, -0.2f), new Vector3(-0.5f, -1f, 1f));
		axisDirections[2] = new AxisDirection(new Vector3(-0.5f, -1f, -0.2f), new Vector3(0f, 0.5f, -1f));
		axisDirections[3] = new AxisDirection(new Vector3(-0.5f, -0.5f, 1f), new Vector3(-1f, -1f, -1f));
	}

	public Vector3 method_3()
	{
		float num = bendModifierWeight;
		if (num <= 0f)
		{
			return bendNormal;
		}
		switch (bendModifier)
		{
		default:
			return bendNormal;
		case BendModifier.Animation:
			if (!MaintainBendFor1Frame)
			{
				MaintainBend();
			}
			MaintainBendFor1Frame = false;
			return Vector3.Lerp(bendNormal, AnimationNormal, num);
		case BendModifier.Target:
		{
			Quaternion b = IKRotation * Quaternion.Inverse(Bone3DefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, b, num) * bendNormal;
		}
		case BendModifier.Parent:
		{
			if (bone1.transform.parent == null)
			{
				return bendNormal;
			}
			Quaternion quaternion = bone1.transform.parent.rotation * Quaternion.Inverse(ParentDefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, quaternion * Quaternion.Inverse(DefaultRootRotation), num) * bendNormal;
		}
		case BendModifier.Arm:
			if (bone1.transform.parent == null)
			{
				return bendNormal;
			}
			if (goal != AvatarIKGoal.LeftFoot && goal != AvatarIKGoal.RightFoot)
			{
				Vector3 normalized = (IKPosition - bone1.transform.position).normalized;
				normalized = Quaternion.Inverse(bone1.transform.parent.rotation * Quaternion.Inverse(ParentDefaultRotation)) * normalized;
				if (goal == AvatarIKGoal.LeftHand)
				{
					normalized.x = 0f - normalized.x;
				}
				for (int i = 1; i < AxisDirection_0.Length; i++)
				{
					AxisDirection_0[i].dot = Mathf.Clamp(Vector3.Dot(AxisDirection_0[i].direction, normalized), 0f, 1f);
					AxisDirection_0[i].dot = GClass1462.Float(AxisDirection_0[i].dot, InterpolationMode.InOutQuintic);
				}
				Vector3 vector2 = AxisDirection_0[0].axis;
				for (int j = 1; j < AxisDirection_0.Length; j++)
				{
					vector2 = Vector3.Slerp(vector2, AxisDirection_0[j].axis, AxisDirection_0[j].dot);
				}
				if (goal == AvatarIKGoal.LeftHand)
				{
					vector2.x = 0f - vector2.x;
					vector2 = -vector2;
				}
				Vector3 vector3 = bone1.transform.parent.rotation * Quaternion.Inverse(ParentDefaultRotation) * vector2;
				if (num >= 1f)
				{
					return vector3;
				}
				return Vector3.Lerp(bendNormal, vector3, num);
			}
			if (!GClass1465.logged)
			{
				LogWarning("Trying to use the 'Arm' bend modifier on a leg.");
			}
			return bendNormal;
		case BendModifier.Goal:
		{
			if (bendGoal == null)
			{
				if (!GClass1465.logged)
				{
					LogWarning("Trying to use the 'Goal' Bend Modifier, but the Bend Goal is unassigned.");
				}
				return bendNormal;
			}
			Vector3 vector = Vector3.Cross(bendGoal.position - bone1.transform.position, IKPosition - bone1.transform.position);
			if (vector == Vector3.zero)
			{
				return bendNormal;
			}
			if (num >= 1f)
			{
				return vector;
			}
			return Vector3.Lerp(bendNormal, vector, num);
		}
		case BendModifier.Custom:
			return bendNormal;
		}
	}

	public override void SetBendGoalPosition(Vector3 goalPosition, float weight)
	{
	}
}
