using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[HelpURL("http://root-motion.com/puppetmasterdox/html/page10.html")]
[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourPuppet")]
public class BehaviourPuppet : BehaviourBase
{
	[Serializable]
	public enum State
	{
		Puppet,
		Unpinned,
		GetUp
	}

	[Serializable]
	public enum NormalMode
	{
		Active,
		Unmapped,
		Kinematic
	}

	[Serializable]
	public class MasterProps
	{
		public NormalMode normalMode;

		public float mappingBlendSpeed = 10f;

		public bool activateOnStaticCollisions;

		public float activateOnImpulse = 0f;
	}

	[Serializable]
	public struct MuscleProps
	{
		[Tooltip("How much will collisions with muscles of this group unpin parent muscles?")]
		[Range(0f, 1f)]
		public float unpinParents;

		[Tooltip("How much will collisions with muscles of this group unpin child muscles?")]
		[Range(0f, 1f)]
		public float unpinChildren;

		[Tooltip("How much will collisions with muscles of this group unpin muscles of the same group?")]
		[Range(0f, 1f)]
		public float unpinGroup;

		[Tooltip("If 1, muscles of this group will always be mapped to the ragdoll.")]
		[Range(0f, 1f)]
		public float minMappingWeight;

		[Tooltip("If 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.")]
		[Range(0f, 1f)]
		public float maxMappingWeight;

		[Tooltip("Defines minimum pin weight for the muscles. Muscle pin weight can’t be reduced beyond this value when damage occurs from collisions.")]
		[Range(0f, 1f)]
		public float minPinWeight;

		[Tooltip("If true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).")]
		public bool disableColliders;

		[Tooltip("How fast will muscles of this group regain their pin weight (multiplier)?")]
		public float regainPinSpeed;

		[Tooltip("Smaller value means more unpinning from collisions (multiplier).")]
		public float collisionResistance;

		[Tooltip("If the distance from the muscle to its target is larger than this value, the character will be knocked out.")]
		public float knockOutDistance;

		[Tooltip("The PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.")]
		public PhysicMaterial puppetMaterial;

		[Tooltip("The PhysicsMaterial applied to the muscles while the character is in Unpinned state.")]
		public PhysicMaterial unpinnedMaterial;
	}

	[Serializable]
	public struct MusclePropsGroup
	{
		[HideInInspector]
		public string name;

		[Tooltip("Muscle groups to which those properties apply.")]
		public Muscle.Group[] groups;

		[Tooltip("The muscle properties for those muscle groups.")]
		public MuscleProps props;
	}

	[Serializable]
	public struct CollisionResistanceMultiplier
	{
		public LayerMask layers;

		[Tooltip("Multiplier for the 'Collision Resistance' for these layers.")]
		public float multiplier;

		[Tooltip("Overrides 'Collision Threshold' for these layers.")]
		public float collisionThreshold;
	}

	public delegate void CollisionImpulseDelegate(MuscleCollision m, float impulse);

	private const string typeSpring = "BehaviourPuppet";

	[LargeHeader("Collision And Recovery")]
	public MasterProps masterProps = new MasterProps();

	[Tooltip("Will ground the target to those layers when getting up.")]
	public LayerMask groundLayers;

	[Tooltip("Will unpin the muscles that collide with those layers.")]
	public LayerMask collisionLayers;

	[Tooltip("The collision impulse sqrMagnitude threshold under which collisions will be ignored.")]
	public float collisionThreshold;

	public Weight collisionResistance = new Weight(3f, "Smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.");

	[Tooltip("Multiplies collision resistance for the specified layers.")]
	public CollisionResistanceMultiplier[] collisionResistanceMultipliers;

	[Tooltip("An optimisation. Will only process up to this number of collisions per physics step.")]
	[Range(1f, 30f)]
	public int maxCollisions = 30;

	[Tooltip("How fast will the muscles of this group regain their pin weight?")]
	[Range(0.001f, 10f)]
	public float regainPinSpeed = 1f;

	[Tooltip("'Boosting' is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.")]
	public float boostFalloff = 1f;

	[LargeHeader("Muscle Group Properties")]
	[Tooltip("The default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.")]
	public MuscleProps defaults;

	[Tooltip("Overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).")]
	public MusclePropsGroup[] groupOverrides;

	[LargeHeader("Losing Balance")]
	[Tooltip("If the distance from the muscle to its target is larger than this value, the character will be knocked out.")]
	[Range(0.001f, 10f)]
	public float knockOutDistance = 1f;

	[Tooltip("Smaller value makes the muscles weaker when the puppet is knocked out.")]
	[Range(0f, 1f)]
	public float unpinnedMuscleWeightMlp = 0.3f;

	[Tooltip("Most character controllers apply supernatural accelerations to characters when changing running direction or jumping. It will require major pinning forces to be applied on the ragdoll to keep up with that acceleration. When a puppet collides with something at that point and is unpinned, those forces might shoot the puppet off to space. This variable limits the velocity of the ragdoll's Rigidbodies when the puppet is unpinned.")]
	public float maxRigidbodyVelocity = 10f;

	[Tooltip("If a muscle has drifted farther than 'Knock Out Distance', will only unpin the puppet if its pin weight is less than this value. Lowering this value will make puppets less likely to lose balance on minor collisions.")]
	[Range(0f, 1f)]
	public float pinWeightThreshold = 1f;

	[Tooltip("If false, will not unbalance the puppet by muscles that have their pin weight set to 0 in PuppetMaster muscle settings.")]
	public bool unpinnedMuscleKnockout = true;

	[Tooltip("If true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.")]
	public bool dropProps;

	[LargeHeader("Getting Up")]
	[Tooltip("If true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.")]
	public bool canGetUp = true;

	[Tooltip("Minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.")]
	public float getUpDelay = 5f;

	[Tooltip("The duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.")]
	public float blendToAnimationTime = 0.2f;

	[Tooltip("Will not get up before the velocity of the hip muscle has come down to this value.")]
	public float maxGetUpVelocity = 0.3f;

	[Tooltip("The duration of the 'GetUp' state after which it switches to the 'Puppetä state.")]
	public float minGetUpDuration = 1f;

	[Tooltip("Collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
	public float getUpCollisionResistanceMlp = 2f;

	[Tooltip("Regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
	public float getUpRegainPinSpeedMlp = 2f;

	[Tooltip("Knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
	public float getUpKnockOutDistanceMlp = 10f;

	[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.")]
	public Vector3 getUpOffsetProne;

	[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.")]
	public Vector3 getUpOffsetSupine;

	[Tooltip("If enabled, onGetUpProne will be called when laying on the right side and onGetUpSupine when on the left side.")]
	public bool isQuadruped;

	[LargeHeader("Events")]
	[Tooltip("Called when the character starts getting up from a prone pose (facing down) or from the right side when 'Is Quadruped' is enabled.")]
	public PuppetEvent onGetUpProne;

	[Tooltip("Called when the character starts getting up from a supine pose (facing up) or from the left side when 'Is Quadruped' is enabled.")]
	public PuppetEvent onGetUpSupine;

	[Tooltip("Called when the character is knocked out (loses balance). Doesn't matter from which state.")]
	public PuppetEvent onLoseBalance;

	[Tooltip("Called when the character is knocked out (loses balance) only from the normal Puppet state.")]
	public PuppetEvent onLoseBalanceFromPuppet;

	[Tooltip("Called when the character is knocked out (loses balance) only from the GetUp state.")]
	public PuppetEvent onLoseBalanceFromGetUp;

	[Tooltip("Called when the character has fully recovered and switched to the Puppet state.")]
	public PuppetEvent onRegainBalance;

	public CollisionDelegate OnCollision;

	public CollisionImpulseDelegate OnCollisionImpulse;

	[HideInInspector]
	public bool canMoveTarget = true;

	private float unpinnedTimer;

	private float getUpTimer;

	private Vector3 hipsForward;

	private Vector3 hipsUp;

	private float getupAnimationBlendWeight;

	private bool getUpTargetFixed;

	private NormalMode lastNormalMode;

	private int collisions;

	private bool eventsEnabled;

	private float lastKnockOutDistance;

	private float knockOutDistanceSqr;

	private bool getupDisabled;

	private bool hasCollidedSinceGetUp;

	private float lastCollisionTime;

	private bool hasBoosted;

	private MuscleCollisionBroadcaster broadcaster;

	private bool dropPropFlag;

	public State state { get; private set; }

	public Vector3 platformVelocity { get; set; }

	public Vector3 getUpPosition { get; set; }

	protected override string GetTypeSpring()
	{
		return "BehaviourPuppet";
	}

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_behaviour_puppet.html");
	}

	public override void OnReactivate()
	{
		state = ((puppetMaster.state != 0) ? State.Unpinned : State.Puppet);
		getUpTimer = 0f;
		unpinnedTimer = 0f;
		getupAnimationBlendWeight = 0f;
		getUpTargetFixed = false;
		getupDisabled = puppetMaster.state != PuppetMaster.State.Alive;
		state = ((puppetMaster.state != 0) ? State.Unpinned : State.Puppet);
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if (!muscle.state.isDisconnected)
			{
				SetColliders(muscle, state == State.Unpinned);
			}
		}
		((Behaviour)this).enabled = true;
	}

	public void Reset(Vector3 position, Quaternion rotation)
	{
	}

	public void SetHasCollided(bool to)
	{
		hasCollidedSinceGetUp = to;
	}

	public override void OnTeleport(Quaternion deltaRotation, Vector3 deltaPosition, Vector3 pivot, bool moveToTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		getUpPosition = pivot + deltaRotation * (getUpPosition - pivot) + deltaPosition;
		if (moveToTarget)
		{
			getupAnimationBlendWeight = 0f;
		}
	}

	protected override void OnInitiate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		CollisionResistanceMultiplier[] array = collisionResistanceMultipliers;
		for (int i = 0; i < array.Length; i++)
		{
			CollisionResistanceMultiplier collisionResistanceMultiplier = array[i];
			if (collisionResistanceMultiplier.layers == 0)
			{
			}
		}
		int num = 0;
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if (((Component)muscle.joint).gameObject.layer == ((Component)puppetMaster.targetRoot).gameObject.layer)
			{
			}
			if (!Physics.GetIgnoreLayerCollision(((Component)muscle.joint).gameObject.layer, ((Component)puppetMaster.targetRoot).gameObject.layer))
			{
			}
			if (muscle.props.group == Muscle.Group.Hips)
			{
				num++;
			}
			if (num > 1)
			{
			}
		}
		hipsForward = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.forward;
		hipsUp = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.up;
		state = State.Unpinned;
		eventsEnabled = true;
	}

	protected override void OnActivate()
	{
		bool flag = true;
		if (puppetMaster.pinWeight > 0f)
		{
			Muscle[] muscles = puppetMaster.muscles;
			foreach (Muscle muscle in muscles)
			{
				if (muscle.state.pinWeightMlp > 0.5f)
				{
					flag = false;
					break;
				}
			}
		}
		bool flag2 = eventsEnabled;
		eventsEnabled = false;
		if (flag)
		{
			SetState(State.Unpinned);
		}
		else
		{
			SetState(State.Puppet);
		}
		eventsEnabled = flag2;
	}

	public override void KillStart()
	{
		getupDisabled = true;
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if (!muscle.state.isDisconnected)
			{
				muscle.state.pinWeightMlp = 0f;
				if (hasBoosted)
				{
					muscle.state.immunity = 0f;
				}
				SetColliders(muscle, unpinned: true);
			}
		}
	}

	public override void KillEnd()
	{
		SetState(State.Unpinned);
	}

	public override void Resurrect()
	{
		getupDisabled = false;
		if (state == State.Unpinned)
		{
			getUpTimer = float.PositiveInfinity;
			unpinnedTimer = float.PositiveInfinity;
			getupAnimationBlendWeight = 0f;
			Muscle[] muscles = puppetMaster.muscles;
			foreach (Muscle muscle in muscles)
			{
				muscle.state.pinWeightMlp = 0f;
			}
		}
	}

	protected override void OnDeactivate()
	{
		state = State.Unpinned;
	}

	protected override void OnFixedUpdate(float deltaTime)
	{
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		collisions = 0;
		if (dropPropFlag)
		{
			RemovePropMuscles();
			PropMuscle[] propMuscles = puppetMaster.propMuscles;
			foreach (PropMuscle propMuscle in propMuscles)
			{
				propMuscle.currentProp = null;
			}
			dropPropFlag = false;
		}
		if (!puppetMaster.isActive && !puppetMaster.isSwitchingMode)
		{
			SetState(State.Puppet);
			return;
		}
		if (!puppetMaster.isAlive)
		{
			Muscle[] muscles = puppetMaster.muscles;
			foreach (Muscle muscle in muscles)
			{
				if (!muscle.state.isDisconnected)
				{
					muscle.state.pinWeightMlp = 0f;
					muscle.state.mappingWeightMlp = Mathf.MoveTowards(muscle.state.mappingWeightMlp, 1f, deltaTime * 5f);
				}
			}
			return;
		}
		if (hasBoosted)
		{
			Muscle[] muscles2 = puppetMaster.muscles;
			foreach (Muscle muscle2 in muscles2)
			{
				if (!muscle2.state.isDisconnected)
				{
					muscle2.state.immunity = Mathf.MoveTowards(muscle2.state.immunity, 0f, deltaTime * boostFalloff);
					muscle2.state.impulseMlp = Mathf.Lerp(muscle2.state.impulseMlp, 1f, deltaTime * boostFalloff);
				}
			}
		}
		if (state == State.Unpinned)
		{
			unpinnedTimer += deltaTime;
			if (unpinnedTimer >= getUpDelay && canGetUp && !getupDisabled)
			{
				Vector3 val = puppetMaster.muscles[0].rigidbody.velocity - platformVelocity;
				if (val.sqrMagnitude < maxGetUpVelocity * maxGetUpVelocity)
				{
					SetState(State.GetUp);
					return;
				}
			}
			Muscle[] muscles3 = puppetMaster.muscles;
			foreach (Muscle muscle3 in muscles3)
			{
				if (!muscle3.state.isDisconnected)
				{
					muscle3.state.pinWeightMlp = 0f;
					muscle3.state.mappingWeightMlp = Mathf.MoveTowards(muscle3.state.mappingWeightMlp, 1f, deltaTime * masterProps.mappingBlendSpeed);
				}
			}
		}
		if (hasCollidedSinceGetUp && Time.time > lastCollisionTime + 3f)
		{
			hasCollidedSinceGetUp = false;
		}
		if (state != State.Unpinned && !puppetMaster.isKilling)
		{
			if (knockOutDistance != lastKnockOutDistance)
			{
				knockOutDistanceSqr = Mathf.Sqrt(knockOutDistance);
				lastKnockOutDistance = knockOutDistance;
			}
			Muscle[] muscles4 = puppetMaster.muscles;
			foreach (Muscle muscle4 in muscles4)
			{
				MuscleProps props = GetProps(muscle4.props.group);
				float num = 1f;
				if (state == State.GetUp)
				{
					num = Mathf.Lerp(getUpKnockOutDistanceMlp, num, muscle4.state.pinWeightMlp);
				}
				float num2 = (unpinnedMuscleKnockout ? muscle4.positionOffset.sqrMagnitude : (muscle4.positionOffset.sqrMagnitude * muscle4.props.pinWeight * puppetMaster.pinWeight));
				if (puppetMaster.pinWeight < 1f)
				{
					hasCollidedSinceGetUp = true;
					lastCollisionTime = Time.time;
				}
				float num3 = muscle4.state.pinWeightMlp * muscle4.props.pinWeight * puppetMaster.pinWeight;
				if (hasCollidedSinceGetUp && !muscle4.state.isDisconnected && !puppetMaster.isBlending && num2 > 0f && num3 <= pinWeightThreshold && num2 > props.knockOutDistance * knockOutDistanceSqr * num)
				{
					if (state != State.GetUp || getUpTargetFixed)
					{
						SetState(State.Unpinned);
					}
					return;
				}
				if (muscle4.state.isDisconnected)
				{
					continue;
				}
				muscle4.state.muscleWeightMlp = Mathf.Lerp(unpinnedMuscleWeightMlp, 1f, muscle4.state.pinWeightMlp);
				if (state == State.GetUp)
				{
					muscle4.state.muscleDamperAdd = 0f;
				}
				if (!puppetMaster.isKilling)
				{
					float num4 = 1f;
					if (state == State.GetUp)
					{
						num4 = Mathf.Lerp(getUpRegainPinSpeedMlp, 1f, muscle4.state.pinWeightMlp);
					}
					muscle4.state.pinWeightMlp += deltaTime * props.regainPinSpeed * regainPinSpeed * num4;
				}
			}
			float num5 = 1f;
			Muscle[] muscles5 = puppetMaster.muscles;
			foreach (Muscle muscle5 in muscles5)
			{
				if ((muscle5.props.group == Muscle.Group.Leg || muscle5.props.group == Muscle.Group.Foot) && !muscle5.state.isDisconnected && muscle5.state.pinWeightMlp < num5)
				{
					num5 = muscle5.state.pinWeightMlp;
				}
			}
			Muscle[] muscles6 = puppetMaster.muscles;
			foreach (Muscle muscle6 in muscles6)
			{
				muscle6.state.pinWeightMlp = Mathf.Clamp(muscle6.state.pinWeightMlp, 0f, num5 * 5f);
			}
		}
		if (state == State.GetUp)
		{
			getUpTimer += deltaTime;
			if (getUpTimer > minGetUpDuration)
			{
				getUpTimer = 0f;
				SetState(State.Puppet);
			}
		}
	}

	protected override void OnLateUpdate(float deltaTime)
	{
		base.forceActive = state != State.Puppet;
		if (!puppetMaster.isAlive)
		{
			return;
		}
		if (masterProps.normalMode != lastNormalMode)
		{
			if (lastNormalMode == NormalMode.Unmapped)
			{
				Muscle[] muscles = puppetMaster.muscles;
				foreach (Muscle muscle in muscles)
				{
					muscle.state.mappingWeightMlp = 1f;
				}
			}
			if (lastNormalMode == NormalMode.Kinematic && puppetMaster.mode == PuppetMaster.Mode.Kinematic)
			{
				puppetMaster.mode = PuppetMaster.Mode.Active;
			}
			lastNormalMode = masterProps.normalMode;
		}
		switch (masterProps.normalMode)
		{
		case NormalMode.Unmapped:
			if (puppetMaster.isActive)
			{
				bool to = puppetMaster.pinWeight < 1f;
				for (int j = 0; j < puppetMaster.muscles.Length; j++)
				{
					BlendMuscleMapping(j, ref to, deltaTime);
				}
			}
			break;
		case NormalMode.Kinematic:
			if (SetKinematic())
			{
				puppetMaster.mode = PuppetMaster.Mode.Kinematic;
			}
			break;
		}
	}

	private bool SetKinematic()
	{
		if (!puppetMaster.isActive)
		{
			return false;
		}
		if (state != 0)
		{
			return false;
		}
		if (puppetMaster.isBlending)
		{
			return false;
		}
		if (getupAnimationBlendWeight > 0f)
		{
			return false;
		}
		if (!puppetMaster.isAlive)
		{
			return false;
		}
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if (!muscle.state.isDisconnected && muscle.state.pinWeightMlp < 1f)
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnReadBehaviour(float deltaTime)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (!puppetMaster.isFrozen && state == State.Unpinned && puppetMaster.isActive && !puppetMaster.isBlending && !puppetMaster.muscles[0].state.isDisconnected && puppetMaster.muscles[0].state.mappingWeightMlp >= 1f)
		{
			MoveTarget(puppetMaster.muscles[0].rigidbody.position);
			GroundTarget(groundLayers);
			getUpPosition = puppetMaster.targetRoot.position;
		}
		if ((state == State.GetUp && getUpTimer < minGetUpDuration * 0.1f) || getupAnimationBlendWeight > 0f)
		{
			Vector3 val = Vector3.Project(puppetMaster.targetRoot.position - getUpPosition, puppetMaster.targetRoot.up);
			getUpPosition += val;
			getUpPosition += platformVelocity * deltaTime;
			MoveTarget(getUpPosition);
		}
		if (getupAnimationBlendWeight > 0f)
		{
			getupAnimationBlendWeight = Mathf.MoveTowards(getupAnimationBlendWeight, 0f, deltaTime);
			if (getupAnimationBlendWeight < 0.01f)
			{
				getupAnimationBlendWeight = 0f;
			}
			Muscle obj = puppetMaster.muscles[0];
			obj.targetSampledPosition += platformVelocity * deltaTime;
			puppetMaster.FixTargetToSampledState(Interp.Float(getupAnimationBlendWeight, InterpolationMode.InOutCubic));
		}
		getUpTargetFixed = true;
	}

	private void BlendMuscleMapping(int muscleIndex, ref bool to, float deltaTime)
	{
		if (puppetMaster.muscles[muscleIndex].state.pinWeightMlp < 1f)
		{
			to = true;
		}
		MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
		float num = ((!to) ? props.minMappingWeight : ((state == State.Puppet) ? props.maxMappingWeight : 1f));
		puppetMaster.muscles[muscleIndex].state.mappingWeightMlp = Mathf.MoveTowards(puppetMaster.muscles[muscleIndex].state.mappingWeightMlp, num, deltaTime * masterProps.mappingBlendSpeed);
	}

	public override void OnMuscleAdded(Muscle m)
	{
		base.OnMuscleAdded(m);
		SetColliders(m, state == State.Unpinned);
	}

	public override void OnMuscleRemoved(Muscle m)
	{
		base.OnMuscleRemoved(m);
		SetColliders(m, unpinned: true);
	}

	protected void MoveTarget(Vector3 position)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (canMoveTarget)
		{
			puppetMaster.targetRoot.position = position;
		}
	}

	protected void RotateTarget(Quaternion rotation)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (canMoveTarget)
		{
			puppetMaster.targetRoot.rotation = rotation;
		}
	}

	protected override void GroundTarget(LayerMask layers)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (canMoveTarget)
		{
			base.GroundTarget(layers);
		}
	}

	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < groupOverrides.Length; i++)
		{
			groupOverrides[i].name = string.Empty;
			if (groupOverrides[i].groups.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < groupOverrides[i].groups.Length; j++)
			{
				if (j > 0)
				{
					groupOverrides[i].name += ", ";
				}
				groupOverrides[i].name += groupOverrides[i].groups[j];
			}
		}
	}

	public void Boost(float immunity, float impulseMlp)
	{
		hasBoosted = true;
		for (int i = 0; i < puppetMaster.muscles.Length; i++)
		{
			Boost(i, immunity, impulseMlp);
		}
	}

	public void Boost(int muscleIndex, float immunity, float impulseMlp)
	{
		hasBoosted = true;
		BoostImmunity(muscleIndex, immunity);
		BoostImpulseMlp(muscleIndex, impulseMlp);
	}

	public void Boost(int muscleIndex, float immunity, float impulseMlp, float boostParents, float boostChildren)
	{
		hasBoosted = true;
		if (boostParents <= 0f && boostChildren <= 0f)
		{
			Boost(muscleIndex, immunity, impulseMlp);
			return;
		}
		for (int i = 0; i < puppetMaster.muscles.Length; i++)
		{
			float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
			Boost(i, immunity * falloff, impulseMlp * falloff);
		}
	}

	public void BoostImmunity(float immunity)
	{
		hasBoosted = true;
		if (!(immunity < 0f))
		{
			for (int i = 0; i < puppetMaster.muscles.Length; i++)
			{
				BoostImmunity(i, immunity);
			}
		}
	}

	public void BoostImmunity(int muscleIndex, float immunity)
	{
		hasBoosted = true;
		puppetMaster.muscles[muscleIndex].state.immunity = Mathf.Clamp(immunity, puppetMaster.muscles[muscleIndex].state.immunity, 1f);
	}

	public void BoostImmunity(int muscleIndex, float immunity, float boostParents, float boostChildren)
	{
		hasBoosted = true;
		for (int i = 0; i < puppetMaster.muscles.Length; i++)
		{
			float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
			BoostImmunity(i, immunity * falloff);
		}
	}

	public void BoostImpulseMlp(float impulseMlp)
	{
		hasBoosted = true;
		for (int i = 0; i < puppetMaster.muscles.Length; i++)
		{
			BoostImpulseMlp(i, impulseMlp);
		}
	}

	public void BoostImpulseMlp(int muscleIndex, float impulseMlp)
	{
		hasBoosted = true;
		puppetMaster.muscles[muscleIndex].state.impulseMlp = Mathf.Max(impulseMlp, puppetMaster.muscles[muscleIndex].state.impulseMlp);
	}

	public void BoostImpulseMlp(int muscleIndex, float impulseMlp, float boostParents, float boostChildren)
	{
		hasBoosted = true;
		for (int i = 0; i < puppetMaster.muscles.Length; i++)
		{
			float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
			BoostImpulseMlp(i, impulseMlp * falloff);
		}
	}

	public void Unpin()
	{
		SetState(State.Unpinned);
	}

	protected override void OnMuscleHitBehaviour(MuscleHit hit)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (masterProps.normalMode == NormalMode.Kinematic)
		{
			puppetMaster.mode = PuppetMaster.Mode.Active;
		}
		UnPin(hit.muscleIndex, hit.unPin);
		puppetMaster.muscles[hit.muscleIndex].SetKinematic(to: false);
		puppetMaster.muscles[hit.muscleIndex].rigidbody.AddForceAtPosition(hit.force, hit.position);
	}

	protected override void OnMuscleCollisionBehaviour(MuscleCollision m)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (OnCollision != null)
		{
			OnCollision(m);
		}
		if (!((Behaviour)this).enabled || state == State.Unpinned || collisions > maxCollisions || !LayerMaskExtensions.Contains(collisionLayers, ((Component)m.collision.collider).gameObject.layer) || (LayerMaskExtensions.Contains(groundLayers, ((Component)m.collision.collider).gameObject.layer) && (state == State.GetUp || puppetMaster.muscles[m.muscleIndex].props.group == Muscle.Group.Foot)) || (masterProps.normalMode == NormalMode.Kinematic && !puppetMaster.isActive && !masterProps.activateOnStaticCollisions && m.collision.gameObject.isStatic))
		{
			return;
		}
		if (puppetMaster.muscles[m.muscleIndex].rigidbody.isKinematic && (Object)(object)m.collision.collider.attachedRigidbody != (Object)null && m.collision.collider.attachedRigidbody.isKinematic)
		{
			if (masterProps.normalMode == NormalMode.Kinematic && puppetMaster.mode == PuppetMaster.Mode.Kinematic)
			{
				puppetMaster.mode = PuppetMaster.Mode.Active;
			}
			return;
		}
		float layerThreshold = collisionThreshold;
		float num = GetImpulse(m, ref layerThreshold);
		float num2 = (((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null) ? (1f + (float)Singleton<PuppetMasterSettings>.instance.currentlyActivePuppets * Singleton<PuppetMasterSettings>.instance.activePuppetCollisionThresholdMlp) : 1f);
		float num3 = layerThreshold * num2;
		if (num <= num3)
		{
			return;
		}
		collisions++;
		if ((Object)(object)m.collision.collider.attachedRigidbody != (Object)null)
		{
			broadcaster = ((Component)m.collision.collider.attachedRigidbody).GetComponent<MuscleCollisionBroadcaster>();
			if ((Object)(object)broadcaster != (Object)null && broadcaster.muscleIndex < broadcaster.puppetMaster.muscles.Length)
			{
				num *= broadcaster.puppetMaster.muscles[broadcaster.muscleIndex].state.impulseMlp;
			}
		}
		if (OnCollisionImpulse != null)
		{
			OnCollisionImpulse(m, num);
		}
		if (Activate(m.collision, num))
		{
			puppetMaster.mode = PuppetMaster.Mode.Active;
		}
		UnPin(m.muscleIndex, num);
	}

	private float GetImpulse(MuscleCollision m, ref float layerThreshold)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 impulse = m.collision.impulse;
		float sqrMagnitude = impulse.sqrMagnitude;
		sqrMagnitude /= puppetMaster.muscles[m.muscleIndex].rigidbody.mass;
		sqrMagnitude *= 0.3f;
		CollisionResistanceMultiplier[] array = collisionResistanceMultipliers;
		for (int i = 0; i < array.Length; i++)
		{
			CollisionResistanceMultiplier collisionResistanceMultiplier = array[i];
			if (LayerMaskExtensions.Contains(collisionResistanceMultiplier.layers, ((Component)m.collision.collider).gameObject.layer))
			{
				sqrMagnitude = ((!(collisionResistanceMultiplier.multiplier <= 0f)) ? (sqrMagnitude / collisionResistanceMultiplier.multiplier) : float.PositiveInfinity);
				layerThreshold = collisionResistanceMultiplier.collisionThreshold;
				break;
			}
		}
		return sqrMagnitude;
	}

	public void UnPin(int muscleIndex, float unpin)
	{
		if (muscleIndex < puppetMaster.muscles.Length)
		{
			MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
			for (int i = 0; i < puppetMaster.muscles.Length; i++)
			{
				UnPinMuscle(i, unpin * GetFalloff(i, muscleIndex, props.unpinParents, props.unpinChildren, props.unpinGroup));
			}
			hasCollidedSinceGetUp = true;
			lastCollisionTime = Time.time;
		}
	}

	private void UnPinMuscle(int muscleIndex, float unpin)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!(unpin <= 0f) && !(puppetMaster.muscles[muscleIndex].state.immunity >= 1f))
		{
			MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
			float num = 1f;
			if (state == State.GetUp)
			{
				num = Mathf.Lerp(getUpCollisionResistanceMlp, 1f, puppetMaster.muscles[muscleIndex].state.pinWeightMlp);
			}
			float num2;
			if (collisionResistance.mode != 0)
			{
				Weight weight = collisionResistance;
				Vector3 targetVelocity = puppetMaster.muscles[muscleIndex].targetVelocity;
				num2 = weight.GetValue(targetVelocity.magnitude);
			}
			else
			{
				num2 = collisionResistance.floatValue;
			}
			float num3 = num2;
			float num4 = unpin / (props.collisionResistance * num3 * num);
			num4 *= 1f - puppetMaster.muscles[muscleIndex].state.immunity;
			if (!puppetMaster.muscles[muscleIndex].state.isDisconnected)
			{
				puppetMaster.muscles[muscleIndex].state.pinWeightMlp = Mathf.Max(puppetMaster.muscles[muscleIndex].state.pinWeightMlp - num4, props.minPinWeight);
			}
		}
	}

	private bool Activate(Collision collision, float impulse)
	{
		if (masterProps.normalMode != NormalMode.Kinematic)
		{
			return false;
		}
		if (puppetMaster.mode != PuppetMaster.Mode.Kinematic)
		{
			return false;
		}
		if (impulse < masterProps.activateOnImpulse)
		{
			return false;
		}
		if (collision.gameObject.isStatic)
		{
			return masterProps.activateOnStaticCollisions;
		}
		return true;
	}

	public bool IsProne()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (isQuadruped)
		{
			float num = Vector3.Dot(puppetMaster.muscles[0].transform.rotation * hipsUp, puppetMaster.targetRoot.right);
			return num > 0f;
		}
		float num2 = Vector3.Dot(puppetMaster.muscles[0].transform.rotation * hipsForward, puppetMaster.targetRoot.up);
		return num2 < 0f;
	}

	private float GetFalloff(int i, int muscleIndex, float falloffParents, float falloffChildren)
	{
		if (i == muscleIndex)
		{
			return 1f;
		}
		bool flag = puppetMaster.muscles[muscleIndex].childFlags[i];
		int num = puppetMaster.muscles[muscleIndex].kinshipDegrees[i];
		return Mathf.Pow(flag ? falloffChildren : falloffParents, (float)num);
	}

	private float GetFalloff(int i, int muscleIndex, float falloffParents, float falloffChildren, float falloffGroup)
	{
		float num = GetFalloff(i, muscleIndex, falloffParents, falloffChildren);
		if (falloffGroup > 0f && i != muscleIndex && InGroup(puppetMaster.muscles[i].props.group, puppetMaster.muscles[muscleIndex].props.group))
		{
			num = Mathf.Max(num, falloffGroup);
		}
		return num;
	}

	private bool InGroup(Muscle.Group group1, Muscle.Group group2)
	{
		if (group1 == group2)
		{
			return true;
		}
		MusclePropsGroup[] array = groupOverrides;
		for (int i = 0; i < array.Length; i++)
		{
			MusclePropsGroup musclePropsGroup = array[i];
			Muscle.Group[] groups = musclePropsGroup.groups;
			foreach (Muscle.Group group3 in groups)
			{
				if (group3 != group1)
				{
					continue;
				}
				Muscle.Group[] groups2 = musclePropsGroup.groups;
				foreach (Muscle.Group group4 in groups2)
				{
					if (group4 == group2)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private MuscleProps GetProps(Muscle.Group group)
	{
		MusclePropsGroup[] array = groupOverrides;
		for (int i = 0; i < array.Length; i++)
		{
			MusclePropsGroup musclePropsGroup = array[i];
			Muscle.Group[] groups = musclePropsGroup.groups;
			foreach (Muscle.Group group2 in groups)
			{
				if (group2 == group)
				{
					return musclePropsGroup.props;
				}
			}
		}
		return defaults;
	}

	public void SetState(State newState)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		if (state == newState)
		{
			return;
		}
		switch (newState)
		{
		case State.Puppet:
			puppetMaster.SampleTargetMappedState();
			unpinnedTimer = 0f;
			getUpTimer = 0f;
			hasCollidedSinceGetUp = false;
			if (state == State.Unpinned)
			{
				Muscle[] muscles2 = puppetMaster.muscles;
				foreach (Muscle muscle2 in muscles2)
				{
					if (!muscle2.state.isDisconnected)
					{
						muscle2.state.pinWeightMlp = 1f;
						muscle2.state.muscleWeightMlp = 1f;
						muscle2.state.muscleDamperAdd = 0f;
						muscle2.positionOffset = Vector3.zero;
						SetColliders(muscle2, unpinned: false);
					}
				}
			}
			state = State.Puppet;
			if (eventsEnabled)
			{
				onRegainBalance.Trigger(puppetMaster);
				if (onRegainBalance.switchBehaviour)
				{
					return;
				}
			}
			break;
		case State.Unpinned:
		{
			unpinnedTimer = 0f;
			getUpTimer = 0f;
			getupAnimationBlendWeight = 0f;
			Muscle[] muscles3 = puppetMaster.muscles;
			foreach (Muscle muscle3 in muscles3)
			{
				if (hasBoosted)
				{
					muscle3.state.immunity = 0f;
				}
				if (maxRigidbodyVelocity != float.PositiveInfinity)
				{
					muscle3.rigidbody.velocity = Vector3.ClampMagnitude(muscle3.rigidbody.velocity, maxRigidbodyVelocity);
					muscle3.mappedVelocity = Vector3.ClampMagnitude(muscle3.mappedVelocity, maxRigidbodyVelocity);
				}
				SetColliders(muscle3, unpinned: true);
			}
			if (dropProps)
			{
				dropPropFlag = true;
			}
			Muscle[] muscles4 = puppetMaster.muscles;
			foreach (Muscle muscle4 in muscles4)
			{
				if (!muscle4.state.isDisconnected)
				{
					muscle4.state.muscleWeightMlp = (puppetMaster.isAlive ? unpinnedMuscleWeightMlp : puppetMaster.stateSettings.deadMuscleWeight);
				}
			}
			onLoseBalance.Trigger(puppetMaster, puppetMaster.isAlive);
			if (onLoseBalance.switchBehaviour)
			{
				state = State.Unpinned;
				return;
			}
			if (state == State.Puppet)
			{
				onLoseBalanceFromPuppet.Trigger(puppetMaster, puppetMaster.isAlive);
				if (onLoseBalanceFromPuppet.switchBehaviour)
				{
					state = State.Unpinned;
					return;
				}
			}
			else
			{
				onLoseBalanceFromGetUp.Trigger(puppetMaster, puppetMaster.isAlive);
				if (onLoseBalanceFromGetUp.switchBehaviour)
				{
					state = State.Unpinned;
					return;
				}
			}
			Muscle[] muscles5 = puppetMaster.muscles;
			foreach (Muscle muscle5 in muscles5)
			{
				muscle5.state.pinWeightMlp = 0f;
			}
			break;
		}
		case State.GetUp:
		{
			unpinnedTimer = 0f;
			getUpTimer = 0f;
			hasCollidedSinceGetUp = false;
			bool flag = IsProne();
			state = State.GetUp;
			if (flag)
			{
				onGetUpProne.Trigger(puppetMaster);
				if (onGetUpProne.switchBehaviour)
				{
					return;
				}
			}
			else
			{
				onGetUpSupine.Trigger(puppetMaster);
				if (onGetUpSupine.switchBehaviour)
				{
					return;
				}
			}
			Muscle[] muscles = puppetMaster.muscles;
			foreach (Muscle muscle in muscles)
			{
				if (!muscle.state.isDisconnected)
				{
					SetColliders(muscle, unpinned: false);
				}
			}
			if (isQuadruped)
			{
				Vector3 val = puppetMaster.muscles[0].rigidbody.rotation * hipsForward;
				Vector3 up = puppetMaster.targetRoot.up;
				Vector3.OrthoNormalize(ref up, ref val);
				RotateTarget(Quaternion.LookRotation(val, puppetMaster.targetRoot.up));
			}
			else
			{
				Vector3 val2 = puppetMaster.muscles[0].rigidbody.rotation * hipsUp;
				Vector3 up2 = puppetMaster.targetRoot.up;
				Vector3.OrthoNormalize(ref up2, ref val2);
				RotateTarget(Quaternion.LookRotation(flag ? val2 : (-val2), puppetMaster.targetRoot.up));
			}
			puppetMaster.SampleTargetMappedState();
			Vector3 val3 = (flag ? getUpOffsetProne : getUpOffsetSupine);
			MoveTarget(puppetMaster.muscles[0].rigidbody.position + puppetMaster.targetRoot.rotation * val3);
			GroundTarget(groundLayers);
			getUpPosition = puppetMaster.targetRoot.position;
			getupAnimationBlendWeight = 1f;
			getUpTargetFixed = false;
			break;
		}
		}
		state = newState;
	}

	public void SetColliders(bool unpinned)
	{
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle m in muscles)
		{
			SetColliders(m, unpinned);
		}
	}

	public void SetColliders(Muscle m, bool unpinned)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		MuscleProps props = GetProps(m.props.group);
		if (unpinned)
		{
			Collider[] colliders = m.colliders;
			foreach (Collider val in colliders)
			{
				val.material = (((Object)(object)props.unpinnedMaterial != (Object)null) ? props.unpinnedMaterial : defaults.unpinnedMaterial);
				if (props.disableColliders)
				{
					val.enabled = true;
				}
			}
			return;
		}
		Collider[] colliders2 = m.colliders;
		foreach (Collider val2 in colliders2)
		{
			val2.material = (((Object)(object)props.puppetMaterial != (Object)null) ? props.puppetMaterial : defaults.puppetMaterial);
			if (props.disableColliders)
			{
				Vector3 inertiaTensor = m.rigidbody.inertiaTensor;
				val2.enabled = false;
				m.rigidbody.inertiaTensor = inertiaTensor;
			}
		}
	}

	public override void OnMuscleDisconnected(Muscle m)
	{
		base.OnMuscleDisconnected(m);
		SetColliders(m, unpinned: true);
	}

	public override void OnMuscleReconnected(Muscle m)
	{
		base.OnMuscleReconnected(m);
		if (m == puppetMaster.muscles[0])
		{
			SetState(State.Puppet);
		}
		float num = ((state == State.Puppet) ? 1f : 0f);
		m.state.pinWeightMlp = num;
		m.state.muscleWeightMlp = num;
		m.state.muscleDamperMlp = num;
		m.state.maxForceMlp = 1f;
		m.state.mappingWeightMlp = 1f;
		SetColliders(m, state == State.Unpinned);
	}
}
