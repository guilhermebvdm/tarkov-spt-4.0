using System;
using EFT.AnimatedInteractionsSubsystem;
using UnityEngine;

namespace EFT;

public class PlayerAnimator
{
	public enum EDoorInteractionIndex
	{
		PullLeft,
		PullRight,
		Push
	}

	public enum EWeaponAnimationType
	{
		Rifle,
		Pistol,
		ThrowWeapon,
		Knife,
		EmptyHands,
		RocketLauncher
	}

	public const int BASE_LAYER_INDEX = 0;

	public const int TP_WEAPON_LAYER_INDEX = 1;

	public const int UTILITY_LAYER_INDEX = 2;

	public const int ADDITIVE_AIMING_LAYER_INDEX = 6;

	public const int GUI_LAYER_INDEX = 11;

	public const int LS_LAYER_INDEX = 15;

	public const int BOAR_LAYER_INDEX = 17;

	public const int BTR_LAYER_INDEX = 19;

	public const int INTERACTIONS_LAYER_INDEX = 20;

	[NonSerialized]
	public static int POSE_LEVEL_PARAM_HASH = UnityEngine.Animator.StringToHash("Level");

	[NonSerialized]
	public static int DIRECTION_X_PARAM_HASH = UnityEngine.Animator.StringToHash("Direct_X");

	[NonSerialized]
	public static int DIRECTION_Y_PARAM_HASH = UnityEngine.Animator.StringToHash("Direct_Y");

	[NonSerialized]
	public static int DIRECTION_PARAM_HASH = UnityEngine.Animator.StringToHash("Direct");

	[NonSerialized]
	public static int SPEED_PARAM_HASH = UnityEngine.Animator.StringToHash("Speed");

	[NonSerialized]
	public static int SPRINT_SPEED_PARAM_HASH = UnityEngine.Animator.StringToHash("SprintSpeed");

	public static readonly int SWING_SPEED_PARAM_HASH = UnityEngine.Animator.StringToHash("SwingSpeed");

	[NonSerialized]
	public static int SPRINT_INERTIA_PARAM_HASH = UnityEngine.Animator.StringToHash("SprintInertia");

	public static readonly int WEAPON_SIZE_MODIFIER_PARAM_HASH = UnityEngine.Animator.StringToHash("WeapSizeModifier");

	[NonSerialized]
	public static int DELTA_ROTATION_PARAM_HASH = UnityEngine.Animator.StringToHash("DeltaRotation");

	[NonSerialized]
	public static int SPRINT_PARAM_HASH = UnityEngine.Animator.StringToHash("Sprint");

	[NonSerialized]
	public static int JUMP_PARAM_HASH = UnityEngine.Animator.StringToHash("IsJumping");

	[NonSerialized]
	public static int LANDING_PARAM_HASH = UnityEngine.Animator.StringToHash("Landing");

	[NonSerialized]
	public static int FALLING_DOWN_PARAM_HASH = UnityEngine.Animator.StringToHash("FallingDown");

	[NonSerialized]
	public static int AIM_ANGLE_PARAM_HASH = UnityEngine.Animator.StringToHash("Aim_angle");

	[NonSerialized]
	public static int INERT_PARAM_HASH = UnityEngine.Animator.StringToHash("Inert");

	[NonSerialized]
	public static int INERT_FLOAT_PARAM_HASH = UnityEngine.Animator.StringToHash("InertFloat");

	[NonSerialized]
	public static int PRONE_PARAM_HASH = UnityEngine.Animator.StringToHash("Prone");

	[NonSerialized]
	public static int PRONE_RELOAD_PARAM_HASH = UnityEngine.Animator.StringToHash("ProneReload");

	public static readonly int KICK_PARAM_HASH = UnityEngine.Animator.StringToHash("Kick");

	public static readonly int KICK_SUCCEED_PARAM_HASH = UnityEngine.Animator.StringToHash("KickSucceed");

	public static readonly int BTR_GO_IN_PARAM_HASH = UnityEngine.Animator.StringToHash("BtrGoInTrigger");

	public static readonly int BTR_GO_OUT_PARAM_HASH = UnityEngine.Animator.StringToHash("BtrGoOutTrigger");

