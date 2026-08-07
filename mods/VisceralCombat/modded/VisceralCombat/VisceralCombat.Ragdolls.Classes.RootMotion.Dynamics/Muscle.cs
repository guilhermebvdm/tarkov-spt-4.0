using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public class Muscle
{
	[Serializable]
	public enum Group
	{
		Hips,
		Spine,
		Head,
		Arm,
		Hand,
		Leg,
		Foot,
		Tail,
		Prop
	}

	[Serializable]
	public class InternalCollisionIgnoreSettings
	{
		[Tooltip("If true, internal collisions between this muscle and all other muscles will be ingored.")]
		public bool ignoreAll;

		[Tooltip("Ignore internal collisions with all muscles in this array.")]
		public ConfigurableJoint[] muscles = (ConfigurableJoint[])(object)new ConfigurableJoint[0];

		[Tooltip("Ignore internal collisions with all these groups.")]
		public Group[] groups = new Group[0];
	}

	[Serializable]
	public class Props
	{
		[Tooltip("Which body part does this muscle belong to?")]
		public Group group;

		[Tooltip("The weight (multiplier) of mapping this muscle's target to the muscle.")]
		[Range(0f, 1f)]
		public float mappingWeight = 1f;

		[Tooltip("The weight (multiplier) of pinning this muscle to its target's position using a simple AddForce command.")]
		[Range(0f, 1f)]
		public float pinWeight = 1f;

		[Tooltip("The muscle strength (multiplier).")]
		[Range(0f, 1f)]
		public float muscleWeight = 1f;

		[Tooltip("Multiplier of the positionDamper of the ConfigurableJoints' Slerp Drive.")]
		[Range(0f, 1f)]
		public float muscleDamper = 1f;

		[Tooltip("Defines which muscles or muscle groups internal collisions are always ignored with.")]
		public InternalCollisionIgnoreSettings internalCollisionIgnores = new InternalCollisionIgnoreSettings();

		[Tooltip("List of animated bones parented to this muscle's Target, except for the bones that are targets or target children of any child muscles. This is used for stopping animation on those bones when the muscle has been disconnected using PuppetMaster.DisconnectMuscleRecursive().For example if you disconnected the spine02 muscle, you would want to have spine03 and clavicles in this list to stop them from animating.")]
		public Transform[] animatedTargetChildren = (Transform[])(object)new Transform[0];

		public bool mapPosition
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		public Props()
		{
			mappingWeight = 1f;
			pinWeight = 1f;
			muscleWeight = 1f;
			muscleDamper = 1f;
		}

		public Props(float pinWeight, float muscleWeight, float mappingWeight, float muscleDamper, Group group = Group.Hips)
		{
			this.pinWeight = pinWeight;
			this.muscleWeight = muscleWeight;
			this.mappingWeight = mappingWeight;
			this.muscleDamper = muscleDamper;
			this.group = group;
		}

		public void Clamp()
		{
			mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
			pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
			muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
			muscleDamper = Mathf.Clamp(muscleDamper, 0f, 1f);
		}
	}

	public struct State
	{
		public float mappingWeightMlp;

		public float pinWeightMlp;

		public float muscleWeightMlp;

		public float maxForceMlp;

		public float muscleDamperMlp;

		public float muscleDamperAdd;

		public float immunity;

		public float impulseMlp;

		public Vector3 velocity;

		public Vector3 angularVelocity;

		public bool isDisconnected;

		public bool resetFlag;

		public static State Default
		{
			get
			{
				State result = default(State);
				result.mappingWeightMlp = 1f;
				result.pinWeightMlp = 1f;
				result.muscleWeightMlp = 1f;
				result.muscleDamperMlp = 1f;
				result.muscleDamperAdd = 0f;
				result.maxForceMlp = 1f;
				result.immunity = 0f;
				result.impulseMlp = 1f;
				result.isDisconnected = false;
				return result;
			}
		}

		public void Clamp()
		{
			mappingWeightMlp = Mathf.Clamp(mappingWeightMlp, 0f, 1f);
			pinWeightMlp = Mathf.Clamp(pinWeightMlp, 0f, 1f);
			muscleWeightMlp = Mathf.Clamp(muscleWeightMlp, 0f, muscleWeightMlp);
			immunity = Mathf.Clamp(immunity, 0f, 1f);
			impulseMlp = Mathf.Max(impulseMlp, 0f);
		}
	}

	public class TargetChild
	{
		public Transform t;

		public Vector3 defaultLocalPosition;

		public Quaternion defaultLocalRotation = Quaternion.identity;

		public TargetChild(Transform t)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			this.t = t;
			defaultLocalPosition = t.localPosition;
			defaultLocalRotation = t.localRotation;
		}

		public void Map()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			t.localPosition = defaultLocalPosition;
			t.localRotation = defaultLocalRotation;
		}
	}

	[HideInInspector]
	public string name;

	public ConfigurableJoint joint;

	public Transform target;

	public Props props = new Props();

	public State state = State.Default;

	[HideInInspector]
	public int[] parentIndexes = new int[0];

	[HideInInspector]
	public int[] childIndexes = new int[0];

	[HideInInspector]
	public bool[] childFlags = new bool[0];

	[HideInInspector]
	public int[] kinshipDegrees = new int[0];

	[HideInInspector]
	public MuscleCollisionBroadcaster broadcaster;

	[HideInInspector]
	public JointBreakBroadcaster jointBreakBroadcaster;

	[HideInInspector]
	public Vector3 positionOffset;

	[HideInInspector]
	public ConfigurableJoint additionalPin;

	[HideInInspector]
	public Transform additionalPinTarget;

	[HideInInspector]
	public float additionalPinWeight;

	[HideInInspector]
	public Vector3 mappedVelocity;

	[HideInInspector]
	public Vector3 mappedAngularVelocity;

	[HideInInspector]
	public bool isPropMuscle;

	[HideInInspector]
	public int index = -1;

	private Transform rebuildParent;

	private float currMuscleTimer = 0f;

	private float TimeToSleep = VisceralEntry.Instance.RagdollSleepTime.Value;

	private VisceralEntry vInstance = VisceralEntry.Instance;

	private Vector3 rebuildPosition;

	private Quaternion rebuildRotation = Quaternion.identity;

	private Vector3 rebuildTargetPosition;

	private Quaternion rebuildTargetRotation = Quaternion.identity;

	private ConfigurableJointMotion rebuildAngularXMotion;

	private ConfigurableJointMotion rebuildAngularYMotion;

	private ConfigurableJointMotion rebuildAngularZMotion;

	[HideInInspector]
	public bool ignoreTargetVelocity;

	[HideInInspector]
	public Vector3 targetMappedPosition;

	[HideInInspector]
	public Quaternion targetMappedRotation = Quaternion.identity;

	[HideInInspector]
	public Vector3 targetSampledPosition;

	[HideInInspector]
	public Quaternion targetSampledRotation = Quaternion.identity;

	private JointDrive slerpDrive = default(JointDrive);

	private float lastJointDriveRotationWeight = -1f;

	private float lastRotationDamper = -1f;

	private Vector3 defaultPosition;

	private Vector3 defaultTargetLocalPosition;

	private Vector3 lastMappedPosition;

	private Quaternion defaultLocalRotation;

	private Quaternion localRotationConvert;

	private Quaternion toParentSpace;

	private Quaternion toJointSpaceInverse;

	private Quaternion toJointSpaceDefault;

	private Quaternion targetAnimatedRotation;

	private Quaternion defaultRotation;

	private Quaternion rotationRelativeToTarget;

	private Quaternion defaultTargetLocalRotation;

	private Quaternion lastMappedRotation;

	private Transform targetParent;

	private Transform connectedBodyTransform;

	private ConfigurableJointMotion angularXMotionDefault;

	private ConfigurableJointMotion angularYMotionDefault;

	private ConfigurableJointMotion angularZMotionDefault;

	private bool directTargetParent;

	private bool initiated;

	private Collider[] _colliders = (Collider[])(object)new Collider[0];

	private float lastReadTime;

	private float lastWriteTime;

	private bool[] disabledColliders = new bool[0];

	private TargetChild[] targetChildren = new TargetChild[0];

	private Vector3 additionalTargetVelocity;

	private Vector3 targetAnimatedCenterOfMass;

	private Vector3 additionalPinTargetAnimatedCenterOfMass;

	private Quaternion defaultTargetRotRelToMuscleInverse = Quaternion.identity;

	public Transform transform { get; private set; }

	public Rigidbody rigidbody { get; private set; }

	public Transform connectedBodyTarget { get; private set; }

	public Vector3 targetAnimatedPosition { get; private set; }

	public Quaternion targetAnimatedWorldRotation { get; private set; }

	public Collider[] colliders
	{
		get
		{
			return _colliders;
		}
		set
		{
			_colliders = value;
		}
	}

	public Vector3 targetVelocity { get; private set; }

	public Rigidbody additionalRigidbody { get; private set; }

	public Quaternion targetRotationRelative { get; private set; }

	public Rigidbody rebuildConnectedBody { get; private set; }

	public Transform rebuildTargetParent { get; private set; }

	public Vector3 defaultTargetPosRelToMuscle { get; private set; }

	public Quaternion defaultTargetRotRelToMuscle { get; private set; }

	public Quaternion defaultMuscleRotRelToTarget { get; private set; }

	private Quaternion localRotation => Quaternion.Inverse(parentRotation) * transform.rotation;

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

	private Quaternion targetLocalRotation => Quaternion.Inverse(targetParentRotation * toParentSpace) * target.rotation;

	public bool IsValid(bool log)
	{
		if ((Object)(object)joint == (Object)null)
		{
			if (log)
			{
			}
			return false;
		}
		if ((Object)(object)target == (Object)null)
		{
			if (log)
			{
			}
			return false;
		}
		if (props != null || log)
		{
		}
		return true;
	}

	public void Rebuild()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		((Component)joint).transform.parent = rebuildParent;
		target.parent = rebuildTargetParent;
		((Component)joint).transform.position = rebuildPosition;
		((Component)joint).transform.rotation = rebuildRotation;
		target.position = rebuildTargetPosition;
		target.rotation = rebuildTargetRotation;
		joint.angularXMotion = rebuildAngularXMotion;
		joint.angularYMotion = rebuildAngularYMotion;
		joint.angularZMotion = rebuildAngularZMotion;
		state = State.Default;
	}

	public virtual void Initiate(Muscle[] colleagues)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_054b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		initiated = false;
		if (!IsValid(log: true))
		{
			return;
		}
		name = ((Object)joint).name;
		state = State.Default;
		if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
		{
			for (int i = 0; i < colleagues.Length; i++)
			{
				if ((Object)(object)((Component)colleagues[i].joint).GetComponent<Rigidbody>() == (Object)(object)((Joint)joint).connectedBody)
				{
					connectedBodyTarget = colleagues[i].target;
				}
			}
		}
		transform = ((Component)joint).transform;
		rigidbody = ((Component)transform).GetComponent<Rigidbody>();
		InitiateAdditionalPin();
		SetKinematic(to: false);
		UpdateColliders();
		if (_colliders.Length == 0)
		{
			Vector3 size = Vector3.one * 0.1f;
			Renderer component = ((Component)transform).GetComponent<Renderer>();
			if ((Object)(object)component != (Object)null)
			{
				Bounds bounds = component.bounds;
				size = bounds.size;
			}
			rigidbody.inertiaTensor = PhysXTools.CalculateInertiaTensorCuboid(size, rigidbody.mass);
		}
		targetParent = (((Object)(object)connectedBodyTarget != (Object)null) ? connectedBodyTarget : target.parent);
		rebuildConnectedBody = ((Joint)joint).connectedBody;
		rebuildTargetParent = target.parent;
		rebuildParent = ((Component)joint).transform.parent;
		rebuildPosition = ((Component)joint).transform.position;
		rebuildRotation = ((Component)joint).transform.rotation;
		rebuildTargetPosition = target.position;
		rebuildTargetRotation = target.rotation;
		rebuildAngularXMotion = joint.angularXMotion;
		rebuildAngularYMotion = joint.angularYMotion;
		rebuildAngularZMotion = joint.angularZMotion;
		defaultLocalRotation = localRotation;
		Vector3 val = Vector3.Cross(((Joint)joint).axis, joint.secondaryAxis);
		Vector3 normalized = val.normalized;
		val = Vector3.Cross(normalized, ((Joint)joint).axis);
		Vector3 normalized2 = val.normalized;
		if (normalized == normalized2)
		{
			return;
		}
		rotationRelativeToTarget = Quaternion.Inverse(target.rotation) * transform.rotation;
		defaultTargetPosRelToMuscle = transform.InverseTransformPoint(target.position);
		defaultTargetRotRelToMuscle = Quaternion.Inverse(transform.rotation) * target.rotation;
		defaultTargetRotRelToMuscleInverse = Quaternion.Inverse(defaultTargetRotRelToMuscle);
		defaultMuscleRotRelToTarget = QuaTools.FromToRotation(target.rotation, transform.rotation);
		Quaternion val2 = Quaternion.LookRotation(normalized, normalized2);
		toJointSpaceInverse = Quaternion.Inverse(val2);
		toJointSpaceDefault = defaultLocalRotation * val2;
		toParentSpace = Quaternion.Inverse(targetParentRotation) * parentRotation;
		localRotationConvert = Quaternion.Inverse(targetLocalRotation) * localRotation;
		if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
		{
			((Joint)joint).autoConfigureConnectedAnchor = false;
			connectedBodyTransform = ((Component)((Joint)joint).connectedBody).transform;
			directTargetParent = (Object)(object)target.parent == (Object)(object)connectedBodyTarget;
			UpdateAnchor(supportTranslationAnimation: true);
		}
		angularXMotionDefault = joint.angularXMotion;
		angularYMotionDefault = joint.angularYMotion;
		angularZMotionDefault = joint.angularZMotion;
		targetRotationRelative = Quaternion.Inverse(((Component)rigidbody).transform.rotation) * target.rotation;
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
		defaultTargetLocalPosition = target.localPosition;
		defaultTargetLocalRotation = target.localRotation;
		joint.rotationDriveMode = (RotationDriveMode)1;
		if (!((Component)joint).gameObject.activeInHierarchy)
		{
			return;
		}
		joint.configuredInWorldSpace = false;
		joint.projectionMode = (JointProjectionMode)0;
		if (!(((Joint)joint).anchor != Vector3.zero))
		{
			targetAnimatedPosition = target.position;
			targetAnimatedCenterOfMass = rigidbody.worldCenterOfMass;
			targetAnimatedWorldRotation = target.rotation;
			targetAnimatedRotation = targetLocalRotation * localRotationConvert;
			Read();
			lastReadTime = Time.time;
			lastWriteTime = Time.time;
			lastMappedPosition = target.position;
			lastMappedRotation = target.rotation;
			targetChildren = new TargetChild[props.animatedTargetChildren.Length];
			for (int j = 0; j < targetChildren.Length; j++)
			{
				targetChildren[j] = new TargetChild(props.animatedTargetChildren[j]);
			}
			initiated = true;
		}
	}

	public void InitiateAdditionalPin()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)additionalPin != (Object)null)
		{
			additionalRigidbody = ((Component)additionalPin).GetComponent<Rigidbody>();
			additionalPinTargetAnimatedCenterOfMass = additionalRigidbody.worldCenterOfMass;
			additionalRigidbody.inertiaTensor = PhysXTools.CalculateInertiaTensorCuboid(Vector3.one * 0.1f, additionalRigidbody.mass);
		}
	}

	public void UpdateColliders()
	{
		_colliders = (Collider[])(object)new Collider[0];
		AddColliders(((Component)joint).transform, ref _colliders, includeMeshColliders: true);
		int childCount = ((Component)joint).transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			AddCompoundColliders(((Component)joint).transform.GetChild(i), ref _colliders);
		}
		disabledColliders = new bool[_colliders.Length];
	}

	public void DisableColliders()
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			if (disabledColliders[i])
			{
				return;
			}
		}
		for (int j = 0; j < _colliders.Length; j++)
		{
			disabledColliders[j] = _colliders[j].enabled;
			_colliders[j].enabled = false;
		}
	}

	public void EnableColliders()
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (disabledColliders[i])
			{
				_colliders[i].enabled = true;
			}
			disabledColliders[i] = false;
		}
	}

	private void AddColliders(Transform t, ref Collider[] C, bool includeMeshColliders)
	{
		Collider[] components = ((Component)t).GetComponents<Collider>();
		int num = 0;
		Collider[] array = components;
		foreach (Collider val in array)
		{
			bool flag = val is MeshCollider;
			if (!val.isTrigger && (!includeMeshColliders || !flag))
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		int num2 = C.Length;
		Array.Resize(ref C, num2 + num);
		int num3 = 0;
		for (int j = 0; j < components.Length; j++)
		{
			bool flag2 = components[j] is MeshCollider;
			if (!components[j].isTrigger && (!includeMeshColliders || !flag2))
			{
				C[num2 + num3] = components[j];
				num3++;
			}
		}
	}

	private void AddCompoundColliders(Transform t, ref Collider[] colliders)
	{
		if (!((Object)(object)((Component)t).GetComponent<Rigidbody>() != (Object)null))
		{
			AddColliders(t, ref colliders, includeMeshColliders: false);
			int childCount = t.childCount;
			for (int i = 0; i < childCount; i++)
			{
				AddCompoundColliders(t.GetChild(i), ref colliders);
			}
		}
	}

	public void IgnoreInternalCollisions(Muscle m)
	{
		if (m == this || state.isDisconnected || m.state.isDisconnected)
		{
			return;
		}
		Collider[] array = colliders;
		foreach (Collider val in array)
		{
			Collider[] array2 = m.colliders;
			foreach (Collider val2 in array2)
			{
				if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null && val.enabled && val2.enabled && ((Component)val).gameObject.activeInHierarchy && ((Component)val2).gameObject.activeInHierarchy)
				{
					Physics.IgnoreCollision(val, val2);
				}
			}
		}
	}

	public void ResetInternalCollisions(Muscle m, bool useInternalCollisionIgnores)
	{
		if (m == this)
		{
			return;
		}
		bool flag = useInternalCollisionIgnores && ForceIgnore(m);
		Collider[] array = colliders;
		foreach (Collider val in array)
		{
			Collider[] array2 = m.colliders;
			foreach (Collider val2 in array2)
			{
				if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null && val.enabled && val2.enabled && ((Component)val).gameObject.activeInHierarchy && ((Component)val2).gameObject.activeInHierarchy)
				{
					if (!flag)
					{
						Physics.IgnoreCollision(val, val2, false);
					}
					else
					{
						Physics.IgnoreCollision(val, val2, true);
					}
				}
			}
		}
	}

	private bool ForceIgnore(Muscle otherMuscle)
	{
		if (state.isDisconnected)
		{
			return false;
		}
		if (otherMuscle.state.isDisconnected)
		{
			return false;
		}
		if (props.internalCollisionIgnores.ignoreAll || otherMuscle.props.internalCollisionIgnores.ignoreAll)
		{
			return true;
		}
		ConfigurableJoint[] muscles = props.internalCollisionIgnores.muscles;
		foreach (ConfigurableJoint val in muscles)
		{
			if ((Object)(object)val == (Object)(object)otherMuscle.joint)
			{
				return true;
			}
		}
		Group[] groups = props.internalCollisionIgnores.groups;
		foreach (Group group in groups)
		{
			if (group == otherMuscle.props.group)
			{
				return true;
			}
		}
		ConfigurableJoint[] muscles2 = otherMuscle.props.internalCollisionIgnores.muscles;
		foreach (ConfigurableJoint val2 in muscles2)
		{
			if ((Object)(object)val2 == (Object)(object)joint)
			{
				return true;
			}
		}
		Group[] groups2 = otherMuscle.props.internalCollisionIgnores.groups;
		foreach (Group group2 in groups2)
		{
			if (group2 == props.group)
			{
				return true;
			}
		}
		return false;
	}

	public void IgnoreAngularLimits(bool ignore)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			joint.angularXMotion = (ConfigurableJointMotion)(ignore ? 2 : ((int)angularXMotionDefault));
			joint.angularYMotion = (ConfigurableJointMotion)(ignore ? 2 : ((int)angularYMotionDefault));
			joint.angularZMotion = (ConfigurableJointMotion)(ignore ? 2 : ((int)angularZMotionDefault));
		}
	}

	public void ResetTargetLocalPosition()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		target.localPosition = defaultTargetLocalPosition;
	}

	public void FixTargetTransforms()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			target.localPosition = defaultTargetLocalPosition;
			target.localRotation = defaultTargetLocalRotation;
		}
	}

	public void Reset()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (initiated && !((Object)(object)joint == (Object)null) && !state.isDisconnected)
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
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (initiated && !state.isDisconnected)
		{
			Vector3 position = target.position;
			Quaternion val = target.rotation * rotationRelativeToTarget;
			transform.SetPositionAndRotation(position, val);
			rigidbody.MovePosition(position);
			rigidbody.MoveRotation(val);
			positionOffset = Vector3.zero;
			if ((Object)(object)additionalPin != (Object)null)
			{
				((Component)additionalPin).transform.position = additionalPinTarget.position;
			}
		}
	}

	public void SetKinematic(bool to)
	{
		if (to)
		{
			rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
		}
		rigidbody.isKinematic = to;
		if ((Object)(object)additionalPin != (Object)null)
		{
			additionalRigidbody.isKinematic = to;
		}
	}

	public void Read()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (state.isDisconnected)
		{
			return;
		}
		float num = Time.time - lastReadTime;
		lastReadTime = Time.time;
		Vector3 val = V3Tools.TransformPointUnscaled(target, defaultTargetRotRelToMuscleInverse * rigidbody.centerOfMass);
		if (num > 0f && !ignoreTargetVelocity)
		{
			targetVelocity = (val - targetAnimatedCenterOfMass) / num;
		}
		targetAnimatedCenterOfMass = val;
		targetAnimatedPosition = target.position;
		targetAnimatedWorldRotation = target.rotation;
		if ((Object)(object)additionalPin != (Object)null)
		{
			Vector3 val2 = V3Tools.TransformPointUnscaled(additionalPinTarget, additionalRigidbody.centerOfMass);
			if (num > 0f && !ignoreTargetVelocity)
			{
				additionalTargetVelocity = (val2 - additionalPinTargetAnimatedCenterOfMass) / num;
			}
			additionalPinTargetAnimatedCenterOfMass = val2;
		}
		if ((Object)(object)((Joint)joint).connectedBody != (Object)null)
		{
			targetAnimatedRotation = targetLocalRotation * localRotationConvert;
		}
	}

	public void ClearVelocities()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		targetVelocity = Vector3.zero;
		mappedVelocity = Vector3.zero;
		mappedAngularVelocity = Vector3.zero;
		additionalTargetVelocity = Vector3.zero;
		targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
		targetAnimatedPosition = target.position;
		targetAnimatedWorldRotation = target.rotation;
		lastMappedPosition = target.position;
		lastMappedRotation = target.rotation;
		if ((Object)(object)additionalPin != (Object)null)
		{
			additionalPinTargetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(additionalPinTarget, additionalRigidbody.centerOfMass);
		}
	}

	public void UpdateAnchor(bool supportTranslationAnimation)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Joint)joint).connectedBody == (Object)null) && !((Object)(object)connectedBodyTarget == (Object)null) && (!directTargetParent || supportTranslationAnimation) && !state.isDisconnected)
		{
			Vector3 val2 = (((Joint)joint).connectedAnchor = InverseTransformPointUnscaled(connectedBodyTarget.position, connectedBodyTarget.rotation * toParentSpace, target.position));
			Vector3 val3 = val2;
			float num = 1f / connectedBodyTransform.lossyScale.x;
			((Joint)joint).connectedAnchor = val3 * num;
		}
	}

	public virtual void Update(float pinWeightMaster, float muscleWeightMaster, float muscleSpring, float muscleDamper, float pinPow, float pinDistanceFalloff, bool rotationTargetChanged, bool angularPinning, float deltaTime)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		state.velocity = rigidbody.velocity;
		state.angularVelocity = rigidbody.angularVelocity;
		if (state.isDisconnected)
		{
			state.pinWeightMlp = 0f;
			state.muscleWeightMlp = 0f;
			state.muscleDamperAdd = 0f;
			state.muscleDamperMlp = 0f;
			state.mappingWeightMlp = 0f;
			state.maxForceMlp = 0f;
			state.immunity = 0f;
			state.impulseMlp = 1f;
		}
		props.Clamp();
		state.Clamp();
		Pin(pinWeightMaster, pinPow, pinDistanceFalloff, angularPinning, deltaTime);
		if (rotationTargetChanged)
		{
			MuscleRotation(muscleWeightMaster, muscleSpring, muscleDamper);
		}
		if (!rigidbody.IsSleeping())
		{
			currMuscleTimer += Time.deltaTime;
			if (currMuscleTimer >= TimeToSleep)
			{
				rigidbody.Sleep();
				currMuscleTimer = 0f;
			}
		}
		else
		{
			currMuscleTimer = 0f;
		}
	}

	public void StoreTargetMappedPosition()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		targetMappedPosition = target.position;
	}

	public void StoreTargetMappedRotation()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		targetMappedRotation = target.rotation;
	}

	public void Map(float mappingWeightMaster)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		float num = props.mappingWeight * mappingWeightMaster * state.mappingWeightMlp;
		if (num <= 0f)
		{
			return;
		}
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		Vector3 val = position;
		Quaternion val2 = rotation;
		if (num >= 1f)
		{
			val2 = rotation * targetRotationRelative;
			val = position;
			if ((Object)(object)connectedBodyTransform != (Object)null)
			{
				Vector3 val3 = InverseTransformPointUnscaled(connectedBodyTransform.position, connectedBodyTransform.rotation, position);
				val = connectedBodyTarget.position + connectedBodyTarget.rotation * val3;
			}
			target.SetPositionAndRotation(val, val2);
		}
		else
		{
			val2 = Quaternion.Lerp(target.rotation, rotation * targetRotationRelative, num);
			if ((Object)(object)connectedBodyTransform != (Object)null)
			{
				Vector3 val4 = InverseTransformPointUnscaled(connectedBodyTransform.position, connectedBodyTransform.rotation, position);
				val = Vector3.Lerp(target.position, connectedBodyTarget.position + connectedBodyTarget.rotation * val4, num);
			}
			else
			{
				val = Vector3.Lerp(target.position, position, num);
			}
			target.SetPositionAndRotation(val, val2);
		}
	}

	public void CalculateMappedVelocity()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time - lastWriteTime;
		if (num > 0f)
		{
			mappedVelocity = (target.position - lastMappedPosition) / num;
			mappedAngularVelocity = PhysXTools.GetAngularVelocity(lastMappedRotation, target.rotation, num);
			lastWriteTime = Time.time;
		}
		lastMappedPosition = target.position;
		lastMappedRotation = target.rotation;
	}

	public void MapDisconnected()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (state.isDisconnected && !isPropMuscle)
		{
			target.position = transform.TransformPoint(defaultTargetPosRelToMuscle);
			target.rotation = transform.rotation * defaultTargetRotRelToMuscle;
			TargetChild[] array = targetChildren;
			foreach (TargetChild targetChild in array)
			{
				targetChild.Map();
			}
		}
	}

	private void Pin(float pinWeightMaster, float pinPow, float pinDistanceFalloff, bool angularPinning, float deltaTime)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		positionOffset = targetAnimatedCenterOfMass - rigidbody.worldCenterOfMass;
		if (float.IsNaN(positionOffset.x))
		{
			positionOffset = Vector3.zero;
		}
		float num = pinWeightMaster * props.pinWeight * state.pinWeightMlp;
		if (num <= 0f)
		{
			return;
		}
		num = Mathf.Pow(num, pinPow);
		Pin(rigidbody, positionOffset, targetVelocity, num, pinDistanceFalloff, deltaTime);
		if (angularPinning)
		{
			Vector3 angularAcceleration = PhysXTools.GetAngularAcceleration(rigidbody.rotation, defaultMuscleRotRelToTarget * targetAnimatedWorldRotation);
			angularAcceleration -= rigidbody.angularVelocity;
			angularAcceleration *= num;
			rigidbody.AddTorque(angularAcceleration, (ForceMode)2);
		}
		if ((Object)(object)additionalPin != (Object)null)
		{
			Vector3 zero = Vector3.zero;
			zero = additionalPinTargetAnimatedCenterOfMass - additionalRigidbody.worldCenterOfMass;
			if (float.IsNaN(zero.x))
			{
				zero = Vector3.zero;
			}
			Pin(additionalRigidbody, zero, additionalTargetVelocity, num * additionalPinWeight, pinDistanceFalloff, deltaTime);
		}
	}

	private void Pin(Rigidbody r, Vector3 posOffset, Vector3 targetVel, float w, float pinDistanceFalloff, float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = posOffset;
		if (deltaTime > 0f)
		{
			val /= deltaTime;
		}
		if (ignoreTargetVelocity)
		{
			targetVel = Vector3.zero;
		}
		Vector3 val2 = -r.velocity + targetVel + val;
		if (r.useGravity)
		{
			val2 -= Physics.gravity * deltaTime;
		}
		val2 *= w;
		if (pinDistanceFalloff > 0f)
		{
			val2 /= 1f + posOffset.sqrMagnitude * pinDistanceFalloff;
		}
		r.AddForce(val2, (ForceMode)2);
	}

	private void MuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		float num = muscleWeightMaster * props.muscleWeight * muscleSpring * state.muscleWeightMlp * 10f;
		if ((Object)(object)((Joint)joint).connectedBody == (Object)null)
		{
			num = 0f;
		}
		else if (num > 0f)
		{
			joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);
		}
		float num2 = props.muscleDamper * muscleDamper * state.muscleDamperMlp + state.muscleDamperAdd;
		if (num != lastJointDriveRotationWeight || num2 != lastRotationDamper)
		{
			lastJointDriveRotationWeight = num;
			lastRotationDamper = num2;
			slerpDrive.positionSpring = num;
			slerpDrive.maximumForce = Mathf.Max(num, num2) * state.maxForceMlp;
			slerpDrive.positionDamper = num2;
			joint.slerpDrive = slerpDrive;
		}
	}

	public void SetMuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		float num = muscleWeightMaster * props.muscleWeight * muscleSpring * 10f;
		if ((Object)(object)((Joint)joint).connectedBody == (Object)null)
		{
			num = 0f;
		}
		else if (num > 0f)
		{
			joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);
		}
		float num2 = props.muscleDamper * muscleDamper;
		slerpDrive.positionSpring = num;
		slerpDrive.maximumForce = Mathf.Max(num, num2);
		slerpDrive.positionDamper = num2;
		joint.slerpDrive = slerpDrive;
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
