using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ConfigurableJointSpawner : JointSpawner
{
	[Serializable]
	public struct SoftJointLimitSpring_
	{
		public float spring;

		public float damper;

		public static implicit operator SoftJointLimitSpring_(SoftJointLimitSpring softJointLimitSpring)
		{
			return new SoftJointLimitSpring_
			{
				spring = softJointLimitSpring.spring,
				damper = softJointLimitSpring.damper
			};
		}

		public static implicit operator SoftJointLimitSpring(SoftJointLimitSpring_ softJointLimitSpring)
		{
			return new SoftJointLimitSpring
			{
				spring = softJointLimitSpring.spring,
				damper = softJointLimitSpring.damper
			};
		}
	}

	[Serializable]
	public struct SoftJointLimit_
	{
		public float limit;

		public float bounciness;

		public float contactDistance;

		public static implicit operator SoftJointLimit_(SoftJointLimit softJointLimit)
		{
			return new SoftJointLimit_
			{
				limit = softJointLimit.limit,
				bounciness = softJointLimit.bounciness,
				contactDistance = softJointLimit.contactDistance
			};
		}

		public static implicit operator SoftJointLimit(SoftJointLimit_ softJointLimit)
		{
			return new SoftJointLimit
			{
				limit = softJointLimit.limit,
				bounciness = softJointLimit.bounciness,
				contactDistance = softJointLimit.contactDistance
			};
		}
	}

	[Serializable]
	public struct JointDrive_
	{
		public float positionSpring;

		public float positionDamper;

		public float maximumForce;

		public static implicit operator JointDrive_(JointDrive jointDrive)
		{
			return new JointDrive_
			{
				positionSpring = jointDrive.positionSpring,
				positionDamper = jointDrive.positionDamper,
				maximumForce = jointDrive.maximumForce
			};
		}

		public static implicit operator JointDrive(JointDrive_ jointDrive)
		{
			return new JointDrive
			{
				positionSpring = jointDrive.positionSpring,
				positionDamper = jointDrive.positionDamper,
				maximumForce = jointDrive.maximumForce
			};
		}
	}

	public float projectionAngle;

	public float projectionDistance;

	[GAttribute10(typeof(JointProjectionMode))]
	public JointProjectionMode projectionMode;

	public JointDrive_ slerpDrive;

	public JointDrive_ angularYZDrive;

	public JointDrive_ angularXDrive;

	[GAttribute10(typeof(RotationDriveMode))]
	public RotationDriveMode rotationDriveMode;

	public Vector3 targetAngularVelocity;

	public Quaternion targetRotation;

	public JointDrive_ zDrive;

	public JointDrive_ yDrive;

	public JointDrive_ xDrive;

	public Vector3 targetVelocity;

	public Vector3 targetPosition;

	public SoftJointLimit_ angularZLimit;

	public SoftJointLimit_ angularYLimit;

	public SoftJointLimit_ highAngularXLimit;

	public SoftJointLimit_ lowAngularXLimit;

	public SoftJointLimit_ linearLimit;

	public SoftJointLimitSpring_ angularYZLimitSpring;

	public SoftJointLimitSpring_ angularXLimitSpring;

	public SoftJointLimitSpring_ linearLimitSpring;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion angularZMotion;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion angularYMotion;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion angularXMotion;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion zMotion;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion yMotion;

	[GAttribute10(typeof(ConfigurableJointMotion))]
	public ConfigurableJointMotion xMotion;

	public Vector3 secondaryAxis;

	public bool configuredInWorldSpace;

	public bool swapBodies;

	public override Joint Create()
	{
		if (_joint == null)
		{
			ConfigurableJoint obj = base.Create() as ConfigurableJoint;
			obj.projectionAngle = projectionAngle;
			obj.projectionDistance = projectionDistance;
			obj.projectionMode = projectionMode;
			obj.slerpDrive = slerpDrive;
			obj.angularYZDrive = angularYZDrive;
			obj.angularXDrive = angularXDrive;
			obj.rotationDriveMode = rotationDriveMode;
			obj.targetAngularVelocity = targetAngularVelocity;
			obj.targetRotation = targetRotation;
			obj.zDrive = zDrive;
			obj.yDrive = yDrive;
			obj.xDrive = xDrive;
			obj.targetVelocity = targetVelocity;
			obj.targetPosition = targetPosition;
			obj.angularZLimit = angularZLimit;
			obj.angularYLimit = angularYLimit;
			obj.highAngularXLimit = highAngularXLimit;
			obj.lowAngularXLimit = lowAngularXLimit;
			obj.linearLimit = linearLimit;
			obj.angularYZLimitSpring = angularYZLimitSpring;
			obj.angularXLimitSpring = angularXLimitSpring;
			obj.linearLimitSpring = linearLimitSpring;
			obj.angularZMotion = angularZMotion;
			obj.angularYMotion = angularYMotion;
			obj.angularXMotion = angularXMotion;
			obj.zMotion = zMotion;
			obj.yMotion = yMotion;
			obj.xMotion = xMotion;
			obj.secondaryAxis = secondaryAxis;
			obj.configuredInWorldSpace = configuredInWorldSpace;
			obj.swapBodies = swapBodies;
		}
		return _joint;
	}

	public override Joint CreateComponent()
	{
		return base.gameObject.AddComponent<ConfigurableJoint>();
	}
}