	public static readonly int BTR_GO_IN_FAST_PARAM_HASH = UnityEngine.Animator.StringToHash("BtrGoInTriggerFast");

	public static readonly int BTR_GO_OUT_FAST_PARAM_HASH = UnityEngine.Animator.StringToHash("BtrGoOutTriggerFast");

	public static readonly int BTR_ITEM_USE_PARAM_HASH = UnityEngine.Animator.StringToHash("BtrItemUse");

	[NonSerialized]
	public static int LOOT_PARAM_HASH = UnityEngine.Animator.StringToHash("Loot");

	[NonSerialized]
	public static int TILT_PARAM_HASH = UnityEngine.Animator.StringToHash("Tilt");

	[NonSerialized]
	public static int BLIND_FIRE_PARAM_HASH = UnityEngine.Animator.StringToHash("BlindFire");

	[NonSerialized]
	public static int BLIND_FIRE_FLOAT_PARAM_HASH = UnityEngine.Animator.StringToHash("BlindFireFloat");

	[NonSerialized]
	public static int STEP_PARAM_HASH = UnityEngine.Animator.StringToHash("Step");

	[NonSerialized]
	public static int FOOT_STEP_PARAM_HASH = UnityEngine.Animator.StringToHash("FootStep");

	public static readonly int PICKUP_PARAM_HASH = UnityEngine.Animator.StringToHash("Pickup");

	public static readonly int INTERACT_PARAM_HASH = UnityEngine.Animator.StringToHash("Interact");

	public static readonly int APPROACHED_HASH = UnityEngine.Animator.StringToHash("Approached");

	public static readonly int WEAPON_TYPE = UnityEngine.Animator.StringToHash("WeaponType");

	public static readonly int STATIONARY_HASH = UnityEngine.Animator.StringToHash("Stationary");

	public static readonly int STATIONARY_TYPE_HASH = UnityEngine.Animator.StringToHash("StationaryType");

	public static readonly int THIRDPERSON_HASH = UnityEngine.Animator.StringToHash("ThirdPerson");

	public static readonly int THIRDPERSON_FLOAT_HASH = UnityEngine.Animator.StringToHash("ThirdPersonFloat");

	public static readonly int DEFLECTED_HASH = UnityEngine.Animator.StringToHash("Deflected");

	public static readonly int TRANSITION_SPEED_HASH = UnityEngine.Animator.StringToHash("TransitionMultiplier");

	public static readonly int WEAPON_TYPE_FLOAT_HASH = UnityEngine.Animator.StringToHash("WeaponTypeFloat");

	public static readonly int RELOAD_FLOAT_PARAM_HASH = UnityEngine.Animator.StringToHash("ReloadFloat");

	[NonSerialized]
	public static int FALLINGDOWN_FLOAT_PARAM_HASH = UnityEngine.Animator.StringToHash("FallingDownFloat");

	public static readonly int WEAPON_OVERLAP_HASH = UnityEngine.Animator.StringToHash("WeaponOverlap");

	public static readonly int WEAPON_OVERLAP_FLOAT_HASH = UnityEngine.Animator.StringToHash("WeaponOverlapFloat");

	public static readonly int SIDESTEP_FLOAT_HASH = UnityEngine.Animator.StringToHash("SidestepFloat");

	public static readonly int BOAR_WEAPON_PKM = UnityEngine.Animator.StringToHash("BoarWeaponPkm");

	public static readonly int FLOAT_VAULTING_HEIGHT = UnityEngine.Animator.StringToHash("VaultingHeight");

	public static readonly int FLOAT_VAULTING_LENGTH = UnityEngine.Animator.StringToHash("VaultingLength");

	public static readonly int BOOL_DO_VAULT = UnityEngine.Animator.StringToHash("DoVault");

	public static readonly int BOOL_DO_CLIMB = UnityEngine.Animator.StringToHash("DoClimb");

	public static readonly int BOOL_IS_VAULTING = UnityEngine.Animator.StringToHash("IsVaulting");

	public static readonly int BOOL_IS_GROUNDED = UnityEngine.Animator.StringToHash("IsGrounded");

