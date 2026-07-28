using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public class MuscleLite
{
	[HideInInspector]
	public string name;

	public ConfigurableJoint joint;

	public Transform target;

	public float pinWeightMlp = 1f;

	public float muscleWeightMlp = 1f;

	public float muscleDamperMlp = 1f;

	public float mappingWeightMlp = 1f;

	private JointDrive slerpDrive = default(JointDrive);

	private Quaternion defaultLocalRotation = Quaternion.identity;

	private Quaternion toJointSpaceInverse = Quaternion.identity;

	private Quaternion toJointSpaceDefault = Quaternion.identity;

	private Quaternion targetAnimatedRotation = Quaternion.identity;

	private Quaternion defaultTargetLocalRotation = Quaternion.identity;

	private Quaternion toParentSpace = Quaternion.identity;

	private Quaternion targetAnimatedWorldRotation = Quaternion.identity;

	private Quaternion defaultRotation = Quaternion.identity;

	private Vector3 defaultPosition;

	private Vector3 defaultTargetLocalPosition;

	private float lastJointDriveRotationWeight;

	private float lastRotationDamper;

	private bool initiated;

	private Transform connectedBodyTarget;

	private Transform connectedBodyTransform;

	private Transform targetParent;

	private bool directTargetParent;

	private Vector3 targetVelocity;

	private Vector3 targetAnimatedCenterOfMass;

	public Transform transform { get; private set; }

	public Rigidbody rigidbody { get; private set; }

	public Vector3 positionOffset { get; private set; }

	public int index { get; private set; }

	private Quaternion localRotation => Quaternion.Inverse(parentRotation) * transform.rotation;

	private Quaternion targetLocalRotation => Quaternion.Inverse(targetParentRotation * toParentSpace) * target.rotation;

	private Quaternion parentRotation
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
			{
				return ((Joint)joint).connectedBody.rotation;
			}
			if ((Object)(object)transform.parent == (Object)null)
			{
				return Quaternion.identity;
			}
			return transform.parent.rotation;
		}
	}

	private Quaternion targetParentRotation
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)targetParent == (Object)null)
			{
				return Quaternion.identity;
			}
			return targetParent.rotation;
		}
	}

	public void Initiate(MuscleLite[] colleagues)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		name = ((Object)joint).name;
		transform = ((Component)joint).transform;
		rigidbody = ((Component)joint).GetComponent<Rigidbody>();
		if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
		{
			for (int i = 0; i < colleagues.Length; i++)
			{
				if ((Object)(object)((Component)colleagues[i].joint).GetComponent<Rigidbody>() == (Object)(object)((Joint)joint).connectedBody)
				{
					connectedBodyTarget = colleagues[i].target;
				}
				if (colleagues[i] == this)
				{
					index = i;
				}
			}
			((Joint)joint).autoConfigureConnectedAnchor = false;
			connectedBodyTransform = ((Component)((Joint)joint).connectedBody).transform;
			directTargetParent = (Object)(object)target.parent == (Object)(object)connectedBodyTarget;
		}
		targetParent = (((Object)(object)connectedBodyTarget != (Object)null) ? connectedBodyTarget : target.parent);
		toParentSpace = Quaternion.Inverse(targetParentRotation) * parentRotation;
		Vector3 val = Vector3.Cross(((Joint)joint).axis, joint.secondaryAxis);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = Vector3.Cross(normalized, ((Joint)joint).axis);
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		defaultLocalRotation = localRotation;
		Quaternion val2 = Quaternion.LookRotation(normalized, normalized2);
		toJointSpaceInverse = Quaternion.Inverse(val2);
		toJointSpaceDefault = defaultLocalRotation * val2;
		joint.rotationDriveMode = (RotationDriveMode)1;
		joint.configuredInWorldSpace = false;
		defaultTargetLocalPosition = target.localPosition;
		defaultTargetLocalRotation = target.localRotation;
		targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
		if ((Object)(object)((Joint)joint).connectedBody == (Object)null)
		{
			defaultPosition = transform.localPosition;
			defaultRotation = transform.localRotation;
		}
		else
		{
			defaultPosition = ((Component)((Joint)joint).connectedBody).transform.InverseTransformPoint(transform.position);
			defaultRotation = Quaternion.Inverse(((Component)((Joint)joint).connectedBody).transform.rotation) * transform.rotation;
		}
		rigidbody.isKinematic = false;
		Read();
		initiated = true;
	}

	public void FixTargetTransforms()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			target.localRotation = defaultTargetLocalRotation;
			target.localPosition = defaultTargetLocalPosition;
		}
	}

	public void Reset()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (initiated && !((Object)(object)joint == (Object)null))
		{
			if ((Object)(object)((Joint)joint).connectedBody == (Object)null)
			{
				transform.localPosition = defaultPosition;
				transform.localRotation = defaultRotation;
			}
			else
			{
				transform.position = ((Component)((Joint)joint).connectedBody).transform.TransformPoint(defaultPosition);
				transform.rotation = ((Component)((Joint)joint).connectedBody).transform.rotation * defaultRotation;
			}
			lastRotationDamper = -1f;
		}
	}

	public void MoveToTarget()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			transform.SetPositionAndRotation(target.position, target.rotation);
			rigidbody.MovePosition(transform.position);
			rigidbody.MoveRotation(transform.rotation);
		}
	}

	public void ClearVelocities()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		targetVelocity = Vector3.zero;
		targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
	}

	public void Read()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
		targetVelocity = (val - targetAnimatedCenterOfMass) / Time.deltaTime;
		targetAnimatedCenterOfMass = val;
		if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
		{
			targetAnimatedRotation = targetLocalRotation;
		}
		targetAnimatedWorldRotation = target.rotation;
	}

	public void Update(float pinWeightMaster, float muscleWeightMaster, float muscleSpring, float muscleDamper, bool angularPinning)
	{
		Pin(pinWeightMaster, 4f, 0f, angularPinning);
		MuscleRotation(muscleWeightMaster, muscleSpring, muscleDamper);
	}

	private void Pin(float pinWeightMaster, float pinPow, float pinDistanceFalloff, bool angularPinning)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		positionOffset = targetAnimatedCenterOfMass - rigidbody.worldCenterOfMass;
		if (float.IsNaN(positionOffset.x))
		{
			positionOffset = Vector3.zero;
		}
		float num = pinWeightMaster * pinWeightMlp;
		if (!(num <= 0f))
		{
			num = Mathf.Pow(num, pinPow);
			if (Time.deltaTime > 0f)
			{
				positionOffset /= Time.deltaTime;
			}
			Vector3 val = -rigidbody.velocity + targetVelocity + positionOffset;
			val *= num;
			if (pinDistanceFalloff > 0f)
			{
				Vector3 val2 = val;
				Vector3 val3 = positionOffset;
				val = val2 / (1f + ((Vector3)(ref val3)).sqrMagnitude * pinDistanceFalloff);
			}
			rigidbody.AddForce(val, (ForceMode)2);
			if (angularPinning)
			{
				Vector3 angularAcceleration = PhysXTools.GetAngularAcceleration(rigidbody.rotation, targetAnimatedWorldRotation);
				angularAcceleration -= rigidbody.angularVelocity;
				angularAcceleration *= num;
				rigidbody.AddTorque(angularAcceleration, (ForceMode)2);
			}
		}
	}

	private void MuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		float num = muscleWeightMaster * muscleSpring * muscleWeightMlp * 10f;
		if ((Object)(object)((Joint)joint).connectedBody == (Object)null)
		{
			num = 0f;
		}
		else if (num > 0f)
		{
			joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);
		}
		float num2 = muscleDamper * muscleDamperMlp;
		if (num != lastJointDriveRotationWeight || num2 != lastRotationDamper)
		{
			lastJointDriveRotationWeight = num;
			lastRotationDamper = num2;
			((JointDrive)(ref slerpDrive)).positionSpring = num;
			((JointDrive)(ref slerpDrive)).maximumForce = Mathf.Max(num, num2);
			((JointDrive)(ref slerpDrive)).positionDamper = num2;
			joint.slerpDrive = slerpDrive;
		}
	}

	public void Map(float masterWeight)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		float num = masterWeight * mappingWeightMlp;
		if (num <= 0f)
		{
			return;
		}
		Quaternion rotation = transform.rotation;
		Vector3 val = transform.position;
		if (num >= 1f)
		{
			if ((Object)(object)connectedBodyTransform != (Object)null)
			{
				Vector3 val2 = connectedBodyTransform.InverseTransformPoint(transform.position);
				val = connectedBodyTarget.TransformPoint(val2);
			}
			target.SetPositionAndRotation(val, rotation);
			return;
		}
		rotation = Quaternion.Lerp(target.rotation, rotation, num);
		if ((Object)(object)connectedBodyTransform != (Object)null)
		{
			Vector3 val3 = connectedBodyTransform.InverseTransformPoint(transform.position);
			val = Vector3.Lerp(target.position, connectedBodyTarget.TransformPoint(val3), num);
		}
		else
		{
			val = Vector3.Lerp(target.position, transform.position, num);
		}
		target.SetPositionAndRotation(val, rotation);
	}

	public void UpdateAnchor(bool supportTranslationAnimation)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Joint)joint).connectedBody == (Object)null) && !((Object)(object)connectedBodyTarget == (Object)null) && (!directTargetParent || supportTranslationAnimation))
		{
			Vector3 val2 = (((Joint)joint).connectedAnchor = InverseTransformPointUnscaled(connectedBodyTarget.position, connectedBodyTarget.rotation * toParentSpace, target.position));
			Vector3 val3 = val2;
			float num = 1f / connectedBodyTransform.lossyScale.x;
			((Joint)joint).connectedAnchor = val3 * num;
		}
	}

	private Quaternion LocalToJointSpace(Quaternion localRotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return toJointSpaceInverse * Quaternion.Inverse(localRotation) * toJointSpaceDefault;
	}

	private static Vector3 InverseTransformPointUnscaled(Vector3 position, Quaternion rotation, Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Inverse(rotation) * (point - position);
	}
}
