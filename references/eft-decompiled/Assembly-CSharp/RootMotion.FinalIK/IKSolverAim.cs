using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverAim : IKSolverHeuristic
{
	public Transform transform;

	public Vector3 axis = Vector3.forward;

	public Vector3 poleAxis = Vector3.up;

	public Vector3 polePosition;

	[Range(0f, 1f)]
	public float poleWeight;

	public Transform poleTarget;

	[Range(0f, 1f)]
	public float clampWeight = 0.1f;

	[Range(0f, 2f)]
	public int clampSmoothing = 2;

	public GDelegate48 OnPreIteration;

	[NonSerialized]
	public float Step;

	[NonSerialized]
	public Vector3 ClampedIKPosition;

	[NonSerialized]
	public RotationLimit TransformLimit;

	[NonSerialized]
	public Transform LastTransform;

	public Vector3 transformAxis => transform.rotation * axis;

	public Vector3 transformPoleAxis => transform.rotation * poleAxis;

	public override int minBones => 1;

	public override Vector3 localDirection => bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.forward);

	public float GetAngle()
	{
		return Vector3.Angle(transformAxis, IKPosition - transform.position);
	}

	public override void OnInitiate()
	{
		if ((FirstInitiation || !Application.isPlaying) && transform != null)
		{
			IKPosition = transform.position + transformAxis * 3f;
			polePosition = transform.position + transformPoleAxis * 3f;
		}
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i].rotationLimit != null)
			{
				bones[i].rotationLimit.Disable();
			}
		}
		Step = 1f / (float)bones.Length;
		if (Application.isPlaying)
		{
			axis = axis.normalized;
		}
	}

	public override void OnUpdate()
	{
		if (axis == Vector3.zero)
		{
			if (!GClass1465.logged)
			{
				LogWarning("IKSolverAim axis is Vector3.zero.");
			}
			return;
		}
		if (poleAxis == Vector3.zero && poleWeight > 0f)
		{
			if (!GClass1465.logged)
			{
				LogWarning("IKSolverAim poleAxis is Vector3.zero.");
			}
			return;
		}
		if (target != null)
		{
			IKPosition = target.position;
		}
		if (poleTarget != null)
		{
			polePosition = poleTarget.position;
		}
		if (XY)
		{
			IKPosition.z = bones[0].transform.position.z;
		}
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		if (transform != LastTransform)
		{
			TransformLimit = transform.GetComponent<RotationLimit>();
			if (TransformLimit != null)
			{
				TransformLimit.enabled = false;
			}
			LastTransform = transform;
		}
		if (TransformLimit != null)
		{
			TransformLimit.Apply();
		}
		if (transform == null)
		{
			if (!GClass1465.logged)
			{
				LogWarning("Aim Transform unassigned in Aim IK solver. Please Assign a Transform (lineal descendant to the last bone in the spine) that you want to be aimed at IKPosition");
			}
			return;
		}
		clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		ClampedIKPosition = method_2();
		Vector3 b = ClampedIKPosition - transform.position;
		b = Vector3.Slerp(transformAxis * b.magnitude, b, IKPositionWeight);
		ClampedIKPosition = transform.position + b;
		for (int i = 0; i < maxIterations && (i < 1 || !(tolerance > 0f) || !(GetAngle() < tolerance)); i++)
		{
			LastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			method_1();
		}
		LastLocalDirection = localDirection;
	}

	public void method_1()
	{
		for (int i = 0; i < bones.Length - 1; i++)
		{
			method_3(ClampedIKPosition, bones[i], Step * (float)(i + 1) * IKPositionWeight * bones[i].weight);
		}
		method_3(ClampedIKPosition, bones[bones.Length - 1], IKPositionWeight * bones[bones.Length - 1].weight);
	}

	public Vector3 method_2()
	{
		if (clampWeight <= 0f)
		{
			return IKPosition;
		}
		if (clampWeight >= 1f)
		{
			return transform.position + transformAxis * (IKPosition - transform.position).magnitude;
		}
		float num = Vector3.Angle(transformAxis, IKPosition - transform.position);
		float num2 = 1f - num / 180f;
		float num3 = ((clampWeight > 0f) ? Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clampWeight > 0f) ? Mathf.Clamp(num2 / clampWeight, 0f, 1f) : 1f);
		for (int i = 0; i < clampSmoothing; i++)
		{
			num4 = Mathf.Sin(num4 * MathF.PI * 0.5f);
		}
		return transform.position + Vector3.Slerp(transformAxis * 10f, IKPosition - transform.position, num4 * num3);
	}

	public void method_3(Vector3 targetPosition, Bone bone, float weight)
	{
		if (XY)
		{
			if (weight >= 0f)
			{
				Vector3 vector = transformAxis;
				Vector3 vector2 = targetPosition - transform.position;
				float current = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
				float num = Mathf.Atan2(vector2.x, vector2.y) * 57.29578f;
				bone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(current, num), Vector3.back) * bone.transform.rotation;
			}
		}
		else
		{
			if (weight >= 0f)
			{
				Quaternion quaternion = Quaternion.FromToRotation(transformAxis, targetPosition - transform.position);
				if (weight >= 1f)
				{
					bone.transform.rotation = quaternion * bone.transform.rotation;
				}
				else
				{
					bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, quaternion, weight) * bone.transform.rotation;
				}
			}
			if (poleWeight > 0f)
			{
				Vector3 tangent = polePosition - transform.position;
				Vector3 normal = transformAxis;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Quaternion b = Quaternion.FromToRotation(transformPoleAxis, tangent);
				bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, b, weight * poleWeight) * bone.transform.rotation;
			}
		}
		if (useRotationLimits && bone.rotationLimit != null)
		{
			bone.rotationLimit.Apply();
		}
	}
}