	public static readonly int FLOAT_ABSOLUTE_FORWARD_VELOCITY = UnityEngine.Animator.StringToHash("AbsoluteForwardVelocity");

	public static readonly int FLOAT_OVER_OBSTACLE_HEIGHT = UnityEngine.Animator.StringToHash("OverObstacleHeight");

	public static readonly int VAULTING_SPEED_HASH = UnityEngine.Animator.StringToHash("Vaulting_speed");

	public static readonly int LEFT_STANCE_BOOL_HASH = UnityEngine.Animator.StringToHash("LeftStance");

	public static readonly int LEFT_STANCE_FORCE_BOOL_HASH = UnityEngine.Animator.StringToHash("ForceLeftStanceInOneFrame");

	public static readonly int PRONE_BIPODS_ON = UnityEngine.Animator.StringToHash("ProneBipodsOn");

	public static readonly int UI_WEAPON_OPERATION = UnityEngine.Animator.StringToHash("UI_WeaponOperation");

	public static readonly int UI_RANDOM_ANIMATIONS_COUNT = UnityEngine.Animator.StringToHash("UI_RandomAnimationsCount");

	public static readonly int UI_RANDOM_TRANSITION = UnityEngine.Animator.StringToHash("UI_RandomTransition");

	public static readonly int UI_WEAPON_TYPE = UnityEngine.Animator.StringToHash("UI_WeaponType");

	public static readonly int ELBOW_LEFT_WEIGHT = UnityEngine.Animator.StringToHash("Elbow_Left");

	public static readonly int ELBOW_RIGHT_WEIGHT = UnityEngine.Animator.StringToHash("Elbow_Right");

	public static readonly int LEFT_HAND_WEIGHT = UnityEngine.Animator.StringToHash("Hand_Left");

	public static readonly int RIGHT_HAND_WEIGHT = UnityEngine.Animator.StringToHash("Hand_Right");

	public static readonly int LEFT_SHOULDER_WEIGHT = UnityEngine.Animator.StringToHash("Shoulder_Left");

	public static readonly int RIGHT_SHOULDER_WEIGHT = UnityEngine.Animator.StringToHash("Shoulder_Right");

	public static readonly int WEAPON_ROOT_3RD = UnityEngine.Animator.StringToHash("Weapon_3rd");

	public static readonly int FIRST_PERSON_CURVE_WEIGHT = UnityEngine.Animator.StringToHash("First_Person_Curve_Weight");

	public static readonly int FIRST_PERSON_ACTION = UnityEngine.Animator.StringToHash("FirstAction");

	public static readonly int POSITION_CACHE_FOR_WEAPON_PROCEDURAL = UnityEngine.Animator.StringToHash("Position_cache");

	public static readonly int LEFT_STANCE_CURVE = UnityEngine.Animator.StringToHash("Leftstance_curve");

	public static readonly int MOUSE_LOOK_CURVE = UnityEngine.Animator.StringToHash("Mouse_look");

	public static readonly int AIMING_LAYER_CURVE = UnityEngine.Animator.StringToHash("Aiming_Layer");

	public static readonly int TURN = UnityEngine.Animator.StringToHash("Turn");

	public static readonly int TURN_LEFT_90_AIM_HASH = UnityEngine.Animator.StringToHash("Turn_Left_90_Aim");

	public static readonly int TURN_RIGHT_90_AIM_HASH = UnityEngine.Animator.StringToHash("Turn_Right_90_Aim");

	public static readonly int SIDEBACK_SPEED_PARAMETR_HASH = UnityEngine.Animator.StringToHash("SidebackSpeed");

	public static readonly int INVENTORY_PARAMETR_HASH = UnityEngine.Animator.StringToHash("Inventory");

	public static readonly int INVENTORY_OPERATION_PARAMETR_HASH = UnityEngine.Animator.StringToHash("InventoryOperation");

	public const int ATTACK_NO_INERT_LAYER_INDEX = 2;

	public static readonly int IS_ALERTED = UnityEngine.Animator.StringToHash("isAlerted");

