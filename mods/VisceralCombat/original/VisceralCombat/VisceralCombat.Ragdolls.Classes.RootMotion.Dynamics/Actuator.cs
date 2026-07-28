using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class Actuator : MonoBehaviour
{
	public Transform target;

	public float spring = 1000f;

	public float damper = 100f;

	private Rigidbody r;

	private ConfigurableJoint joint;

	private Quaternion toJointSpaceInverse = Quaternion.identity;

	private Quaternion toJointSpaceDefault = Quaternion.identity;

	private JointDrive slerpDrive = default(JointDrive);

	private float lastSpring;

	private float lastDamper;

	private void Start()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		r = ((Component)this).GetComponent<Rigidbody>();
		joint = ((Component)this).GetComponent<ConfigurableJoint>();
		if ((Object)(object)joint == (Object)null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		Vector3 val = Vector3.Cross(((Joint)joint).axis, joint.secondaryAxis);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = Vector3.Cross(normalized, ((Joint)joint).axis);
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		Quaternion localRotation = ((Component)this).transform.localRotation;
		Quaternion val2 = Quaternion.LookRotation(normalized, normalized2);
		toJointSpaceInverse = Quaternion.Inverse(val2);
		toJointSpaceDefault = localRotation * val2;
		joint.rotationDriveMode = (RotationDriveMode)1;
		joint.configuredInWorldSpace = false;
	}

	private void FixedUpdate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!r.isKinematic)
		{
			if (spring > 0f)
			{
				joint.targetRotation = LocalToJointSpace(target.localRotation);
			}
			if (spring != lastSpring || damper != lastDamper)
			{
				lastSpring = spring;
				lastDamper = damper;
				((JointDrive)(ref slerpDrive)).positionSpring = spring;
				((JointDrive)(ref slerpDrive)).positionDamper = damper;
				((JointDrive)(ref slerpDrive)).maximumForce = Mathf.Max(spring, damper);
				joint.slerpDrive = slerpDrive;
			}
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
}
