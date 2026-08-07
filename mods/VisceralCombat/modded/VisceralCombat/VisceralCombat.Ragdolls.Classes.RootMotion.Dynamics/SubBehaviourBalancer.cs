using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public class SubBehaviourBalancer : SubBehaviourBase
{
	[Serializable]
	public class Settings
	{
		[Tooltip("Ankle joint damper / spring. Increase to make the balancing effect softer.")]
		public float damperForSpring = 1f;

		[Tooltip("Multiplier for joint max force.")]
		public float maxForceMlp = 0.05f;

		[Tooltip("Multiplier for the inertia tensor. Increasing this will increase the balancing forces.")]
		public float IMlp = 1f;

		[Tooltip("Velocity-based prediction.")]
		public float velocityF = 0.5f;

		[Tooltip("World space offset for the center of pressure. Can be used to make the characer lean in a certain direction.")]
		public Vector3 copOffset;

		[Tooltip("The amount of torque applied to the lower legs to help keep the puppet balanced. Note that this is an external force (not coming from the joints themselves) and might make the simulation seem unnatural.")]
		public float torqueMlp = 0f;

		[Tooltip("Maximum magnitude of the torque applied to the lower legs if 'Torque Mlp' > 0.")]
		public float maxTorqueMag = 45f;
	}

	private Settings settings;

	private Rigidbody[] rigidbodies = (Rigidbody[])(object)new Rigidbody[0];

	private Transform[] copPoints = (Transform[])(object)new Transform[0];

	private PressureSensor pressureSensor;

	private Rigidbody Ibody;

	private Vector3 I;

	private Quaternion toJointSpace = Quaternion.identity;

	public ConfigurableJoint joint { get; private set; }

	public Vector3 dir { get; private set; }

	public Vector3 dirVel { get; private set; }

	public Vector3 cop { get; private set; }

	public Vector3 com { get; private set; }

	public Vector3 comV { get; private set; }

	public void Initiate(BehaviourBase behaviour, Settings settings, Rigidbody Ibody, Rigidbody[] rigidbodies, ConfigurableJoint joint, Transform[] copPoints, PressureSensor pressureSensor)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.behaviour = behaviour;
		this.settings = settings;
		this.Ibody = Ibody;
		this.rigidbodies = rigidbodies;
		this.joint = joint;
		this.copPoints = copPoints;
		this.pressureSensor = pressureSensor;
		toJointSpace = PhysXTools.ToJointSpace(joint);
		behaviour.OnPreFixedUpdate = (BehaviourBase.BehaviourUpdateDelegate)Delegate.Combine(behaviour.OnPreFixedUpdate, new BehaviourBase.BehaviourUpdateDelegate(Solve));
	}

	private void Solve(float deltaTime)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		if (copPoints.Length == 0)
		{
			cop = ((Component)joint).transform.TransformPoint(((Joint)joint).anchor);
		}
		else
		{
			cop = Vector3.zero;
			Transform[] array = copPoints;
			foreach (Transform val in array)
			{
				cop += val.position;
			}
			cop /= (float)copPoints.Length;
		}
		cop += settings.copOffset;
		com = PhysXTools.GetCenterOfMass(rigidbodies);
		comV = PhysXTools.GetCenterOfMassVelocity(rigidbodies);
		dir = com - cop;
		dirVel = com + comV * settings.velocityF - cop;
		Vector3 fromToAcceleration = PhysXTools.GetFromToAcceleration(dirVel, -Physics.gravity);
		fromToAcceleration -= Ibody.angularVelocity;
		Vector3 v = fromToAcceleration / deltaTime;
		PhysXTools.ScaleByInertia(ref v, Ibody.rotation, Ibody.inertiaTensor * settings.IMlp);
		v = Vector3.ClampMagnitude(v, settings.maxTorqueMag);
		if ((Object)(object)pressureSensor == (Object)null || !((Behaviour)pressureSensor).enabled || pressureSensor.inContact)
		{
			Ibody.AddTorque(v * settings.torqueMlp, (ForceMode)0);
			joint.targetAngularVelocity = Quaternion.Inverse(toJointSpace) * Quaternion.Inverse(((Component)joint).transform.rotation) * v;
		}
		else
		{
			joint.targetAngularVelocity = Vector3.zero;
		}
	}
}