	public static readonly int IS_ALERTED_FLOAT = UnityEngine.Animator.StringToHash("isAlertedFloat");

	public static readonly int IS_LOST = UnityEngine.Animator.StringToHash("isLost");

	public static readonly int IS_HIT = UnityEngine.Animator.StringToHash("isHit");

	public static readonly int HIT_INDEX_FLOAT = UnityEngine.Animator.StringToHash("HitIndex");

	public static readonly int HIT_DIRECTION_X = UnityEngine.Animator.StringToHash("HitDirectionX");

	public static readonly int HIT_DIRECTION_Y = UnityEngine.Animator.StringToHash("HitDirectionY");

	public static readonly int IS_ATTACKING = UnityEngine.Animator.StringToHash("isAttacking");

	public static readonly int ATTACK_VARIANT = UnityEngine.Animator.StringToHash("AnimVariant");

	public static readonly int IS_TURNING = UnityEngine.Animator.StringToHash("isTurning");

	public static readonly int TURNING_DELTA_ROTATION = UnityEngine.Animator.StringToHash("turnDeltaRotation");

	public static readonly int DOOR_INTERACTION_ID = UnityEngine.Animator.StringToHash("doorIndexFloat");

	public static readonly int ZOMBIE_WEAPON_TYPE_FLOAT_HASH = UnityEngine.Animator.StringToHash("ZombieWeaponTypeFloat");

	public static readonly int IS_AIMING_BOOL_HASH = UnityEngine.Animator.StringToHash("isAiming");

	public static readonly int USE_LEFT_HAND_BOOL_HASH = UnityEngine.Animator.StringToHash("UseLeftHand");

	public static readonly int REMOVE_LEFT_HAND_SPEED_FLOAT_HASH = UnityEngine.Animator.StringToHash("RemoveLeftHandSpeed");

	[NonSerialized]
	public Func<IAnimator> AnimatorGetter;

	[NonSerialized]
	public PlayerAnimatorEventsDispatcher EventsDispatcher_1;

	[NonSerialized]
	public float CachedSidebackSpeed = float.MinValue;

	[NonSerialized]
	public float CachedSidestepFloat = float.MinValue;

	[NonSerialized]
	public float CachedPoseLevel = float.MinValue;

	[NonSerialized]
	public float CachedHandsToBodyAngle = float.MinValue;

	[NonSerialized]
	public float CachedaimAngle = float.MinValue;

	[NonSerialized]
	public Vector2 CachedMovemenDirection = Vector2.zero;

	[NonSerialized]
	public EMovementDirection CachedDirection;

	[NonSerialized]
	public float CachedCharaterMovementSpeed = float.MinValue;

	[NonSerialized]
	public float CachedCharacterSprintSpeed = float.MinValue;

	[NonSerialized]
	public float CachedTilt = float.MinValue;

	[NonSerialized]
	public float CachedBlindFireFloat = float.MinValue;

	[NonSerialized]
	public int CachedStep = int.MinValue;

	public IAnimator Animator => AnimatorGetter();

	public Vector3 DeltaPosition => Animator.deltaPosition;

	public Quaternion DeltaRotation => Animator.deltaRotation;

	public PlayerAnimatorEventsDispatcher EventsDispatcher => EventsDispatcher_1;

	[field: NonSerialized]
	public IAnimatedInteractions AnimatedInteractions { get; set; }

	public event Action OnJumpEvent;

	public event Action OnLandEvent;

	public PlayerAnimator(Func<IAnimator> animatorGetter)
	{
		AnimatorGetter = animatorGetter;
		EventsDispatcher_1 = new PlayerAnimatorEventsDispatcher(AnimatorGetter());
		AnimatedInteractions = new GClass2726(20, Animator);
	}

	public void SetSidebackSpeed(float speed)
	{
		if (Mathf.Abs(CachedSidebackSpeed - speed) >= float.Epsilon)
		{
			Animator.SetFloat(SIDEBACK_SPEED_PARAMETR_HASH, speed);
			CachedSidebackSpeed = speed;
		}
	}

	public void SetSidestepFloat(float speed)
	{
		if (Mathf.Abs(CachedSidestepFloat - speed) >= float.Epsilon)
		{
			Animator.SetFloat(SIDESTEP_FLOAT_HASH, speed);
			CachedSidestepFloat = speed;
		}
	}

	public void SetLayerWeight(int index, float weight)
	{
		Animator.SetLayerWeight(index, weight);
	}

	public void SetPoseLevel(float poseLevel)
	{
		if (Mathf.Abs(CachedPoseLevel - poseLevel) >= float.Epsilon)
		{
			Animator.SetFloat(POSE_LEVEL_PARAM_HASH, poseLevel);
			CachedPoseLevel = poseLevel;
		}
	}

	public void SetSwingSpeed(float swingSpeed)
	{
		Animator.SetFloat(SWING_SPEED_PARAM_HASH, swingSpeed);
	}

	public void method_0(float reload = 1f, float draw = 1f)
	{
		WeaponAnimationSpeedControllerClass.SetSpeedReload(Animator, reload);
		WeaponAnimationSpeedControllerClass.SetSpeedDraw(Animator, draw);
	}

	public void method_1(float fix = 1f)
	{
		WeaponAnimationSpeedControllerClass.SetSpeedFix(Animator, fix);
	}

	public void EnableInert(bool enabled)
	{
		Animator.SetBool(INERT_PARAM_HASH, enabled);
		Animator.SetFloat(INERT_FLOAT_PARAM_HASH, enabled ? 1f : 0f);
	}

	public void SetFallingDownFloat(float val)
	{
		if (Animator.HasParameter(FALLINGDOWN_FLOAT_PARAM_HASH))
		{
			Animator.SetFloat(FALLINGDOWN_FLOAT_PARAM_HASH, val);
		}
	}

	public void SetKickSucceed(bool succeed)
	{
		Animator.SetBool(KICK_SUCCEED_PARAM_HASH, succeed);
	}

	public void EnableSprint(bool enabled)
	{
		Animator.SetBool(SPRINT_PARAM_HASH, enabled);
	}

	public void EnableJump(bool enabled)
	{
		Animator.SetBool(JUMP_PARAM_HASH, enabled);
	}

	public void EnableBoarPkm(bool enabled)
	{
		Animator.SetBool(BOAR_WEAPON_PKM, enabled);
	}

	public void SetDeltaRotation(float handsToBodyAngle)
	{
		if (Mathf.Abs(CachedHandsToBodyAngle - handsToBodyAngle) >= float.Epsilon)
		{
			Animator.SetFloat(DELTA_ROTATION_PARAM_HASH, handsToBodyAngle);
			CachedHandsToBodyAngle = handsToBodyAngle;
		}
	}

	public void SetRotationSpeed(float rotationSpeed)
	{
		Debug.Log("Obsolete call");
	}

	public void SetAimAngle(float aimAngle)
	{
		if (Math.Abs(CachedaimAngle - aimAngle) >= float.Epsilon)
		{
			CachedaimAngle = aimAngle;
			Animator.SetFloat(AIM_ANGLE_PARAM_HASH, aimAngle);
		}
	}

	public void EnableProneReload(bool enabled)
	{
		Animator.SetBool(PRONE_RELOAD_PARAM_HASH, enabled);
	}

	public void EnableFallingDown(bool enabled)
	{
		Animator.SetBool(FALLING_DOWN_PARAM_HASH, enabled);
	}

	public void EnableLanding(bool enabled)
	{
		Animator.SetBool(LANDING_PARAM_HASH, enabled);
	}

	public void SetMovementDirection(Vector2 direction)
	{
		if (Math.Abs(CachedMovemenDirection.x - direction.x) >= float.Epsilon)
		{
			CachedMovemenDirection.x = direction.x;
			Animator.SetFloat(DIRECTION_X_PARAM_HASH, direction.x);
		}
		if (Math.Abs(CachedMovemenDirection.y - direction.y) >= float.Epsilon)
		{
			CachedMovemenDirection.y = direction.y;
			Animator.SetFloat(DIRECTION_Y_PARAM_HASH, direction.y);
		}
	}

	public void SetDiscreteDirection(EMovementDirection direction)
	{
		if (direction != EMovementDirection.None && CachedDirection != direction)
		{
			CachedDirection = direction;
			float value = GClass2076.InvertDirection(direction);
			Animator.SetFloat(DIRECTION_PARAM_HASH, value);
		}
	}

	public void SetDiscreteDirection(float direction)
	{
		Animator.SetFloat(DIRECTION_PARAM_HASH, direction);
	}

	public void SetSprintInertia(float val)
	{
		Animator.SetFloat(SPRINT_INERTIA_PARAM_HASH, val);
	}

	public void SetCharacterMovementSpeed(float charaterMovementSpeed)
	{
		if (Mathf.Abs(CachedCharaterMovementSpeed - charaterMovementSpeed) >= Mathf.Epsilon)
		{
			Animator.SetFloat(SPEED_PARAM_HASH, charaterMovementSpeed);
			CachedCharaterMovementSpeed = charaterMovementSpeed;
		}
	}

	public float GetCharacterMovementSpeed()
	{
		return Animator.GetFloat(SPEED_PARAM_HASH);
	}

	public void SetCharacterSprintSpeed(float speed)
	{
		if (Mathf.Abs(CachedCharacterSprintSpeed - speed) >= Mathf.Epsilon)
		{
			Animator.SetFloat(SPRINT_SPEED_PARAM_HASH, speed);
			CachedCharacterSprintSpeed = speed;
		}
	}

	public void EnableKick(bool enabled)
	{
		Animator.SetBool(KICK_PARAM_HASH, enabled);
	}

	public void SetLeftStance(bool enabled)
	{
		Animator.SetBool(LEFT_STANCE_BOOL_HASH, enabled);
	}

	public void SetLeftStanceForceInOneFrame(bool enabled)
	{
		Animator.SetBool(LEFT_STANCE_FORCE_BOOL_HASH, enabled);
	}

	public void EnableLoot(bool enabled)
	{
		Animator.SetBool(LOOT_PARAM_HASH, enabled);
	}

	public void SetTilt(float tilt)
	{
		if (Mathf.Abs(CachedTilt - tilt) >= Mathf.Epsilon)
		{
			Animator.SetFloat(TILT_PARAM_HASH, tilt);
			CachedTilt = tilt;
		}
	}

	public void SetBlindFire(float blindFire)
	{
		Animator.SetFloat(BLIND_FIRE_PARAM_HASH, blindFire);
	}

	public void SetBlindFireFloat(float blindFireFloatValue)
	{
		if (Mathf.Abs(CachedBlindFireFloat - blindFireFloatValue) >= float.Epsilon)
		{
			Animator.SetFloat(BLIND_FIRE_FLOAT_PARAM_HASH, blindFireFloatValue);
			CachedBlindFireFloat = blindFireFloatValue;
		}
	}

	public void SetStep(int step)
	{
		if (step != CachedStep)
		{
			CachedStep = step;
			Animator.SetInteger(STEP_PARAM_HASH, step);
		}
	}

	public void SetFootStep(float val)
	{
		Animator.SetFloat(FOOT_STEP_PARAM_HASH, val);
	}

	public void EnableProne(bool prone)
	{
		Animator.SetBool(PRONE_PARAM_HASH, prone);
	}

	public void EnablePickup(bool enabled)
	{
		Animator.SetBool(PICKUP_PARAM_HASH, enabled);
	}

	public void SetInteraction(bool isInteracting, bool approached = false)
	{
		Animator.SetBool(INTERACT_PARAM_HASH, isInteracting);
		SetApproached(isInteracting && approached);
	}

	public bool IsInteractionOn()
	{
		if (!Animator.GetBool(INTERACT_PARAM_HASH) && !Animator.GetBool(KICK_PARAM_HASH))
		{
			return Animator.GetBool(STATIONARY_HASH);
		}
		return true;
	}

	public bool IsProneReload()
	{
		return Animator.GetBool(PRONE_RELOAD_PARAM_HASH);
	}

	public void SetDoorBreak(bool isInteracting, bool approached = false)
	{
		Animator.SetBool(KICK_PARAM_HASH, isInteracting);
		SetApproached(isInteracting && approached);
	}

	public void SetBtrLayerEnabled(bool enable)
	{
		SetLayerWeight(19, enable ? 1 : 0);
	}

	public void SetBtrGoIn(bool fast)
	{
		Animator.SetTrigger(fast ? BTR_GO_IN_FAST_PARAM_HASH : BTR_GO_IN_PARAM_HASH);
	}

	public void SetBtrGoOut(bool fast)
	{
		Animator.SetTrigger(fast ? BTR_GO_OUT_FAST_PARAM_HASH : BTR_GO_OUT_PARAM_HASH);
	}

	public void SetItemUse(bool enable)
	{
		Animator.SetBool(BTR_ITEM_USE_PARAM_HASH, enable);
	}

	public bool GetItemUse()
	{
		return Animator.GetBool(BTR_ITEM_USE_PARAM_HASH);
	}

	public void SetApproached(bool isInPosition)
	{
		Animator.SetBool(APPROACHED_HASH, isInPosition);
	}

	public void SetStationary(bool stationary)
	{
		Animator.SetBool(STATIONARY_HASH, stationary);
	}

	public void SetStationaryType(int index)
	{
		Animator.SetInteger(STATIONARY_TYPE_HASH, index);
	}

	public void SetTransitionSpeed(float speed)
	{
		Animator.SetFloat(TRANSITION_SPEED_HASH, speed);
	}

	public void SetVaultingHeight(float height)
	{
		Animator.SetFloat(FLOAT_VAULTING_HEIGHT, height);
	}

	public float GetVaultingHeight()
	{
		return Animator.GetFloat(FLOAT_VAULTING_HEIGHT);
	}

	public void SetVaultingLength(float length)
	{
		Animator.SetFloat(FLOAT_VAULTING_LENGTH, length);
	}

	public float GetVaultingLength()
	{
		return Animator.GetFloat(FLOAT_VAULTING_LENGTH);
	}

	public void SetDoVault(bool doVaulting)
	{
		Animator.SetBool(BOOL_DO_VAULT, doVaulting);
	}

	public bool GetDoVault()
	{
		return Animator.GetBool(BOOL_DO_VAULT);
	}

	public void SetDoClimb(bool doClimb)
	{
		Animator.SetBool(BOOL_DO_CLIMB, doClimb);
	}

	public bool GetDoClimb()
	{
		return Animator.GetBool(BOOL_DO_CLIMB);
	}

	public void SetAbsoluteForwardVelocity(float velocity)
	{
		Animator.SetFloat(FLOAT_ABSOLUTE_FORWARD_VELOCITY, velocity);
	}

	public float GetAbsoluteForwardVelocity()
	{
		return Animator.GetFloat(FLOAT_ABSOLUTE_FORWARD_VELOCITY);
	}

	public void SetBehindObstacleRatio(float height)
	{
		Animator.SetFloat(FLOAT_OVER_OBSTACLE_HEIGHT, height);
	}

	public float GetBehindObstacleRatio()
	{
		return Animator.GetFloat(FLOAT_OVER_OBSTACLE_HEIGHT);
	}

	public void SetIsVaulting(bool isVaulting)
	{
		Animator.SetBool(BOOL_IS_VAULTING, isVaulting);
	}

	public bool GetIsVaulting()
	{
		return Animator.GetBool(BOOL_IS_VAULTING);
	}

	public void SetVaultingSpeed(float speed)
	{
		Animator.SetFloat(VAULTING_SPEED_HASH, speed);
	}

	public float GetVaultingSpeed()
	{
		return Animator.GetFloat(VAULTING_SPEED_HASH);
	}

	public void SetIsGrounded(bool isGrounded)
	{
		Animator.SetBool(BOOL_IS_GROUNDED, isGrounded);
	}

	public bool GetLeftStance()
	{
		return Animator.GetBool(LEFT_STANCE_BOOL_HASH);
	}

	public void SetInventory(bool inventory)
	{
		Animator.SetBool(INVENTORY_PARAMETR_HASH, inventory);
	}

	public void SetInventoryOperation(bool isOperation)
	{
		Animator.SetBool(INVENTORY_OPERATION_PARAMETR_HASH, isOperation);
	}

	public void SetProneBipodMount(bool isProneBipodMounted)
	{
		Animator.SetBool(PRONE_BIPODS_ON, isProneBipodMounted);
	}

	public void SetAlert(bool isAlerted)
	{
		Animator.SetBool(IS_ALERTED, isAlerted);
	}

	public void SetAlertFloat(float alertFloat)
	{
		Animator.SetFloat(IS_ALERTED_FLOAT, alertFloat);
	}

	public void TriggerIsLost()
	{
		Animator.SetTrigger(IS_LOST);
	}

	public void SetHit(int hitIndex, float hitDirectionY, float hitDirectionX)
	{
		Animator.SetTrigger(IS_HIT);
		Animator.SetInteger(HIT_INDEX_FLOAT, hitIndex);
		Animator.SetFloat(HIT_DIRECTION_X, hitDirectionX);
		Animator.SetFloat(HIT_DIRECTION_Y, hitDirectionY);
	}

	public void SetAttack(float attackVariant)
	{
		Animator.SetFloat(ATTACK_VARIANT, attackVariant);
	}

	public void TriggerAttack()
	{
		Animator.SetTrigger(IS_ATTACKING);
	}

	public void SetTurning(bool isTurning)
	{
		Animator.SetBool(IS_TURNING, isTurning);
	}

	public void SetTurnDeltaAngle(float turnDeltaAngle)
	{
		Animator.SetFloat(TURNING_DELTA_ROTATION, turnDeltaAngle);
	}

	public void SetDoorInteraction(EDoorInteractionIndex doorInteractionIndex)
	{
		Animator.SetFloat(DOOR_INTERACTION_ID, (float)doorInteractionIndex);
	}

	public void SetAiming(bool isAiming)
	{
		Animator.SetBool(IS_AIMING_BOOL_HASH, isAiming);
	}

	public void SetUseLeftHand(bool val)
	{
		Animator.SetBool(USE_LEFT_HAND_BOOL_HASH, val);
	}

	public bool GetTurning()
	{
		return Animator.GetBool(IS_TURNING);
	}

	public float GetTurningRotation()
	{
		return Animator.GetFloat(TURNING_DELTA_ROTATION);
	}

	public bool IsJumpSetted()
	{
		return Animator.GetBool(JUMP_PARAM_HASH);
	}

	public void SetWeaponId(EWeaponAnimationType val)
	{
		Animator.SetInteger(WEAPON_TYPE, (int)val);
		Animator.SetFloat(WEAPON_TYPE_FLOAT_HASH, (float)val);
	}

	public void SetWeaponType(EWeaponAnimationType value)
	{
		Animator.SetInteger(WEAPON_TYPE, (int)value);
	}

	public void SetWeaponTypeFloat(float value)
	{
		Animator.SetFloat(WEAPON_TYPE_FLOAT_HASH, value);
	}

	public float GetWeaponTypeFloat()
	{
		return Animator.GetFloat(WEAPON_TYPE_FLOAT_HASH);
	}

	public void SetIsThirdPerson(bool isItThough)
	{
		Animator.SetBool(THIRDPERSON_HASH, isItThough);
		Animator.SetFloat(THIRDPERSON_FLOAT_HASH, isItThough ? 1f : 0f);
	}

	public void SetDeflected(bool deflected)
	{
		Animator.SetFloat(DEFLECTED_HASH, deflected ? 1f : 0f);
	}

	public void SetWeaponOverlap(float overlap)
	{
		Animator.SetInteger(WEAPON_OVERLAP_HASH, (overlap > 0f) ? 1 : 0);
		Animator.SetFloat(WEAPON_OVERLAP_FLOAT_HASH, overlap);
	}

	public void SetProneStateForce()
	{
		Animator.Play(PRONE_PARAM_HASH, 0, 0f);
	}

	public void DoAction()
	{
	}

	public void OnJump()
	{
		this.OnJumpEvent?.Invoke();
	}

	public void OnLand()
	{
		this.OnLandEvent?.Invoke();
	}

	public void Dispose()
	{
		EventsDispatcher_1.Dispose();
	}
}
