using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.MovingPlatforms;
using EFT.ObstacleCollision;
using EFT.Vaulting;
using EFT.WeaponMounting;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

public class MovementContext : GInterface283
{
	public delegate void GDelegate72(EPlayerState previousState, EPlayerState nextState);

	public delegate void GDelegate73(int oldStateHashIndex, int newStateHashIndex);

	[Serializable]
	[CompilerGenerated]
	public class Class1396
	{
		public static readonly Class1396 class1396_0 = new Class1396();

		public static Func<IPlayerStateContainerBehaviour, int> func_0;

		public static Func<IPlayerStateContainerBehaviour, int> func_1;

		public static Func<IPlayerStateContainerBehaviour, EPlayerState> func_2;

		public int method_0(IPlayerStateContainerBehaviour x)
		{
			return x.FullNameHash;
		}

		public int method_1(IPlayerStateContainerBehaviour x)
		{
			return x.FullNameHash;
		}

		public EPlayerState method_2(IPlayerStateContainerBehaviour x)
		{
			return x.PlayerStateName;
		}

		public void method_3(Player player)
		{
			if (!player.UsedSimplifiedSkeleton)
			{
				Quaternion handsRotation = player.HandsRotation;
				player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(player.PlayerBones.Ribcage.Original.position, handsRotation);
				player.CameraContainer.transform.rotation = handsRotation;
			}
		}

		public void method_4(Player player)
		{
			if (CachedVaultingRotation == Vector3.zero)
			{
				CachedVaultingRotation = player.HandsRotation.eulerAngles;
			}
			float num = 40f;
			LastVaultingRotation = Quaternion.Euler(Vector3.Lerp(b: new Vector3((!(CachedVaultingRotation.x < 180f)) ? CachedVaultingRotation.x : Mathf.Min(90f - num, CachedVaultingRotation.x), CachedVaultingRotation.y, CachedVaultingRotation.z), a: CachedVaultingRotation, t: player.MovementContext.PlayerAnimator.Animator.GetFloat(PlayerAnimator.MOUSE_LOOK_CURVE)));
			player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(player.PlayerBones.Ribcage.Original.position, LastVaultingRotation);
			player.CameraContainer.transform.rotation = LastVaultingRotation;
		}

		public void method_5(Player player)
		{
			StationaryWeapon stationaryWeapon = player.MovementContext.StationaryWeapon;
			Quaternion handsRotation = player.HandsRotation;
			player.HandsController.ControllerGameObject.transform.rotation = handsRotation;
			Vector3 vector = stationaryWeapon.Hinge.position - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(stationaryWeapon.Hinge.localPosition);
			player.HandsController.ControllerGameObject.transform.position += vector;
			player.CameraContainer.transform.rotation = handsRotation;
			player.MovementContext.StationaryWeapon.OnRotation();
		}

		public void method_6(Player player)
		{
			Quaternion quaternion = Quaternion.Euler(0f, player.MovementContext.Yaw, 0f);
			Quaternion rotation = Quaternion.Euler(player.MovementContext.Pitch - 90f, player.MovementContext.Yaw, 0f);
			StationaryWeapon stationaryWeapon = player.MovementContext.StationaryWeapon;
			Quaternion rotation2 = quaternion;
			player.HandsController.ControllerGameObject.transform.rotation = rotation2;
			Vector3 vector = stationaryWeapon.Hinge.position - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(stationaryWeapon.Hinge.localPosition);
			player.HandsController.HandsHierarchy.GetCachedTransform(3).rotation = rotation;
			player.HandsController.ControllerGameObject.transform.position += vector;
			player.CameraContainer.transform.rotation = quaternion;
			player.MovementContext.StationaryWeapon.OnRotation();
			if (player.FirstPersonPointOfView)
			{
				player.ProceduralWeaponAnimation.AdjustOpticsPAG17();
			}
		}

		public void method_7(Player player)
		{
			Quaternion handsRotation = player.HandsRotation;
			player.CameraContainer.transform.rotation = handsRotation;
			Vector3 vector = player.MovementContext.PlayerMountingPointData.MountPointData.MountPoint + Vector3.up * player.MovementContext.PlayerMountingPointData.CurrentMountingPointVerticalOffset;
			Vector3 position = ((player.MovementContext.PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward || !player.ProceduralWeaponAnimation.IsBipodUsed) ? player.ProceduralWeaponAnimation.HandsContainer.MountingRotationCenter : player.ProceduralWeaponAnimation.HandsContainer.MountingRotationCenterBipods);
			Vector3 vector2 = vector - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(position);
			player.MovementContext.PlayerMountingPointData.WeaponMountingPos = player.HandsController.ControllerGameObject.transform.position + vector2;
			Vector3 position2 = Vector3.Lerp(player.PlayerBones.Ribcage.Original.position, player.MovementContext.PlayerMountingPointData.WeaponMountingPos, player.MovementContext.PlayerMountingPointData.TransitionProgress);
			player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(position2, handsRotation);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct333
	{
		public MovementContext movementContext_0;

		public EPhysicalCondition conditions;
	}

	[CompilerGenerated]
	public class Class1397
	{
		public MovementContext movementContext_0;

		public Action<Player.AbstractHandsController, Player.AbstractHandsController> callback;

		public void method_0(Player.AbstractHandsController controller, Player.AbstractHandsController handsController)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (movementContext_0.StationaryWeapon == null)
			{
				movementContext_0.OnHandsControllerChanged -= method_0;
			}
			else if (handsController != null && handsController.Item == movementContext_0.StationaryWeapon.Item)
			{
				movementContext_0.OnHandsControllerChanged -= method_0;
				if (movementContext_0._player.AIData != null)
				{
					movementContext_0._player.AIData.SetStationaryWeapon((Weapon)movementContext_0.StationaryWeapon.Item);
				}
				callback(null, handsController);
			}
		}
	}

	[NonSerialized]
	public float SlopeTreshold = 0.65f;

	protected Player _player;

	[NonSerialized]
	public bool IsBot;

	[NonSerialized]
	public Func<ICharacterController> CharacterControllerGetter;

	[NonSerialized]
	public BifacialTransform PlayerTransform_1;

	[NonSerialized]
	public PlayerAnimator PlayerAnimator_1;

	public GStruct254 InteractionInfo;

	public BaseMovementState OverridenState;

	public BaseMovementState PickUpState;

	public BaseMovementState PlantState;

	public BaseMovementState IdleWeaponMountingState;

	public int BlindFire;

	public bool IgnoreDeltaMovement;

	public float FreefallTime;

	[NonSerialized]
	public EMovementDirection DiscreteDirection_1;

	[NonSerialized]
	public float SmoothedPoseLevel_1;

	[NonSerialized]
	public float PoseLevel_1 = 1f;

	public bool CanSlope = true;

	[NonSerialized]
	public float HandsToBodyAngle_1;

	[NonSerialized]
	public float SmoothedCharacterMovementSpeed_1;

	[NonSerialized]
	public float SprintSpeed_1 = 1f;

	[NonSerialized]
	public float Tilt_1;

	[NonSerialized]
	public float SmoothedTilt_1;

	[NonSerialized]
	public int Step_1;

	[NonSerialized]
	public Vector2 MovementDirection_1;

	[NonSerialized]
	public Vector2 Rotation_1;

	[NonSerialized]
	public Vector2 PreviousRotation_1;

	[NonSerialized]
	public IObstacleCollisionFacade ObstacleCollisionFacade_1;

	[NonSerialized]
	public RaycastHitData GroundHit_1;

	public float CheckGroundedRayDistance = 0.07f;

	public float CheckGroundedCastedSphereContraction = 0.1f;

	[NonSerialized]
	public List<Collider> IgnoredColliders = new List<Collider>();

	[NonSerialized]
	public Collider[] CoroutineColliders = new Collider[0];

	[NonSerialized]
	public float LastSpeedCovertCheck = -1f;

	[NonSerialized]
	public float NextStepNoise;

	[NonSerialized]
	public float NextJumpNoise;

	[NonSerialized]
	[CanBeNull]
	public GClass2175 SpeedLimiter;

	[NonSerialized]
	public LayerMask GroundMask_1;

	[NonSerialized]
	public LayerMask GroundMaskPlusWater;

	[NonSerialized]
	public LayerMask GroundMaskPlusPlayer;

	[NonSerialized]
	public int GrounderMask;

	[NonSerialized]
	public Vector2 YawLimit_1;

	[NonSerialized]
	public Vector2 PitchLimit_1;

	[NonSerialized]
	public Vector2 PitchTargetLimit;

	[NonSerialized]
	public Vector3 PrevSecPos;

	[NonSerialized]
	public float StraightenTimer_1;

	public const float TIME_TO_STRAIGHTEN_UP = 2f;

	[NonSerialized]
	public const float CC_PRONE_STEP_OFFSET = 0.15f;

	[NonSerialized]
	public const float CC_NORMAL_STEP_OFFSET = 0.25f;

	[NonSerialized]
	public const float CC_PRONE_SLOPLE_LIMIT = 45f;

	[NonSerialized]
	public const float CC_NORMAL_SLOPLE_LIMIT = 60f;

	public const float COVERT_MOVEMENT_THRESHOLD = 0.4f;

	public const float COVERT_MOVEMENT_BLEND_RANGE = 0.2f;

	public float CovertEfficiency;

	public bool SoftSurface;

	[NonSerialized]
	public bool PatrolStatus;

	[NonSerialized]
	public float RelativeSpeed = 1f;

	[NonSerialized]
	public Action UnsubFromSprintExit;

	[NonSerialized]
	public EPhysicalCondition PhysicalCondition_1;

	[NonSerialized]
	public Collider[] OverlapColliders = new Collider[1];

	public bool NextBreachResult;

	[NonSerialized]
	public bool SpeedLimitIsDirty;

	public float LevelOnApproachStart = -1f;

	[NonSerialized]
	public Action UnsubscribeOnStrengthSkillLevelChanged;

	[NonSerialized]
	public LeftStanceController LeftStanceController_1;

	[NonSerialized]
	public (Player.ESpeedLimit cause, float time) NextSpeedLimitToExpire;

	[NonSerialized]
	public bool IsInPronePose_1;

	[NonSerialized]
	public bool IsAutoVaultingEnabled;

	public global::BindableStateClass<bool> CanUseProp = new global::BindableStateClass<bool>(initialValue: true);

	[NonSerialized]
	public int CurrentClientAnimatorStateIndex;

	public WorldInteractiveObject.GStruct436 InteractionParameters;

	[NonSerialized]
	public float TimeSinceLastJump;

	[NonSerialized]
	public bool InMountedState;

	[NonSerialized]
	public float InertiaAppliedTime;

	[NonSerialized]
	public float LevelOnJumpEnd;

	[NonSerialized]
	public float SpeedOnJumpEnd;

	[NonSerialized]
	public RaycastHit PredictionHit;

	[NonSerialized]
	public float TiltInertia = EFTHardSettings.Instance.InertiaTiltCurve.Evaluate(0f);

	[NonSerialized]
	public float MaxTiltStep = 25f;

	[NonSerialized]
	public float TiltStepMultiplier;

	[NonSerialized]
	public float PrevTilt;

	[NonSerialized]
	public bool IsImpostorCastTried;

	[NonSerialized]
	public GClass782 ImpostorCC;

	[NonSerialized]
	public Dictionary<Player.ESpeedLimit, float> SpeedLimits = new Dictionary<Player.ESpeedLimit, float>(GClass866<Player.ESpeedLimit>.Count, GClass866<Player.ESpeedLimit>.EqualityComparer) { 
	{
		Player.ESpeedLimit.SurfaceNormal,
		1f
	} };

	[NonSerialized]
	public Dictionary<Player.ESpeedLimit, float> SpeedLimitTimings = new Dictionary<Player.ESpeedLimit, float>(GClass866<Player.ESpeedLimit>.Count, GClass866<Player.ESpeedLimit>.EqualityComparer);

	[NonSerialized]
	public List<ObstacleCollider> EnteredObstacles = new List<ObstacleCollider>();

	public float MountedSmoothedTilt;

	public float MountedSmoothedTiltForCamera;

	[NonSerialized]
	public MovingPlatform Platform_1;

	public Quaternion PlatformRotation;

	[NonSerialized]
	public bool IsOnPlatform;

	[NonSerialized]
	public float GetOffFromPlatformTime;

	[NonSerialized]
	public const float GET_OFF_FROM_PLATFORM_COOLDOWN = 2f;

	public Vector3 LastBlendMotionDelta;

	public Vector3 InputMotion;

	public Vector3 InputMotionBeforeLimit;

	[NonSerialized]
	public int MotionFrame;

	[NonSerialized]
	public bool ScheduleDirectApplyMotion;

	[NonSerialized]
	public Vector3 ScheduledMotion;

	[NonSerialized]
	public float ScheduledDeltaTime;

	[NonSerialized]
	public bool ImpostorCCLateUpdRequested;

	public Vector3 PreviousPosition;

	public const float HEIGHT_FOR_PRONE = 0.3f;

	public const float HEIGHT_FROM_CROUCH = 1.2f;

	public const float HEIGHT_TO_STAND = 1.6f;

	public static readonly Vector2 CCCenterPoseLevel = new Vector2(0.6f, 0.8f);

	public const float CC_CENTER_POSE_LEVEL_FOR_PRONE = 0.35f;

	[NonSerialized]
	public float AccumulatedDeltaPose;

	[NonSerialized]
	public Vector2 CachedRotation_1;

	[NonSerialized]
	public Vector2 MyRotation;

	[NonSerialized]
	public Quaternion MyTransformRotation = Quaternion.identity;

	[NonSerialized]
	public GClass828 AverageRotationX = new GClass828(Singleton<BackendConfigSettingsClass>.Instance.Inertia.AverageRotationFrameSpan);

	[NonSerialized]
	public Vector3 LookDirection_1 = Vector3.forward;

	[NonSerialized]
	public Vector3 PlayerRealForward_1;

	[NonSerialized]
	public bool PreviousGroundResult;

	[NonSerialized]
	public bool IsGrounded_1;

	[NonSerialized]
	public RollingMedian NormalDotHistory = new RollingMedian(5);

	[NonSerialized]
	public List<ControllerColliderHit> CCHits = new List<ControllerColliderHit>();

	[NonSerialized]
	public List<ControllerColliderHit> CCAllHits = new List<ControllerColliderHit>();

	[NonSerialized]
	public float TimeSinceLastCCCollision;

	public RollingMedian VerticalSpeed = new RollingMedian(3);

	[NonSerialized]
	public float StartFallingHeight;

	[NonSerialized]
	public float StartFlyHeight;

	public Action<float, float> OnGrounded;

	[NonSerialized]
	public float SurfacePitch_1;

	[NonSerialized]
	public Vector3 SurfaceDirection = Vector3.zero;

	[NonSerialized]
	public Vector3 SurfaceNormalInterpolated = Vector3.up;

	[NonSerialized]
	public Collider[] CollidersAllocated = new Collider[2];

	[NonSerialized]
	public Dictionary<EPlayerState, BaseMovementState> States_1 = GClass866<EPlayerState>.GetDictWith<BaseMovementState>();

	[NonSerialized]
	public List<int> StateHashes;

	[NonSerialized]
	public Dictionary<int, int> StatesHashToIndex;

	[NonSerialized]
	public Dictionary<int, EPlayerState> StatesHashToName;

	[NonSerialized]
	public static Dictionary<int, EPlayerState> StatesIndexToName = new Dictionary<int, EPlayerState>();

	[NonSerialized]
	public int LastNormalHashSuperCrutch;

	[NonSerialized]
	public float LastPlayerHeight;

	[NonSerialized]
	public bool HasLastHeight;

	[NonSerialized]
	public IPlayerStateContainerBehaviour[] MovementStates;

	[NonSerialized]
	public bool IsInPatrol;

	[NonSerialized]
	public bool BlockFirearms_1;

	public float _poseInertia;

	public float _currentPoseInertia;

	[NonSerialized]
	public StationaryWeapon StationaryWeapon_1;

	public Action<Player> RotationAction = DefaultRotationFunction;

	public static Vector3 CachedVaultingRotation;

	public static Quaternion LastVaultingRotation;

	public static Action<Player> DefaultRotationFunction = delegate(Player player)
	{
		if (!player.UsedSimplifiedSkeleton)
		{
			Quaternion handsRotation = player.HandsRotation;
			player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(player.PlayerBones.Ribcage.Original.position, handsRotation);
			player.CameraContainer.transform.rotation = handsRotation;
		}
	};

	public static Action<Player> VaultingRotationFunction = delegate(Player player)
	{
		if (CachedVaultingRotation == Vector3.zero)
		{
			CachedVaultingRotation = player.HandsRotation.eulerAngles;
		}
		float num = 40f;
		LastVaultingRotation = Quaternion.Euler(Vector3.Lerp(b: new Vector3((!(CachedVaultingRotation.x < 180f)) ? CachedVaultingRotation.x : Mathf.Min(90f - num, CachedVaultingRotation.x), CachedVaultingRotation.y, CachedVaultingRotation.z), a: CachedVaultingRotation, t: player.MovementContext.PlayerAnimator.Animator.GetFloat(PlayerAnimator.MOUSE_LOOK_CURVE)));
		player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(player.PlayerBones.Ribcage.Original.position, LastVaultingRotation);
		player.CameraContainer.transform.rotation = LastVaultingRotation;
	};

	public static Action<Player> UtesRotationFunction = delegate(Player player)
	{
		StationaryWeapon stationaryWeapon = player.MovementContext.StationaryWeapon;
		Quaternion handsRotation = player.HandsRotation;
		player.HandsController.ControllerGameObject.transform.rotation = handsRotation;
		Vector3 vector = stationaryWeapon.Hinge.position - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(stationaryWeapon.Hinge.localPosition);
		player.HandsController.ControllerGameObject.transform.position += vector;
		player.CameraContainer.transform.rotation = handsRotation;
		player.MovementContext.StationaryWeapon.OnRotation();
	};

	public static Action<Player> AGSRotationFunction = delegate(Player player)
	{
		Quaternion quaternion = Quaternion.Euler(0f, player.MovementContext.Yaw, 0f);
		Quaternion rotation = Quaternion.Euler(player.MovementContext.Pitch - 90f, player.MovementContext.Yaw, 0f);
		StationaryWeapon stationaryWeapon = player.MovementContext.StationaryWeapon;
		Quaternion rotation2 = quaternion;
		player.HandsController.ControllerGameObject.transform.rotation = rotation2;
		Vector3 vector = stationaryWeapon.Hinge.position - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(stationaryWeapon.Hinge.localPosition);
		player.HandsController.HandsHierarchy.GetCachedTransform(3).rotation = rotation;
		player.HandsController.ControllerGameObject.transform.position += vector;
		player.CameraContainer.transform.rotation = quaternion;
		player.MovementContext.StationaryWeapon.OnRotation();
		if (player.FirstPersonPointOfView)
		{
			player.ProceduralWeaponAnimation.AdjustOpticsPAG17();
		}
	};

	public static Action<Player> MountingRotationFunction = delegate(Player player)
	{
		Quaternion handsRotation = player.HandsRotation;
		player.CameraContainer.transform.rotation = handsRotation;
		Vector3 vector = player.MovementContext.PlayerMountingPointData.MountPointData.MountPoint + Vector3.up * player.MovementContext.PlayerMountingPointData.CurrentMountingPointVerticalOffset;
		Vector3 position = ((player.MovementContext.PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward || !player.ProceduralWeaponAnimation.IsBipodUsed) ? player.ProceduralWeaponAnimation.HandsContainer.MountingRotationCenter : player.ProceduralWeaponAnimation.HandsContainer.MountingRotationCenterBipods);
		Vector3 vector2 = vector - player.HandsController.HandsHierarchy.GetTransform(ECharacterWeaponBones.Weapon_root).TransformPoint(position);
		player.MovementContext.PlayerMountingPointData.WeaponMountingPos = player.HandsController.ControllerGameObject.transform.position + vector2;
		Vector3 position2 = Vector3.Lerp(player.PlayerBones.Ribcage.Original.position, player.MovementContext.PlayerMountingPointData.WeaponMountingPos, player.MovementContext.PlayerMountingPointData.TransitionProgress);
		player.HandsController.ControllerGameObject.transform.SetPositionAndRotation(position2, handsRotation);
	};

	public Quaternion VaultingCachedLookRotation;

	[NonSerialized]
	public GInterface38 MountingStrategy = new GClass912();

	public ICharacterController CharacterController => CharacterControllerGetter();

	[field: NonSerialized]
	public BaseMovementState OverridenControlsState { get; set; }

	[field: NonSerialized]
	public RollingMedian AverageRotationSpeed { get; set; }

	public RaycastHitData GroundHit => GroundHit_1;

	public int GroundMask => GroundMask_1;

	public float StraightenTimer => StraightenTimer_1;

	public EPhysicalCondition PhysicalCondition => PhysicalCondition_1;

	[field: NonSerialized]
	public float StateSpeedLimit { get; set; } = 1f;

	[field: NonSerialized]
	public float StateSprintSpeedLimit { get; set; } = 1f;

	public SkillManager SkillManager => _player.Skills;

	public float Overweight => _player.Physical.Overweight;

	public float Inertia => _player.Physical.Inertia;

	public float MoveSideInertia => _player.Physical.MoveSideInertia;

	public float MoveDiagonalInertia => _player.Physical.MoveDiagonalInertia;

	public EFTHardSettings Settings => EFTHardSettings.Instance;

	public LeftStanceController LeftStanceController => LeftStanceController_1;

	public IObstacleCollisionFacade ObstacleCollisionFacade => ObstacleCollisionFacade_1;

	public bool UsedSimplifiedSkeleton => _player.UsedSimplifiedSkeleton;

	[field: NonSerialized]
	public bool IsAxesIgnored { get; set; }

	public BackendConfigSettingsClass.InertiaSettings InertiaSettings => Singleton<BackendConfigSettingsClass>.Instance.Inertia;

	public bool AutoVaultingSettingEnabled
	{
		get
		{
			IsAutoVaultingEnabled = (GClass1085.EAutoVaultingUseMode)Singleton<SharedGameSettingsClass>.Instance.Game.Settings.AutoVaultingMode == GClass1085.EAutoVaultingUseMode.Automatic && !_player.IsVaultingPressed;
			return IsAutoVaultingEnabled;
		}
	}

	public virtual bool IsInPronePose
	{
		get
		{
			return IsInPronePose_1;
		}
		set
		{
			if (IsInPronePose_1 == value || _player.UsedSimplifiedSkeleton)
			{
				return;
			}
			if (IsInPronePose)
			{
				if (!IsAI && !CanStandAt(PoseLevel))
				{
					if (!CanSit)
					{
						return;
					}
					SetPoseLevel(0f, force: true);
				}
				_player.Physical.StandUp();
				if (PoseLevel >= _player.Physical.MaxPoseLevel)
				{
					_player.ExecuteSkill((Action)delegate
					{
						_player.Skills.PushUp.Complete(new SkillManager.GStruct279
						{
							Overweight = Overweight
						});
					});
				}
				else
				{
					StraightenTimer_1 = Time.time;
				}
			}
			else if ((!IsAI || !_player.AIData.BotOwner.BotLay.CanLayWithoutCheck()) && !CanProne)
			{
				return;
			}
			IsInPronePose_1 = value;
			PlayerAnimatorEnableProne(IsInPronePose_1);
			this.OnPoseChanged?.Invoke(PoseToInt);
			if (Singleton<BotEventHandler>.Instantiated)
			{
				Singleton<BotEventHandler>.Instance.PoseChanged(_player, PoseToInt);
			}
			this.OnSmoothedPoseLevelChange?.Invoke(IsInPronePose ? (-0.35f) : SmoothedPoseLevel_1, CovertNoiseLevel);
		}
	}

	public float VaultingHeight => _player.VaultingParameters.VaultingHeight;

	[field: NonSerialized]
	public BaseMovementState CurrentState { get; set; }

	[field: NonSerialized]
	public BaseMovementState PreviousState { get; set; }

	public int PoseToInt
	{
		get
		{
			if (!IsInPronePose)
			{
				if (!(PoseLevel < 0.11f))
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}
	}

	[field: NonSerialized]
	public float LastDeltaTime { get; set; } = 0.02f;

	public bool InMountState => InMountedState;

	public bool LeftStanceEnabled => LeftStanceController.LeftStance;

	public float Single_0 => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Inertia.InertiaBackwardCoef, Inertia);

	public float Single_1 => MaxTiltStep * TiltStepMultiplier * Mathf.Sign(PrevTilt);

	public float Single_2 => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Inertia.TiltInertiaMaxSpeed, Inertia);

	public GClass782 GClass782_0
	{
		get
		{
			if (!IsImpostorCastTried)
			{
				ImpostorCC = CharacterController as GClass782;
				IsImpostorCastTried = true;
			}
			return ImpostorCC;
		}
	}

	public PlayerAnimator PlayerAnimator => PlayerAnimator_1;

	public float SprintSpeed
	{
		get
		{
			return SprintSpeed_1;
		}
		set
		{
			SprintSpeed_1 = value;
			PlayerAnimator_1.SetCharacterSprintSpeed(SprintSpeed_1);
		}
	}

	public float SmoothedCharacterMovementSpeed
	{
		get
		{
			return SmoothedCharacterMovementSpeed_1;
		}
		set
		{
			SmoothedCharacterMovementSpeed_1 = value;
			PlayerAnimatorSetCharacterMovementSpeed(SmoothedCharacterMovementSpeed_1);
			if (_player.ProceduralWeaponAnimation != null)
			{
				_player.ProceduralWeaponAnimation.Walk.Speed = SmoothedCharacterMovementSpeed_1;
			}
		}
	}

	public float ActualLinearVelocity => Velocity.magnitude;

	public float SmoothedPoseLevel
	{
		get
		{
			return SmoothedPoseLevel_1;
		}
		set
		{
			SmoothedPoseLevel_1 = value;
			PlayerAnimatorSetPoseLevel(SmoothedPoseLevel);
		}
	}

	public float SmoothedTilt
	{
		get
		{
			return SmoothedTilt_1;
		}
		set
		{
			if (!_player.UsedSimplifiedSkeleton && !InMountedState)
			{
				SmoothedTilt_1 = value;
				PlayerAnimatorSetTilt(SmoothedTilt);
			}
		}
	}

	public float TiltDiff => Math.Abs(SmoothedTilt - Tilt_1);

	public MovingPlatform Platform
	{
		get
		{
			return Platform_1;
		}
		set
		{
			Platform_1 = value;
			bool isOnPlatform;
			if ((isOnPlatform = Platform_1 != null) != IsOnPlatform)
			{
				IsOnPlatform = isOnPlatform;
				if (!IsOnPlatform)
				{
					GetOffFromPlatformTime = Time.time;
				}
			}
		}
	}

	[field: NonSerialized]
	public virtual Vector3 PlatformMotion { get; set; }

	public bool IsOnPlatformWithCooldown
	{
		get
		{
			if (!IsOnPlatform && Time.time > GetOffFromPlatformTime + 2f)
			{
				return false;
			}
			return true;
		}
	}

	public Quaternion TransformRotation => PlayerTransform_1.rotation;

	public Vector3 TransformPosition
	{
		get
		{
			return PlayerTransform_1.position;
		}
		set
		{
			PlayerTransform_1.position = value;
		}
	}

	public Vector3 TransformForwardVector => PlayerTransform_1.forward;

	public BifacialTransform PlayerTransform => PlayerTransform_1;

	public bool IsAI
	{
		get
		{
			if (_player.AIData != null)
			{
				return _player.AIData.IsAI;
			}
			return false;
		}
	}

	public float MaxSpeed => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.WalkSpeed, (float)SkillManager.Strength.SummaryLevel / 60f);

	public float SprintingSpeed => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.SprintSpeed, (float)SkillManager.Strength.SummaryLevel / 60f);

	public float PreviousYaw => PreviousRotation.x;

	public float HandsToBodyAngle => HandsToBodyAngle_1;

	public float TrunkRotationLimit
	{
		get
		{
			return _player.TrunkRotationLimit;
		}
		set
		{
			_player.TrunkRotationLimit = value;
		}
	}

	public Vector2 PitchLimit
	{
		get
		{
			return PitchLimit_1;
		}
		set
		{
			PitchLimit_1 = value;
		}
	}

	public Vector2 MovementDirection
	{
		get
		{
			return MovementDirection_1;
		}
		set
		{
			MovementDirection_1 = value;
			PlayerAnimatorSetMovementDirection(MovementDirection_1);
		}
	}

	public Vector3 AbsoluteMovementDirection => PlayerTransform.TransformVector(new Vector3(MovementDirection.x, 0f, MovementDirection.y));

	public float TransitionSpeed => _player.Physical.TransitionSpeed;

	public Vector3 PlayerColliderCenter => PlayerTransform.TransformPoint(CharacterController.center);

	public float CovertMovementVolume
	{
		get
		{
			if (!SoftSurface)
			{
				return 1f - (float)_player.Skills.CovertMovementLoud;
			}
			return 1f - (float)_player.Skills.CovertMovementSoundVolume;
		}
	}

	public int CovertNoiseLevel
	{
		get
		{
			if (IsInPronePose)
			{
				return 1;
			}
			if (IsSprintEnabled)
			{
				return 2;
			}
			if (CovertEfficiency > 0.9f)
			{
				return 0;
			}
			if (PoseLevel > 0.01f && ClampedSpeed / MaxSpeed > 0.8f)
			{
				return 2;
			}
			return 1;
		}
	}

	public float CovertMovementVolumeBySpeed
	{
		get
		{
			if (!SoftSurface)
			{
				return 1f - (float)_player.Skills.CovertMovementLoud * CovertEfficiency;
			}
			return 1f - (float)_player.Skills.CovertMovementSoundVolume * CovertEfficiency;
		}
	}

	public float CovertEquipmentNoise => 1f + Overweight - (float)_player.Skills.CovertMovementEquipment;

	public float ClampedSpeed => ClampSpeed(CharacterMovementSpeed);

	[field: NonSerialized]
	public float CharacterMovementSpeed { get; set; }

	public float PoseLevel => PoseLevel_1;

	public float Tilt => Tilt_1;

	public Vector2 YawLimit => YawLimit_1;

	public float SurfacePitch => SurfacePitch_1;

	public Vector2 CachedRotation
	{
		get
		{
			return CachedRotation_1;
		}
		set
		{
			CachedRotation_1 = ClampRotation(value);
		}
	}

	public Vector2 Rotation
	{
		get
		{
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
			{
				return _player.Spirit.LookRotation;
			}
			return Rotation_1;
		}
		set
		{
			method_11(Rotation_1);
			Vector2 rotation = ClampRotation(value);
			method_9(rotation);
			AverageRotationX.AddValue(Yaw - PreviousRotation.x);
			_player.ProceduralWeaponAnimation.Pitch = Pitch;
			MyRotation = ClampRotation(value);
			float num = Mathf.DeltaAngle(360f + MyTransformRotation.eulerAngles.y, 360f + MyRotation.x);
			if (Mathf.Abs(num) > EFTHardSettings.Instance.HANDS_TO_BODY_MAX_ANGLE)
			{
				float y = Mathf.DeltaAngle((float)Math.Sign(num) * EFTHardSettings.Instance.HANDS_TO_BODY_MAX_ANGLE, num);
				MyTransformRotation *= Quaternion.Euler(0f, y, 0f);
			}
		}
	}

	public Vector3 LookDirection => LookDirection_1;

	public Vector3 PlayerRealForward => PlayerRealForward_1;

	public Vector3 PlayerRealRight => Vector3.Cross(PlayerRealForward_1, Vector3.up).normalized;

	public Vector2 PreviousRotation
	{
		get
		{
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
			{
				return _player.Spirit.PreviousLookRotation;
			}
			return PreviousRotation_1;
		}
		set
		{
			method_11(value);
		}
	}

	public float Yaw => Rotation.x;

	public float Pitch => Rotation.y;

	public bool IsGrounded
	{
		get
		{
			return IsGrounded_1;
		}
		set
		{
			IsGrounded_1 = value;
		}
	}

	[field: NonSerialized]
	public Vector3 SurfaceNormal { get; set; }

	[field: NonSerialized]
	public Vector3 PlayerSurfaceUpAlignNormal { get; set; }

	[field: NonSerialized]
	public Vector3 Slope { get; set; }

	public Vector3 Velocity => CharacterController.velocity;

	public int Step
	{
		get
		{
			return Step_1;
		}
		set
		{
			int step_ = Step_1;
			Step_1 = value;
			PlayerAnimatorSetStep(Step_1);
			if (step_ != Step_1 && this.OnStepChanged != null)
			{
				this.OnStepChanged(step_, Step_1);
			}
		}
	}

	[field: NonSerialized]
	public Action PickupAction { get; set; }

	[field: NonSerialized]
	public Action<bool> PlantAction { get; set; }

	public Vector3 Vector3_0
	{
		get
		{
			SimpleCharacterController simpleCharacterController = CharacterController as SimpleCharacterController;
			if (simpleCharacterController != null)
			{
				return simpleCharacterController.surfaceNormalAccordingToCapsule;
			}
			Vector3 result = Vector3.zero;
			for (int i = 0; i < CCHits.Count; i++)
			{
				if (CCHits[i].normal.y > result.y)
				{
					result = CCHits[i].normal;
				}
			}
			if (result.sqrMagnitude != 0f)
			{
				return result;
			}
			return Vector3.up;
		}
	}

	[field: NonSerialized]
	public float FallHeight { get; set; }

	[field: NonSerialized]
	public float JumpHeight { get; set; }

	public bool IsSprintEnabled => _player.Physical.Sprinting;

	[field: NonSerialized]
	public float VaultingSpeed { get; set; }

	public Vector3 ForwardProjectionOnSurface
	{
		get
		{
			Vector3 forward = PlayerTransform.forward;
			return Vector3.Cross(new Vector3(forward.z, 0f, 0f - forward.x), SurfaceNormal).normalized;
		}
	}

	public virtual bool CanProne
	{
		get
		{
			if (PhysicalConditionIs(EPhysicalCondition.SprintDisabled))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.ProneDisabled))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.UsingMeds))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.HealingLegs))
			{
				return false;
			}
			if (Vector3.Angle(GClass842.Vector3Up, SurfaceNormal) > 30f)
			{
				return false;
			}
			Vector3 forwardProjectionOnSurface = ForwardProjectionOnSurface;
			Vector3 vector = PlayerTransform.TransformPoint(new Vector3(CharacterController.center.x, 0.4f, CharacterController.center.z));
			float num = 1.55f;
			float num2 = 1.55f;
			if (Physics.SphereCast(vector, 0.25f, forwardProjectionOnSurface, out var hitInfo, 1.3f, GroundMask_1))
			{
				num = ((hitInfo.distance < num) ? hitInfo.distance : num);
			}
			if ((double)num < 0.5 + (double)(InverseTransformVector(PlayerAnimatorDeltaPosition).z * 3f))
			{
				return false;
			}
			if (Physics.SphereCast(vector, 0.25f, -forwardProjectionOnSurface, out hitInfo, 1.3f, GroundMask_1))
			{
				num2 = ((hitInfo.distance < num2) ? hitInfo.distance : num2);
			}
			if (num + num2 < 1.7f)
			{
				return false;
			}
			float num3 = Mathf.Clamp01(1.1f - num2);
			Vector3 test = vector - forwardProjectionOnSurface * (Mathf.Clamp(num2, 0f, 0.6f) - num3);
			Vector3 test2 = vector + forwardProjectionOnSurface * (Mathf.Clamp(num, 0f, 0.4f) + num3);
			if (!method_19(test))
			{
				return false;
			}
			if (!method_19(test2))
			{
				return false;
			}
			return true;
		}
	}

	public virtual bool CanSprint
	{
		get
		{
			if (PhysicalConditionIs(EPhysicalCondition.SprintDisabled))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.UsingMeds))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.HealingLegs))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
			{
				return true;
			}
			if (PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged))
			{
				return false;
			}
			return true;
		}
	}

	public virtual bool CanJump
	{
		get
		{
			if (PhysicalConditionIs(EPhysicalCondition.UsingMeds))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.HealingLegs))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.JumpDisabled))
			{
				return false;
			}
			return true;
		}
	}

	public virtual bool CanWalk
	{
		get
		{
			if (PhysicalConditionIs(EPhysicalCondition.HealingLegs))
			{
				return false;
			}
			if (!ObstacleCollisionFacade_1.CanMove())
			{
				return false;
			}
			return true;
		}
	}

	public virtual bool CanMoveInProne
	{
		get
		{
			if (PhysicalConditionIs(EPhysicalCondition.ProneMovementDisabled))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.UsingMeds))
			{
				return false;
			}
			if (PhysicalConditionIs(EPhysicalCondition.HealingLegs))
			{
				return false;
			}
			return true;
		}
	}

	public bool CanSit => CanStandAt(0f);

	public float Overlap => _player.ProceduralWeaponAnimation.TurnAway.OverlapValue;

	public float OverlapDepth
	{
		get
		{
			if (!_player.ProceduralWeaponAnimation.TurnAway.OverlapsWithPlayer)
			{
				return _player.ProceduralWeaponAnimation.TurnAway.OverlapDepth;
			}
			return 0f;
		}
	}

	public Player.BetterValueBlender IK => _player.ThirdIkWeight;

	public Dictionary<EPlayerState, BaseMovementState> States => States_1;

	public int CurrentAnimatorStateIndex
	{
		get
		{
			int num = CurrentState.AnimatorStateHash;
			if (num != -1)
			{
				LastNormalHashSuperCrutch = num;
			}
			else
			{
				num = LastNormalHashSuperCrutch;
			}
			return GetStateHashTableIndex(num);
		}
	}

	public IPlayerStateContainerBehaviour[] GetInitedMovementState => MovementStates;

	public Vector3 POMOffset => _player.POM.GetOffsetXZ();

	public bool BlockFirearms
	{
		get
		{
			return BlockFirearms_1;
		}
		set
		{
			BlockFirearms_1 = value;
			RestorePreviousPatrol();
		}
	}

	public Quaternion AnimatorDeltaRotation => PlayerAnimator_1.DeltaRotation;

	public Vector3 PlayerAnimatorDeltaPosition
	{
		get
		{
			if (!IgnoreDeltaMovement)
			{
				return PlayerAnimator_1.DeltaPosition;
			}
			return Vector3.zero;
		}
	}

	public float PlayerAnimatorTransitionSpeed
	{
		set
		{
			PlayerAnimator_1?.SetTransitionSpeed(value);
		}
	}

	public bool IsAnimatorInteractionOn => PlayerAnimator_1.IsInteractionOn();

	public virtual Error CanInteract
	{
		get
		{
			if (!_player.HandsController.IsInInteraction() && _player.MovementContext.GetHandProgress() <= 0f)
			{
				if (!_player.CurrentState.CanInteract)
				{
					return new GClass1522("Can't interact in " + _player.CurrentState.Name);
				}
				if (IsInPronePose && !CanSit)
				{
					return new GClass1522("Can't reach");
				}
				return null;
			}
			return new GClass1522("Busy now, can't interact");
		}
	}

	public virtual bool StateLocksInventory
	{
		set
		{
			_player.InventoryController.Locked = value;
		}
	}

	public StationaryWeapon StationaryWeapon
	{
		get
		{
			return StationaryWeapon_1;
		}
		set
		{
			StationaryWeapon_1 = value;
		}
	}

	public bool IsStationaryWeaponInHands
	{
		get
		{
			if (StationaryWeapon_1 != null)
			{
				return StationaryWeapon_1.Item == _player.HandsController.Item;
			}
			return false;
		}
	}

	public EMovementDirection DiscreteDirection
	{
		get
		{
			return DiscreteDirection_1;
		}
		set
		{
			DiscreteDirection_1 = value;
		}
	}

	public float PoseInertia
	{
		get
		{
			return _poseInertia;
		}
		set
		{
			_poseInertia = value;
			_currentPoseInertia = value;
			InertiaAppliedTime = Time.time;
		}
	}

	[field: NonSerialized]
	public float WalkInertia { get; set; } = GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Inertia.WalkInertia, 0f);

	[field: NonSerialized]
	public float SprintBrakeInertia { get; set; } = GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.Inertia.SprintBrakeInertia, 0f);

	public float DeltaRotation => HandsToBodyAngle_1;

	[field: NonSerialized]
	public WorldInteractiveObject InteractionDoor { get; }

	[field: NonSerialized]
	public InteractionResult InteractionDoorResult { get; }

	Vector3 GInterface283.MovementDirection => MovementDirection_1;

	public Transform VaultingGridRoot => _player.PlayerBones.VaultingGridRoot;

	public bool IsInMountedState => PlayerMountingPointData.MountPointData != null;

	[field: NonSerialized]
	public PlayerMountingPointData PlayerMountingPointData { get; } = new PlayerMountingPointData();

	public event Action<EPhysicalCondition, EPhysicalCondition> PhysicalConditionChanged;

	public event Action<ECantRotate> OnCantRotate;

	public event Action<float, float, float, int> OnMaxSpeedChangedEvent;

	public event Action OnCharacterControllerSpeedLimitChanged;

	public event GDelegate72 OnStateChanged;

	public event Action<int> OnPoseChanged;

	public event GDelegate73 OnClientAnimatorStateIndexChanged;

	public event Action<float, int> OnSmoothedPoseLevelChange;

	public event Action<CollisionFlags, Vector3> OnMotionApplied;

	public event Action<float> OnTiltChanged;

	public event Action<float> StationaryTiltChanged;

	public event Action<int, int> OnStepChanged;

	public event Action<Player.AbstractHandsController, Player.AbstractHandsController> OnHandsControllerChanged
	{
		add
		{
			_player.OnHandsControllerChanged += value;
		}
		remove
		{
			_player.OnHandsControllerChanged -= value;
		}
	}

	public event Action HandsChangingEvent
	{
		add
		{
			_player.HandsChangingEvent += value;
		}
		remove
		{
			_player.HandsChangingEvent -= value;
		}
	}

	public void SetCurrentClientAnimatorStateIndex(int hashTableIndex)
	{
		if (CurrentClientAnimatorStateIndex != hashTableIndex)
		{
			int currentClientAnimatorStateIndex = CurrentClientAnimatorStateIndex;
			CurrentClientAnimatorStateIndex = hashTableIndex;
			if (this.OnClientAnimatorStateIndexChanged != null)
			{
				this.OnClientAnimatorStateIndexChanged(currentClientAnimatorStateIndex, CurrentClientAnimatorStateIndex);
			}
		}
	}

	public bool PhysicalConditionIs(EPhysicalCondition c)
	{
		return (PhysicalCondition_1 & c) == c;
	}

	public bool PhysicalConditionContainsAny(EPhysicalCondition c)
	{
		return (PhysicalCondition_1 & c) != 0;
	}

	public bool SetPhysicalCondition(EPhysicalCondition c, bool val)
	{
		if (!PhysicalConditionIs(c) && val)
		{
			PhysicalCondition_1 |= c;
			this.PhysicalConditionChanged?.Invoke(c, PhysicalCondition_1);
		}
		else if (PhysicalConditionIs(c) && !val)
		{
			PhysicalCondition_1 &= ~c;
			this.PhysicalConditionChanged?.Invoke(c, PhysicalCondition_1);
		}
		return val;
	}

	public void ResetPhysicalCondition()
	{
		EPhysicalCondition[] array = (EPhysicalCondition[])Enum.GetValues(typeof(EPhysicalCondition));
		foreach (EPhysicalCondition c in array)
		{
			SetPhysicalCondition(c, val: false);
		}
	}

	public void CopyPhysicalCondition(EPhysicalCondition conditionForCopy)
	{
		EPhysicalCondition[] array = (EPhysicalCondition[])Enum.GetValues(typeof(EPhysicalCondition));
		foreach (EPhysicalCondition ePhysicalCondition in array)
		{
			if ((conditionForCopy & ePhysicalCondition) == ePhysicalCondition)
			{
				SetPhysicalCondition(ePhysicalCondition, val: true);
			}
		}
	}

	public void GrounderSetActive(bool grounderIsOn)
	{
		_player.Grounder.solver.layers = (grounderIsOn ? GrounderMask : 0);
	}

	public void OnJump()
	{
		GrounderSetActive(grounderIsOn: false);
		_player.Physical.OnJump(enabled: true);
	}

	public void PlayBreachSound()
	{
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.PlaySound(_player, _player.Position, LocalBotSettingsProviderClass.Core.SOUND_DOOR_BREACH_METERS, AISoundType.step);
		}
	}

	public void OnBreach()
	{
		PlayBreachSound();
		_player.Physical.OnBreach();
	}

	public void OnJumpEnd()
	{
		float f = GClass2298.Evaluate(InertiaSettings.SpeedInertiaAfterJump, Inertia);
		float num = InertiaSettings.BaseJumpPenalty + Mathf.Pow(f, InertiaSettings.PenaltyPower) - 1f;
		float duration = InertiaSettings.BaseJumpPenaltyDuration + Mathf.Pow(f, InertiaSettings.DurationPower) - 1f;
		float num2 = (1f - num) * MaxSpeed;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		ChangeSpeedLimit(num2, Player.ESpeedLimit.Fall, duration);
		GrounderSetActive(grounderIsOn: true);
		_player.Physical.OnJump(enabled: false);
	}

	public Vector3 RibcagePosition()
	{
		return _player.PlayerBones.Ribcage.position;
	}

	public Transform RootJointTransform()
	{
		return _player.PlayerBones.RootJoint;
	}

	public Vector2 CalculateLookAtDirection(Vector3 target, Vector3 playerPos, float relativeRootHeight = 0f)
	{
		float y = ((relativeRootHeight > 0f) ? (_player.Position.y + relativeRootHeight) : _player.PlayerBones.Ribcage.position.y);
		Vector3 vector = new Vector3(playerPos.x, y, playerPos.z);
		Vector3 eulerAngles = Quaternion.LookRotation((target - vector).normalized, Vector3.up).eulerAngles;
		return new Vector2(eulerAngles.y, Mathf.DeltaAngle(0f, eulerAngles.x));
	}

	public void AddStateSpeedLimit(float speedLimit, Player.ESpeedLimit cause)
	{
		if (!SpeedLimits.ContainsKey(cause))
		{
			SpeedLimits.Add(cause, speedLimit);
			method_5();
		}
	}

	public void AddStateSpeedLimit(float speedLimit, Player.ESpeedLimit cause, float duration)
	{
		SpeedLimitTimings.Add(cause, Time.realtimeSinceStartup + duration);
		AddStateSpeedLimit(speedLimit, cause);
	}

	public void OnEnterObstacle(ObstacleCollider obstacle)
	{
		if (EnteredObstacles.IndexOf(obstacle) == -1)
		{
			EnteredObstacles.Add(obstacle);
			method_0();
		}
	}

	public void OnExitObstacle(ObstacleCollider obstacle)
	{
		EnteredObstacles.Remove(obstacle);
		method_0();
	}

	public void method_0()
	{
		Struct333 struct333_ = default(Struct333);
		struct333_.movementContext_0 = this;
		struct333_.conditions = EPhysicalCondition.None;
		bool flag = false;
		foreach (ObstacleCollider enteredObstacle in EnteredObstacles)
		{
			struct333_.conditions |= enteredObstacle.ConditionsMask;
			flag |= enteredObstacle.HasSwampSpeedLimit;
		}
		method_28(EPhysicalCondition.ProneDisabled, ref struct333_);
		method_28(EPhysicalCondition.ProneMovementDisabled, ref struct333_);
		method_28(EPhysicalCondition.SprintDisabled, ref struct333_);
		method_28(EPhysicalCondition.JumpDisabled, ref struct333_);
		if (PhysicalConditionIs(EPhysicalCondition.SprintDisabled))
		{
			EnableSprint(enable: false);
		}
		if (flag)
		{
			AddStateSpeedLimit(0.2f, Player.ESpeedLimit.Swamp);
		}
		else
		{
			RemoveStateSpeedLimit(Player.ESpeedLimit.Swamp);
		}
	}

	public void method_1(Vector3 motion)
	{
		if (_player.AIData == null || (_player.AIData.IsAI && _player.AIData.BotOwner.BotState != EBotState.Active) || !(Time.time > NextStepNoise))
		{
			return;
		}
		if (motion.y < 0.2f && motion.y > -0.2f)
		{
			motion.y = 0f;
		}
		NextStepNoise = Time.time + LocalBotSettingsProviderClass.Core.STEP_NOISE_DELTA;
		float num = 0f;
		num = _player.Speed;
		if (_player.IsSprintEnabled)
		{
			num = 2f;
		}
		float num2 = Mathf.Clamp(0.5f * _player.PoseLevel + 0.5f, 0f, 1f);
		num *= num2;
		if (!(motion.sqrMagnitude < 1E-06f))
		{
			float num3 = (_player.IsSprintEnabled ? 1f : CovertMovementVolumeBySpeed);
			float power = LocalBotSettingsProviderClass.Core.BASE_WALK_SPEREAD2 * (num3 + num) / 2f;
			if (Singleton<BotEventHandler>.Instantiated)
			{
				Singleton<BotEventHandler>.Instance.PlaySound(_player, TransformPosition, power, AISoundType.step);
			}
		}
	}

	public void method_2(float powerCoef = 1f)
	{
		if (_player.AIData != null && (!_player.AIData.IsAI || _player.AIData.BotOwner.BotState == EBotState.Active) && Time.time > NextJumpNoise)
		{
			NextJumpNoise = Time.time + LocalBotSettingsProviderClass.Core.JUMP_NOISE_DELTA;
			float power = LocalBotSettingsProviderClass.Core.JUMP_SPREAD_DIST * powerCoef;
			if (Singleton<BotEventHandler>.Instantiated)
			{
				Singleton<BotEventHandler>.Instance.PlaySound(_player, TransformPosition, power, AISoundType.step);
			}
		}
	}

	public void method_3(float spreadDistance, Color color)
	{
		Vector3 start = TransformPosition + new Vector3(0f - spreadDistance, 1f, 0f);
		Vector3 start2 = TransformPosition + new Vector3(0f - spreadDistance, 1f, 0f);
		for (float num = 0f - spreadDistance; num <= spreadDistance; num += 0.5f)
		{
			float num2 = Mathf.Sqrt(spreadDistance * spreadDistance - num * num);
			Vector3 vector = TransformPosition + new Vector3(num, 1f, num2);
			Vector3 vector2 = TransformPosition + new Vector3(num, 1f, 0f - num2);
			Debug.DrawLine(start, vector, color, 2f);
			Debug.DrawLine(start2, vector2, color, 2f);
			start2 = vector2;
			start = vector;
		}
	}

	public void RemoveStateSpeedLimit(Player.ESpeedLimit cause)
	{
		if (SpeedLimits.Remove(cause))
		{
			method_5();
		}
	}

	public void method_4()
	{
		SpeedLimitIsDirty = false;
		StateSpeedLimit = 1f;
		StateSprintSpeedLimit = 1f;
		if (SpeedLimits.ContainsKey(Player.ESpeedLimit.Fall))
		{
			StateSprintSpeedLimit = SpeedLimits[Player.ESpeedLimit.Fall];
		}
		foreach (KeyValuePair<Player.ESpeedLimit, float> speedLimit in SpeedLimits)
		{
			if (speedLimit.Value <= StateSpeedLimit)
			{
				StateSpeedLimit = speedLimit.Value;
				_ = speedLimit.Key;
			}
		}
		NextSpeedLimitToExpire = (cause: Player.ESpeedLimit.SurfaceNormal, time: float.MaxValue);
		foreach (KeyValuePair<Player.ESpeedLimit, float> speedLimitTiming in SpeedLimitTimings)
		{
			if (speedLimitTiming.Value < NextSpeedLimitToExpire.time)
			{
				NextSpeedLimitToExpire = (cause: speedLimitTiming.Key, time: speedLimitTiming.Value);
			}
		}
		this.OnMaxSpeedChangedEvent?.Invoke(StateSpeedLimit, ClampedSpeed, MaxSpeed, CovertNoiseLevel);
	}

	public void method_5()
	{
		SpeedLimitIsDirty = true;
	}

	public void SetAimingSlowdown(bool isAiming, float slow)
	{
		if (isAiming)
		{
			AddStateSpeedLimit(slow, Player.ESpeedLimit.Aiming);
		}
		else
		{
			RemoveStateSpeedLimit(Player.ESpeedLimit.Aiming);
		}
	}

	public float ClampSpeed(float speed)
	{
		return Mathf.Clamp(speed, 0f, StateSpeedLimit);
	}

	public static MovementContext Create(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
	{
		return Create<MovementContext>(player, animatorGetter, characterControllerGetter, groundMask);
	}

	public static TMovementContext Create<TMovementContext>(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask) where TMovementContext : MovementContext, new()
	{
		TMovementContext obj = new TMovementContext
		{
			_player = player,
			GroundMask_1 = groundMask,
			GroundMaskPlusWater = GClass1458.AddToMask(groundMask, "Water"),
			GroundMaskPlusPlayer = GClass1458.AddToMask(groundMask, "Player"),
			GrounderMask = player.Grounder.solver.layers,
			PlayerTransform_1 = player.PlayerBones.BodyTransform,
			PlayerAnimator_1 = new PlayerAnimator(animatorGetter),
			AverageRotationSpeed = new RollingMedian(10),
			YawLimit_1 = Player.PlayerMovementConstantsClass.FULL_YAW_RANGE,
			PitchLimit_1 = Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE,
			PitchTargetLimit = Player.PlayerMovementConstantsClass.STAND_POSE_ROTATION_PITCH_RANGE,
			CharacterControllerGetter = characterControllerGetter
		};
		IPlayerStateContainerBehaviour[] array = (BackendConfigAbstractClass.Config.UseSpiritPlayer ? player.Spirit.BodyAnimatorWrapper.GetBehaviours<IPlayerStateContainerBehaviour>() : player._animators[0].GetBehaviours<IPlayerStateContainerBehaviour>());
		smethod_0();
		obj.InitMovementStates(array);
		obj.method_20(array);
		obj.Init();
		obj.InitComponents();
		obj.ResetFlying();
		return obj;
	}

	public static void smethod_0()
	{
		EFTHardSettings instance = EFTHardSettings.Instance;
		BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
		instance.SpeedLimitAfterFall.MoveKey(0, new Keyframe(inertia.SpeedLimitAfterFallMin.x, inertia.SpeedLimitAfterFallMin.y));
		instance.SpeedLimitAfterFall.MoveKey(1, new Keyframe(inertia.SpeedLimitAfterFallMax.x, inertia.SpeedLimitAfterFallMax.y));
		instance.SpeedLimitDuration.MoveKey(0, new Keyframe(inertia.SpeedLimitDurationMin.x, inertia.SpeedLimitDurationMin.y));
		instance.SpeedLimitDuration.MoveKey(1, new Keyframe(inertia.SpeedLimitDurationMax.x, inertia.SpeedLimitDurationMax.y));
		instance.InertiaTiltCurve.MoveKey(0, new Keyframe(inertia.InertiaTiltCurveMin.x, inertia.InertiaTiltCurveMin.y));
		instance.InertiaTiltCurve.MoveKey(1, new Keyframe(inertia.InertiaTiltCurveMax.x, inertia.InertiaTiltCurveMax.y));
		instance.sprintSpeedInertiaCurve.MoveKey(0, new Keyframe(inertia.SprintSpeedInertiaCurveMin.x, inertia.SprintSpeedInertiaCurveMin.y));
		instance.sprintSpeedInertiaCurve.MoveKey(1, new Keyframe(inertia.SprintSpeedInertiaCurveMax.x, inertia.SprintSpeedInertiaCurveMax.y));
	}

	public virtual void InitComponents()
	{
		method_6();
	}

	public void method_6()
	{
		GClass2785 obstacleCollisionFacade_ = new GClass2785(PlayerTransform_1.Original, () => ActualLinearVelocity, () => MovementDirection, () => HandsToBodyAngle_1);
		ObstacleCollisionFacade_1 = obstacleCollisionFacade_;
		_player.UpdateEvent += ObstacleCollisionFacade_1.Tick;
	}

	public virtual void Init()
	{
		PickUpState = GetNewState(EPlayerState.Pickup);
		PickUpState.Name = EPlayerState.Pickup;
		PickUpState.AnimatorStateHash = -1;
		PlantState = GetNewState(EPlayerState.Plant);
		PlantState.Name = EPlayerState.Plant;
		PlantState.AnimatorStateHash = -1;
		IdleWeaponMountingState = GetNewState(EPlayerState.IdleWeaponMounting);
		IdleWeaponMountingState.Name = EPlayerState.IdleWeaponMounting;
		IdleWeaponMountingState.AnimatorStateHash = -1;
		SetCharacterMovementSpeed(MaxSpeed, force: true);
		SetPoseLevel(1f, force: true);
		SmoothedPoseLevel = 1f;
		AdjustCharacterController(prone: false);
		SetTilt(0f, force: true);
		Step = 0;
		IsInPronePose_1 = false;
		PlayerAnimatorEnableProne(IsInPronePose_1);
		_player.Physical.WeightRelatedValuesUpdated += WeightRelatedValuesUpdated;
		LeftStanceController_1 = new LeftStanceController(method_24, method_25, _player.HasFirearmInHands);
		UnsubscribeOnStrengthSkillLevelChanged = SkillManager.Strength.SkillLevelChanged.Subscribe(method_23);
	}

	public virtual void WeightRelatedValuesUpdated()
	{
		if (_player.ProceduralWeaponAnimation != null)
		{
			_player.ProceduralWeaponAnimation.Overweight = _player.Physical.Overweight;
			_player.ProceduralWeaponAnimation.UpdateSwayFactors();
			_player.ProceduralWeaponAnimation.UpdateSwaySettings();
			_player.ProceduralWeaponAnimation.WeaponFlipSpeed = GClass2298.Evaluate(InertiaSettings.WeaponFlipSpeed, _player.Physical.Inertia);
		}
		UpdateCovertEfficiency(_player.MovementContext.ClampedSpeed, force: true);
		TiltInertia = EFTHardSettings.Instance.InertiaTiltCurve.Evaluate(_player.Physical.Inertia);
		WalkInertia = GClass2298.Evaluate(InertiaSettings.WalkInertia, _player.Physical.Inertia);
		SprintBrakeInertia = GClass2298.Evaluate(InertiaSettings.SprintBrakeInertia, _player.Physical.Inertia);
		_player.HealthController.FallSafeHeight = Mathf.Lerp(Singleton<BackendConfigSettingsClass>.Instance.Health.Falling.SafeHeight, Singleton<BackendConfigSettingsClass>.Instance.Stamina.SafeHeightOverweight, _player.Physical.Overweight);
		PlayerAnimatorTransitionSpeed = TransitionSpeed;
		if (PoseLevel > _player.Physical.MaxPoseLevel && CurrentState is MovementState movementState)
		{
			movementState.ChangePose(_player.Physical.MaxPoseLevel - PoseLevel);
		}
		if (_player.PoseMemo > _player.Physical.MaxPoseLevel)
		{
			_player.PoseMemo = _player.Physical.MaxPoseLevel;
		}
		float walkSpeedLimit = _player.Physical.WalkSpeedLimit;
		RemoveStateSpeedLimit(Player.ESpeedLimit.Weight);
		if (walkSpeedLimit < 1f)
		{
			AddStateSpeedLimit(walkSpeedLimit * MaxSpeed, Player.ESpeedLimit.Weight);
		}
		UpdateCharacterControllerSpeedLimit();
	}

	public virtual void Dispose()
	{
		IPlayerStateContainerBehaviour[] movementStates = MovementStates;
		for (int i = 0; i < movementStates.Length; i++)
		{
			movementStates[i].Deinit();
		}
		_player.Physical.WeightRelatedValuesUpdated -= WeightRelatedValuesUpdated;
		UnsubscribeOnStrengthSkillLevelChanged?.Invoke();
		RotationAction = null;
		UnsubFromSprintExit?.Invoke();
		GClass782_0?.Dispose();
		LeftStanceController_1.Clear();
		_player.UpdateEvent -= ObstacleCollisionFacade_1.Tick;
		PlayerAnimator.Dispose();
	}

	public void InitMovementStates(IPlayerStateContainerBehaviour[] playerStates)
	{
		MovementStates = playerStates;
		IPlayerStateContainerBehaviour[] movementStates = MovementStates;
		for (int i = 0; i < movementStates.Length; i++)
		{
			movementStates[i].Init(GetNewStateFabric);
		}
		BaseMovementState encapsulatedState = MovementStates[0].EncapsulatedState;
		for (int j = 0; j < MovementStates.Length; j++)
		{
			IPlayerStateContainerBehaviour playerStateContainerBehaviour = MovementStates[j];
			if (playerStateContainerBehaviour.DefaultState)
			{
				encapsulatedState = playerStateContainerBehaviour.EncapsulatedState;
				break;
			}
		}
		CurrentState = encapsulatedState;
	}

	public virtual void Flash(ref Vector3 motion)
	{
	}

	public virtual void ApplyApproachMotion(Vector3 motion, float deltaTime)
	{
		ApplyMotion(motion, deltaTime);
	}

	public virtual void ApplyMotion(Vector3 motion, float deltaTime)
	{
		ScheduleDirectApplyMotion = true;
		ScheduledMotion = motion;
		ScheduledDeltaTime = deltaTime;
	}

	public void AnimatorStatesLateUpdate()
	{
		if (ScheduleDirectApplyMotion)
		{
			ScheduleDirectApplyMotion = false;
			DirectApplyMotion(ScheduledMotion, ScheduledDeltaTime);
		}
	}

	public virtual void DirectApplyMotion(Vector3 motion, float deltaTime)
	{
		_ = TransformPosition;
		InputMotion = motion / deltaTime;
		Flash(ref motion);
		float speedLimit = CharacterController.SpeedLimit;
		bool num = PlatformMotion != Vector3.zero;
		if (num)
		{
			CharacterController.SpeedLimit = -1f;
		}
		CollisionFlags arg = CharacterController.Move(motion + PlatformMotion, deltaTime);
		if (num)
		{
			CharacterController.SpeedLimit = speedLimit;
		}
		if (Platform == null)
		{
			method_8(deltaTime);
		}
		method_1(motion);
		if (GClass782_0 != null && GClass782_0.UseAsyncMoveValidation)
		{
			ImpostorCCLateUpdRequested = true;
		}
		else
		{
			this.OnMotionApplied?.Invoke(arg, motion);
		}
	}

	public void method_7()
	{
		if (ImpostorCCLateUpdRequested)
		{
			ImpostorCCLateUpdRequested = false;
			CollisionFlags arg = GClass782_0.UpdateDelayedMoveLogic();
			this.OnMotionApplied?.Invoke(arg, Vector3.zero);
		}
	}

	public bool ValidateMotion()
	{
		if (MotionFrame == Time.frameCount)
		{
			return false;
		}
		MotionFrame = Time.frameCount;
		return true;
	}

	public void method_8(float deltaTime)
	{
		PlatformMotion = GClass842.Vector3Zero;
	}

	public virtual void ApplyRotation(Quaternion rotation)
	{
		PlayerTransform_1.rotation = rotation;
		HandsToBodyAngle_1 = Mathf.DeltaAngle(360f + PlayerTransform_1.eulerAngles.y, 360f + Yaw);
		_player.MyHandsToBodyAngle = HandsToBodyAngle_1;
		if (CurrentState.AnimatorStateHash == PlayerAnimator.TURN_LEFT_90_AIM_HASH)
		{
			HandsToBodyAngle_1 += _player.BodyAnimatorCommon.GetFloat(PlayerAnimator.TURN);
		}
		else if (CurrentState.AnimatorStateHash == PlayerAnimator.TURN_RIGHT_90_AIM_HASH)
		{
			HandsToBodyAngle_1 -= _player.BodyAnimatorCommon.GetFloat(PlayerAnimator.TURN);
		}
		UpdateAnimatorRotationParameters();
	}

	public void UpdateStationaryDeltaAngle()
	{
		UpdateDeltaAngle();
	}

	public virtual void UpdateDeltaAngle()
	{
		HandsToBodyAngle_1 = Mathf.DeltaAngle(360f + PlayerTransform_1.eulerAngles.y, 360f + Yaw);
		UpdateAnimatorRotationParameters();
	}

	public virtual void UpdateAnimatorRotationParameters()
	{
		PlayerAnimatorSetDeltaRotation(HandsToBodyAngle_1);
		PlayerAnimatorSetAimAngle(Pitch - SurfacePitch_1);
	}

	public Vector3 TransformVector(Vector3 vector3)
	{
		return PlayerTransform_1.TransformVector(vector3);
	}

	public Vector3 InverseTransformVector(Vector3 vector3)
	{
		return PlayerTransform_1.InverseTransformVector(vector3);
	}

	public Vector3 InverseTransformPoint(Vector3 vector3)
	{
		return PlayerTransform_1.InverseTransformPoint(vector3);
	}

	public void AddRotaionDelta(float rotationDelta)
	{
		AverageRotationSpeed.AddValue(rotationDelta);
	}

	public void UpdatePoseAfterProne()
	{
		SetPoseLevel(_player.PoseMemo);
	}

	public bool SetPoseLevel(float poseLevel, bool force = false)
	{
		if (CurrentState.Name == EPlayerState.ClimbOver || CurrentState.Name == EPlayerState.ClimbUp)
		{
			return false;
		}
		if (Math.Abs(PoseLevel_1 - poseLevel) < float.Epsilon)
		{
			return true;
		}
		if (!CanStandAt(poseLevel))
		{
			return false;
		}
		bool prone = CurrentState.Name == EPlayerState.ProneIdle || CurrentState.Name == EPlayerState.ProneMove || CurrentState.Name == EPlayerState.Transit2Prone;
		CalculatePoseLevelValues(poseLevel, _player.Physical.MaxPoseLevel, CharacterController.center, CharacterController.skinWidth, prone, out PoseLevel_1, out var characterControllerHeight, out var characterControllerCenterResult);
		if (!force && PoseLevel_1 >= _player.Physical.MaxPoseLevel && StraightenTimer_1 + 2f > Time.time)
		{
			_player.ExecuteSkill((Action)delegate
			{
				_player.Skills.PushUp.Complete(new SkillManager.GStruct279
				{
					Overweight = Overweight
				});
			});
		}
		this.OnSmoothedPoseLevelChange?.Invoke(IsInPronePose ? (-0.35f) : PoseLevel_1, CovertNoiseLevel);
		CharacterController.height = characterControllerHeight;
		CharacterController.center = characterControllerCenterResult;
		this.OnPoseChanged?.Invoke(PoseToInt);
		if (force)
		{
			SmoothedPoseLevel = PoseLevel_1;
		}
		return true;
	}

	public static void CalculatePoseLevelValues(float poseLevel, float maxPoseLevel, Vector3 characterControllerCenter, float characterControllerSkinWidth, bool prone, out float poseLevelResult, out float characterControllerHeight, out Vector3 characterControllerCenterResult)
	{
		poseLevelResult = Mathf.Clamp(poseLevel, 0f, maxPoseLevel);
		characterControllerHeight = (prone ? 0.3f : Mathf.Lerp(1.2f, 1.6f, poseLevelResult));
		characterControllerCenterResult = new Vector3(characterControllerCenter.x, prone ? (0.35f + characterControllerSkinWidth) : Mathf.Lerp(CCCenterPoseLevel.x + characterControllerSkinWidth, CCCenterPoseLevel.y + characterControllerSkinWidth, poseLevelResult), characterControllerCenter.z);
	}

	public static void CalculatePoseLevelValues(float poseLevel, Vector3 characterControllerCenter, float characterControllerSkinWidth, bool prone, out float characterControllerHeight, out Vector3 characterControllerCenterResult)
	{
		characterControllerHeight = (prone ? 0.3f : Mathf.Lerp(1.2f, 1.6f, poseLevel));
		characterControllerCenterResult = new Vector3(characterControllerCenter.x, prone ? (0.35f + characterControllerSkinWidth) : Mathf.Lerp(CCCenterPoseLevel.x + characterControllerSkinWidth, CCCenterPoseLevel.y + characterControllerSkinWidth, poseLevel), characterControllerCenter.z);
	}

	public void AdjustCharacterController(bool prone)
	{
		CalculatePoseLevelValues(PoseLevel_1, _player.Physical.MaxPoseLevel, CharacterController.center, CharacterController.skinWidth, prone, out PoseLevel_1, out var characterControllerHeight, out var characterControllerCenterResult);
		CharacterController.center = characterControllerCenterResult;
		CharacterController.height = characterControllerHeight;
		CharacterController.stepOffset = (prone ? 0.15f : 0.25f);
		CharacterController.slopeLimit = (prone ? 45f : 60f);
	}

	public Vector3 PlayerColliderCenterOfHeight(float h)
	{
		return PlayerTransform.TransformPoint(new Vector3(CharacterController.center.x, h, CharacterController.center.z));
	}

	public Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight)
	{
		return PlayerTransform.TransformPoint(new Vector3(CharacterController.center.x, CharacterController.height * relativeHeight, CharacterController.center.z));
	}

	public virtual void SmoothPoseLevel(float deltaTime)
	{
		float num;
		if (_currentPoseInertia > float.Epsilon)
		{
			num = Mathf.Max(Mathf.Min(PoseLevel_1, 0.1f), PoseLevel_1 - _currentPoseInertia);
			_currentPoseInertia = PoseInertia * EFTHardSettings.Instance.PoseInertiaDamp.Evaluate(Time.time - InertiaAppliedTime);
		}
		else
		{
			num = PoseLevel_1;
		}
		float num2 = num - SmoothedPoseLevel;
		if (GClass855.IsZero(num2))
		{
			return;
		}
		if (Mathf.Abs(num2) > 0.001f)
		{
			float t = ((num2 > 0f) ? (deltaTime * EFTHardSettings.Instance.POSE_CHANGING_SPEED * _player.Physical.PoseLevelIncreaseSpeed.Value) : (deltaTime * EFTHardSettings.Instance.POSE_CHANGING_SPEED * _player.Physical.PoseLevelDecreaseSpeed.Value));
			float num3 = Mathf.Lerp(SmoothedPoseLevel, num, t);
			num2 = num3 - SmoothedPoseLevel;
			if (num2 > 0f)
			{
				AccumulatedDeltaPose += num2;
				int num4 = (int)(AccumulatedDeltaPose / EFTHardSettings.Instance.DELTA_LEVEL / 3f);
				if (num4 > 0)
				{
					_player.Physical.ConsumePoseLevelChange((float)num4 / 3f);
					AccumulatedDeltaPose -= (float)num4 * EFTHardSettings.Instance.DELTA_LEVEL / 3f;
				}
			}
			SmoothedPoseLevel = num3;
		}
		else
		{
			SmoothedPoseLevel = PoseLevel_1;
		}
	}

	public virtual float GetPenetrationDepth(Vector3? axis = null, float extraCastLn = 0f)
	{
		Ray ray = new Ray(PlayerColliderCenter, axis ?? ForwardProjectionOnSurface);
		float num = CharacterController.radius + extraCastLn;
		Debug.DrawRay(ray.origin, ray.direction * num, Color.yellow, 5f);
		if (Physics.Raycast(ray, out var hitInfo, num, GroundMask_1, QueryTriggerInteraction.Ignore))
		{
			return num - hitInfo.distance;
		}
		return 0f;
	}

	public virtual float GetSpherePenetrationDepth(Vector3? axis = null, float radius = 0.1f, float extraCastLn = 0f)
	{
		Ray ray = new Ray(PlayerColliderCenter, axis ?? ForwardProjectionOnSurface);
		float num = CharacterController.radius + extraCastLn;
		if (Physics.SphereCast(ray, radius, out var hitInfo, num, GroundMaskPlusWater, QueryTriggerInteraction.Ignore))
		{
			return num - hitInfo.distance;
		}
		return 0f;
	}

	public virtual bool CanProneTilt(int direction)
	{
		if (!CanRoll(direction, EFTHardSettings.Instance.ProneTiltCheckShift, EFTHardSettings.Instance.ProneTiltCheckSize))
		{
			return CanRoll(-direction, EFTHardSettings.Instance.ProneTiltCheckShift, EFTHardSettings.Instance.ProneTiltCheckSize);
		}
		return true;
	}

	public virtual bool CanRoll(int direction)
	{
		return CanRoll(direction, EFTHardSettings.Instance.RollCheckShift, EFTHardSettings.Instance.RollCheckSize);
	}

	public bool CanRoll(int direction, Vector3 shift, Vector3 size)
	{
		float rollCheckGroundCastLength = EFTHardSettings.Instance.RollCheckGroundCastLength;
		shift.x = ((direction > 0) ? Mathf.Abs(shift.x) : (0f - Mathf.Abs(shift.x)));
		Quaternion quaternion = Quaternion.Euler(0f, Yaw, 0f) * Quaternion.Euler(SurfacePitch, 0f, 0f);
		Vector3 vector = new Vector3(TransformPosition.x, RibcagePosition().y, TransformPosition.z) + quaternion * shift;
		if (LegacyTwoPointSteadyGroundCheck(vector, rollCheckGroundCastLength))
		{
			return false;
		}
		return !Physics.CheckBox(vector, size / 2f, quaternion, GroundMaskPlusWater, QueryTriggerInteraction.Ignore);
	}

	public virtual bool HeadBump(float velocity)
	{
		velocity = Mathf.Max(0f, velocity);
		return Physics.CheckSphere(_player.Position + (CharacterController.height - 0.2f + 0.1f + velocity * 2f) * Vector3.up, 0.2f, GroundMask_1, QueryTriggerInteraction.Ignore);
	}

	public bool LegacyTwoPointSteadyGroundCheck(Vector3 origin, float castLn)
	{
		float num = 0f;
		if (Physics.Raycast(origin, Vector3.down, out PredictionHit, castLn, GroundMaskPlusWater))
		{
			if (PredictionHit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				return true;
			}
			num = PredictionHit.normal.y;
		}
		if (num < 0.707f && Physics.Raycast(origin + PlayerTransform.forward * 0.1f, Vector3.down, out PredictionHit, castLn, GroundMask_1))
		{
			num = PredictionHit.normal.y;
		}
		return num < 0.707f;
	}

	public virtual bool OverlapOrHasNoGround(float depth, Vector3? axis = null, float width = 0f, float heightDivider = 4f, float extraCastLn = 0f)
	{
		Vector3 vector = axis ?? ForwardProjectionOnSurface;
		Vector3 vector2 = PlayerColliderCenter + (CharacterController.radius + depth / 2f) * vector;
		width = ((width > 0f) ? width : (CharacterController.radius / 2f));
		heightDivider = CharacterController.height / heightDivider;
		float num = CharacterController.height / 2f + 0.3f + (1f - SurfaceNormal.y);
		if (Physics.OverlapBoxNonAlloc(vector2, new Vector3(width, heightDivider, depth / 2f), OverlapColliders, _player.PlayerBones.AnimatedTransform.rotation, GroundMaskPlusWater, QueryTriggerInteraction.Ignore) <= 0)
		{
			return LegacyTwoPointSteadyGroundCheck(vector2, num + extraCastLn);
		}
		return true;
	}

	public virtual bool HasGround(float depth, Vector3? axis = null, float extraCastLn = 0f)
	{
		Vector3 vector = axis ?? ForwardProjectionOnSurface;
		Vector3 origin = PlayerColliderCenter + (CharacterController.radius + depth / 2f) * vector;
		float num = CharacterController.height / 2f + 0.3f + (1f - SurfaceNormal.y);
		return !LegacyTwoPointSteadyGroundCheck(origin, num + extraCastLn);
	}

	public void SetBlindFire(float blind)
	{
		PlayerAnimatorSetBlindFire(blind);
	}

	public void SetTilt(float tilt, bool force = false)
	{
		float tilt_ = Tilt_1;
		Tilt_1 = Mathf.Clamp(tilt, -5f, 5f);
		if (force)
		{
			SmoothedTilt = Tilt_1;
		}
		if (this.OnTiltChanged != null && !Mathf.Approximately(tilt_, Tilt_1))
		{
			this.OnTiltChanged(Tilt_1);
		}
	}

	public void UpdateCovertEfficiency(float speed, bool force = false)
	{
		if (force || !((double)Mathf.Abs(speed - LastSpeedCovertCheck) < 0.01))
		{
			float num = 0.4f * (float)_player.Skills.CovertMovementSpeed;
			CovertEfficiency = (PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged) ? 0f : (Mathf.InverseLerp(num, num - 0.2f, speed / MaxSpeed) * (1f - _player.Physical.WalkOverweight)));
			LastSpeedCovertCheck = speed;
		}
	}

	public virtual void SetCharacterMovementSpeed(float characterMovementSpeed, bool force = false)
	{
		CharacterMovementSpeed = Mathf.Clamp(characterMovementSpeed, 0f, MaxSpeed);
		UpdateCovertEfficiency(ClampedSpeed);
		RelativeSpeed = CharacterMovementSpeed / MaxSpeed;
		if (force)
		{
			CharacterMovementSpeed = characterMovementSpeed;
			SmoothedCharacterMovementSpeed = ClampedSpeed;
		}
	}

	public Vector2 ClampRotation(Vector2 rotation)
	{
		return new Vector2(GClass842.ClampAngle360Sensitive(rotation.x, YawLimit_1.x, YawLimit_1.y), GClass842.ClampAngle(rotation.y, PitchLimit_1.x + SurfacePitch_1, PitchLimit_1.y + SurfacePitch_1));
	}

	public void method_9(Vector2 rotation)
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
		{
			_player.Spirit.LookRotation = rotation;
		}
		else
		{
			Rotation_1 = rotation;
		}
		method_10();
	}

	public void method_10()
	{
		LookDirection_1 = Quaternion.Euler(Rotation.y, Rotation.x, 0f) * Vector3.forward;
		PlayerRealForward_1 = Quaternion.Euler(0f, Rotation.x, 0f) * Vector3.forward;
	}

	public void method_11(Vector2 rotation)
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
		{
			_player.Spirit.PreviousLookRotation = rotation;
		}
		else
		{
			PreviousRotation_1 = rotation;
		}
	}

	public void SetDirectlyLookRotations(Vector2 current, Vector2 previous)
	{
		Rotation_1 = current;
		PreviousRotation_1 = previous;
		method_10();
	}

	public void RestorePreviousYaw()
	{
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && _player.Spirit.IsActive)
		{
			_player.Spirit.LookRotation.x = _player.Spirit.PreviousLookRotation.x;
		}
		else
		{
			Rotation_1.x = PreviousRotation_1.x;
		}
		method_10();
	}

	public void SetYawLimit(Vector2 yawLimit)
	{
		YawLimit_1 = yawLimit;
		if (!(YawLimit_1.x > 360f) && YawLimit_1.y <= 360f)
		{
			if (YawLimit_1.x < -360f || YawLimit_1.y < -360f)
			{
				YawLimit_1 = new Vector2(YawLimit_1.x + 360f, YawLimit_1.y + 360f);
			}
		}
		else
		{
			YawLimit_1 = new Vector2(YawLimit_1.x - 360f, YawLimit_1.y - 360f);
		}
	}

	public void SetRotationLimit(Vector2 yawLimit, Vector2 pitchLimit)
	{
		YawLimit_1 = yawLimit;
		PitchLimit_1 = (PitchTargetLimit = pitchLimit);
	}

	public void SetPitchSmoothly(Vector2 pitchLimit)
	{
		PitchTargetLimit = pitchLimit;
	}

	public void SetPitchSmoothly(float upPith, float downPitch)
	{
		PitchTargetLimit.x = upPith;
		PitchTargetLimit.y = downPitch;
	}

	public void SetPitchForce(float upPith, float downPitch)
	{
		PitchLimit_1.x = upPith;
		PitchLimit_1.y = downPitch;
	}

	public virtual void ManualUpdate(float deltaTime)
	{
		if (!_player.HealthController.IsAlive)
		{
			return;
		}
		if (Mathf.Approximately(deltaTime, 0f))
		{
			Debug.LogErrorFormat("[ServerPlayer.ManualUpdate] deltaTime = {0}", deltaTime);
			return;
		}
		LastDeltaTime = deltaTime;
		SpeedLimiter?.Update(deltaTime);
		UpdateGroundCollision(deltaTime);
		SmoothPoseLevel(deltaTime);
		if (_player.Physical.Sprinting)
		{
			PreSprintAcceleration(deltaTime);
		}
		ProcessSpeedLimits(deltaTime);
		method_13(deltaTime);
		SmoothPitchLimitations(deltaTime);
		if (Mathf.Abs(Tilt_1) > 0f)
		{
			_player.ProceduralWeaponAnimation.UpdatePossibleTilt(SmoothedCharacterMovementSpeed, SmoothedPoseLevel);
		}
	}

	public void LateFixedUpdate()
	{
		method_7();
		PlayerAnimator.EventsDispatcher.EmitEvents();
	}

	public virtual void SmoothPitchLimitations(float deltaTime)
	{
		if (Vector2.Distance(PitchLimit_1, PitchTargetLimit) < 0.1f)
		{
			PitchLimit_1 = PitchTargetLimit;
		}
		else
		{
			PitchLimit_1 = Vector2.Lerp(PitchLimit_1, PitchTargetLimit, deltaTime * 5f);
		}
	}

	public virtual void PreSprintAcceleration(float deltaTime)
	{
		if (!_player.UsedSimplifiedSkeleton)
		{
			if (MovementDirection.y < 0.1f)
			{
				EnableSprint(enable: false);
			}
			else if (!(SmoothedCharacterMovementSpeed >= 1f))
			{
				float preSprintAcceleration = _player.Physical.PreSprintAcceleration;
				float characterMovementSpeed = (SmoothedCharacterMovementSpeed = Mathf.Clamp(SmoothedCharacterMovementSpeed + deltaTime * preSprintAcceleration, 0f, 1f));
				CharacterMovementSpeed = characterMovementSpeed;
				RaiseChangeSpeedEvent();
			}
		}
	}

	public void SprintAcceleration(float deltaTime)
	{
		float num = _player.Physical.SprintAcceleration * deltaTime;
		float num2 = (_player.Physical.SprintSpeed * SprintingSpeed + 1f) * StateSprintSpeedLimit;
		float num3 = Mathf.Max(EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate(Mathf.Abs((float)AverageRotationX.Average)), EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate(2.1474836E+09f) * (2f - _player.Physical.Inertia));
		num2 = Mathf.Clamp(num2 * num3, 0.1f, num2);
		SprintSpeed = Mathf.Clamp(SprintSpeed + num * Mathf.Sign(num2 - SprintSpeed), 0.01f, num2);
	}

	public virtual void ProcessSpeedLimits(float deltaTime)
	{
		if (SpeedLimitIsDirty)
		{
			method_4();
		}
		if (!(Time.realtimeSinceStartup < NextSpeedLimitToExpire.time))
		{
			SpeedLimitTimings.Remove(NextSpeedLimitToExpire.cause);
			RemoveStateSpeedLimit(NextSpeedLimitToExpire.cause);
		}
	}

	public virtual void CheckFlying(float deltaTime)
	{
		float y = TransformPosition.y;
		if (IsGrounded)
		{
			float num = StartFallingHeight - y;
			float num2 = y - StartFlyHeight;
			_player.ActiveHealthController?.HandleFall(num);
			StartFallingHeight = y;
			if (!PreviousGroundResult)
			{
				OnGrounded?.Invoke(num, num2);
				FallHeight = num;
				JumpHeight = num2;
			}
		}
		else
		{
			if (PreviousGroundResult)
			{
				StartFlyHeight = y;
			}
			if (y > StartFallingHeight)
			{
				StartFallingHeight = y;
			}
		}
		PreviousGroundResult = IsGrounded;
	}

	public virtual void ResetFlying()
	{
		StartFallingHeight = TransformPosition.y;
		StartFlyHeight = TransformPosition.y;
	}

	public void DetectSurfaceNormal()
	{
		if (!GroundHit_1.hasHit)
		{
			SurfaceNormal = Vector3_0;
		}
		else if (GroundHit_1.normal.y < SlopeTreshold)
		{
			Vector3 vector3_ = Vector3_0;
			SurfaceNormal = ((GroundHit_1.normal.y > vector3_.y) ? GroundHit_1.normal : vector3_);
		}
		else
		{
			SurfaceNormal = GroundHit_1.normal;
		}
		Debug.DrawRay(TransformPosition, SurfaceNormal, (SurfaceNormal.y < SlopeTreshold) ? Color.red : Color.green, Time.deltaTime);
		Debug.DrawRay(TransformPosition, PlayerSurfaceUpAlignNormal, Color.blue, Time.deltaTime);
	}

	public virtual void UpdateGroundCollision(float deltaTime)
	{
		ICharacterController characterController = CharacterController;
		if (!characterController.ShouldStickToGround)
		{
			IsGrounded = true;
			return;
		}
		Vector3 origin = PlayerTransform.TransformPoint(characterController.center);
		Ray ray = new Ray(origin, Vector3.down);
		float num = Mathf.Max(characterController.height / 2f, characterController.radius);
		float num2 = CheckGroundedRayDistance + characterController.skinWidth + num;
		float additionSearch = 0.25f;
		GroundCollisionRaycast(ray, num2, additionSearch, deltaTime, out var groundHit);
		bool flag = groundHit.collider != null && groundHit.distance <= num2;
		IsGrounded = characterController.isGrounded || flag;
		GroundHit_1 = groundHit;
		float val = Mathf.Clamp(groundHit.distance - num2, 0f, 0.3f) / 0.3f;
		if (!IsGrounded && groundHit.collider == null)
		{
			val = 1f;
		}
		else if (IsGrounded)
		{
			val = 0f;
		}
		if (!_player.UsedSimplifiedSkeleton)
		{
			PlayerAnimatorSetFallingDownFloat(val);
			PlayerAnimator.SetIsGrounded(IsGrounded);
		}
		DetectSurfaceNormal();
		NormalDotHistory.AddValue(SurfaceNormal.y);
		if (CanSlope)
		{
			if (flag && groundHit.collider.gameObject.layer == LayerMaskClass.PlayerLayer)
			{
				Vector3 position = groundHit.transform.position;
				Vector3 playerColliderCenter = PlayerColliderCenter;
				Slope = new Vector3(position.x - playerColliderCenter.x, 0f, position.z - playerColliderCenter.z).normalized * EFTHardSettings.Instance.HumanPyramidSlopeLength + Vector3.down;
				Debug.DrawRay(playerColliderCenter, Slope, Color.yellow, deltaTime, depthTest: false);
			}
			else if (NormalDotHistory.Avarage < SlopeTreshold)
			{
				Vector3 lhs = Vector3.Cross(SurfaceNormal, Vector3.up);
				Slope = Vector3.Cross(lhs, SurfaceNormal).normalized * (0.9f - SurfaceNormal.y);
			}
			else
			{
				Slope = Vector3.zero;
			}
		}
		CheckFlying(deltaTime);
		VerticalSpeed.AddValue(CharacterController.velocity.y);
	}

	public virtual bool GroundCollisionRaycast(Ray ray, float groundDistance, float additionSearch, float deltaTime, out RaycastHit groundHit)
	{
		float maxDistance = groundDistance + additionSearch;
		return EFTPhysicsClass.Raycast(ray, out groundHit, maxDistance, GroundMaskPlusPlayer);
	}

	public bool GroundCollisionSpherecast(Ray ray, float radius, float groundDistance, float additionSearch, out RaycastHit groundHit)
	{
		float num = groundDistance + additionSearch - radius;
		Vector3 start = ray.origin + Vector3.down * num;
		Debug.DrawRay(start, Vector3.down * radius, Color.blue);
		Debug.DrawRay(start, Vector3.left * radius, Color.blue);
		Debug.DrawRay(start, Vector3.right * radius, Color.blue);
		Debug.DrawRay(start, Vector3.forward * radius, Color.blue);
		Debug.DrawRay(start, Vector3.back * radius, Color.blue);
		return EFTPhysicsClass.SphereCast(ray, radius, out groundHit, num, GroundMask_1);
	}

	public float method_12(float deltaTime)
	{
		float num = 0f;
		for (float num2 = TiltStepMultiplier; num2 > 0f; num2 = Mathf.Clamp(num2 - TiltInertia, 0f, Single_2))
		{
			num += MaxTiltStep * num2 * deltaTime;
		}
		return Mathf.Abs(SmoothedTilt - Tilt_1) - num + 0.1f;
	}

	public void method_13(float deltaTime)
	{
		float tiltDiff = TiltDiff;
		if (!(tiltDiff < float.Epsilon))
		{
			if (_player.CurrentLeanType == Player.LeanType.NormalLean)
			{
				method_15(tiltDiff, deltaTime);
			}
			else
			{
				method_14(tiltDiff, deltaTime);
			}
			if (InMountedState)
			{
				_player.ProceduralWeaponAnimation.SmoothedTilt = MountedSmoothedTiltForCamera;
			}
			else
			{
				_player.ProceduralWeaponAnimation.SmoothedTilt = SmoothedTilt;
			}
		}
	}

	public void method_14(float smoothDiff, float deltaTime)
	{
		if (smoothDiff > 0.001f)
		{
			SmoothedTilt = Mathf.Lerp(SmoothedTilt, Tilt_1, deltaTime * EFTHardSettings.Instance.TILT_CHANGING_SPEED);
		}
		else
		{
			SmoothedTilt = Tilt_1;
		}
	}

	public void method_15(float smoothDiff, float deltaTime)
	{
		if (!PrevTilt.Equals(Tilt_1))
		{
			if (Mathf.Abs(TiltStepMultiplier) <= TiltInertia)
			{
				PrevTilt = Tilt_1;
				TiltStepMultiplier = 0f;
			}
			else
			{
				TiltStepMultiplier = Mathf.Clamp(TiltStepMultiplier - TiltInertia * Mathf.Sign(TiltStepMultiplier), 0f - Single_2, Single_2);
			}
		}
		else
		{
			TiltStepMultiplier = ((Tilt_1 == 0f) ? Mathf.Clamp(TiltStepMultiplier - TiltInertia * Mathf.Sign(method_12(deltaTime) * SmoothedTilt_1) * Single_0, 0f - Single_2, Single_2) : Mathf.Clamp(TiltStepMultiplier + TiltInertia * Mathf.Sign(method_12(deltaTime)), 0f - Single_2, Single_2));
		}
		if (smoothDiff > Mathf.Abs(Single_1 * deltaTime))
		{
			method_16(deltaTime);
			return;
		}
		SmoothedTilt = Tilt_1;
		TiltStepMultiplier = 0f;
	}

	public void method_16(float deltaTime)
	{
		SmoothedTilt = Mathf.Clamp(SmoothedTilt + Single_1 * deltaTime, -5f, 5f);
	}

	public Vector2 ApplyExternalSense(Vector2 deltaRotation)
	{
		return deltaRotation * _player.GetRotationMultiplier();
	}

	public bool IsHoldingBreath()
	{
		return _player.Physical.HoldingBreath;
	}

	public void EnableSprint(bool enable)
	{
		_player.Physical.Sprint(enable);
		if (enable && CanSprint)
		{
			PlayerAnimator.AnimatedInteractions.ForceStopInteractions();
			_player.RemoveLeftHandItem();
		}
		if (!enable)
		{
			RaiseChangeSpeedEvent();
		}
	}

	public void TryJump()
	{
		if (_player.Physical.CanJump && CanJump)
		{
			method_2();
			PlayerAnimatorEnableJump(enabled: true);
		}
	}

	public bool TryVaulting()
	{
		int num;
		if (_player.VaultingComponent != null && _player.VaultingGameplayRestrictions != null && _player.VaultingGameplayRestrictions.CanVaulting() && _player.IsForwardInputDirection)
		{
			num = (_player.VaultingComponent.TryVaulting() ? 1 : 0);
			if (num != 0)
			{
				_player.RemoveLeftHandItem(2f);
				_player.OnVaulting();
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	public void OnVault()
	{
		_player.Physical.OnVault();
		Vector3 vector = Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.VaultSettings.SpeedRange;
		method_17(vector, EVaultingStrategy.Vault);
	}

	public void OnClimb()
	{
		_player.Physical.OnClimb();
		Vector3 vector = Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.SpeedRange;
		method_17(vector, EVaultingStrategy.Climb);
	}

	public virtual void ApplyDamageByVaulting(float height, EVaultingStrategy strategy)
	{
		switch (strategy)
		{
		case EVaultingStrategy.Vault:
			if (_player.MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged) && height >= Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.VaultSettings.MaxWithoutHandHeight)
			{
				_player.ActiveHealthController?.ApplyDamage(EBodyPart.LeftLeg, 3f, GClass3051.FallDamage);
			}
			break;
		case EVaultingStrategy.Climb:
			if (_player.MovementContext.PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged) && height >= Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxWithoutHandHeight)
			{
				_player.ActiveHealthController?.ApplyDamage(EBodyPart.LeftLeg, 3f, GClass3051.FallDamage);
			}
			if (_player.MovementContext.PhysicalConditionIs(EPhysicalCondition.RightLegDamaged) && height >= Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings.ClimbSettings.MaxOneHandHeight)
			{
				_player.ActiveHealthController?.ApplyDamage(EBodyPart.RightLeg, 3f, GClass3051.FallDamage);
			}
			break;
		}
	}

	public void method_17(Vector2 speedRange, EVaultingStrategy strategy)
	{
		float b = 0f;
		BackendConfigSettingsClass.MovesSettings movesSettings = Singleton<BackendConfigSettingsClass>.Instance.VaultingSettings.MovesSettings;
		if (strategy == EVaultingStrategy.Vault && _player.VaultingParameters?.VaultingHeight > movesSettings.VaultSettings.MaxWithoutHandHeight)
		{
			if (PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged) || PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
			{
				b = (PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 0.75f : 1f);
			}
			if (PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged))
			{
				b = (PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 0.75f : 1f);
			}
		}
		else if (strategy == EVaultingStrategy.Climb && _player.VaultingParameters?.VaultingHeight > movesSettings.ClimbSettings.MaxWithoutHandHeight)
		{
			if (PhysicalConditionIs(EPhysicalCondition.LeftLegDamaged) || PhysicalConditionIs(EPhysicalCondition.RightLegDamaged))
			{
				b = (PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 0.75f : 1f);
			}
			if (PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged))
			{
				b = (PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 0.75f : 1f);
			}
			if (PhysicalConditionIs(EPhysicalCondition.RightArmDamaged) && _player.VaultingParameters?.VaultingHeight > movesSettings.ClimbSettings.MaxOneHandHeight)
			{
				b = (PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? 0.75f : 1f);
			}
		}
		switch ((PlayerPhysicalClass.EPose)PoseToInt)
		{
		default:
			PlayerAnimator.SetVaultingSpeed(GClass2298.Evaluate(speedRange, 0f));
			VaultingSpeed = GClass2298.Evaluate(speedRange, 0f);
			break;
		case PlayerPhysicalClass.EPose.Stand:
			VaultingSpeed = GClass2298.Evaluate(speedRange, Mathf.Max(_player.Physical.WalkOverweight, b));
			PlayerAnimator.SetVaultingSpeed(VaultingSpeed);
			break;
		case PlayerPhysicalClass.EPose.Sit:
			VaultingSpeed = GClass2298.Evaluate(speedRange, Mathf.Max(_player.Physical.Overweight, b));
			PlayerAnimator.SetVaultingSpeed(VaultingSpeed);
			break;
		}
	}

	public void HoldBreath(bool enable)
	{
		Player.AbstractHandsController handsController = _player.HandsController;
		if (handsController != null && handsController.IsAiming)
		{
			_player.Physical.HoldBreath(enable);
		}
		if (_player.Physical.HoldingBreath)
		{
			_player.Speaker.Shut();
		}
	}

	public void OverrideState(BaseMovementState enteredState)
	{
		ProcessStateEnter(enteredState);
		OverridenState = enteredState;
	}

	public void ExitOverridenState()
	{
		OverridenState = null;
		ProcessStateEnter(PreviousState);
	}

	public void ProcessStateEnter(BaseMovementState enteredState)
	{
		if (OverridenState != null)
		{
			PlayerAnimator_1.Animator.Play(PreviousState.AnimatorStateHash, 0, 0f);
			return;
		}
		BaseMovementState currentState = CurrentState;
		bool flag = smethod_1(enteredState, currentState);
		if (enteredState == currentState)
		{
			CurrentState.ReEnter();
			return;
		}
		if (!flag)
		{
			PreviousState = currentState;
		}
		currentState.Exit(flag);
		CurrentState = enteredState;
		CanUseProp.Value = CurrentState.CanInteract;
		enteredState.Enter(flag);
		_player.AuthorityBlender.Target = enteredState.AnimationAuthority;
		_player.AuthorityBlender.Speed = enteredState.AuthoritySpeed;
		if (!flag)
		{
			UpdateCharacterControllerSpeedLimit();
			this.OnStateChanged?.Invoke(PreviousState.Name, CurrentState.Name);
		}
	}

	public void EnterMountedState()
	{
		_player.RemoveLeftHandItem(3f);
		OverridenControlsState = IdleWeaponMountingState;
		OverridenControlsState.Enter(isFromSameState: false);
		PlayerMountingPointData.OnStartExitMountedState += StartExitingMountedState;
		HandsChangingEvent += ExitMountedState;
		InMountedState = true;
		if (IsInPronePose_1)
		{
			PlayerAnimator.SetProneBipodMount(isProneBipodMounted: true);
		}
		if (_player.HandsController is Player.FirearmController firearmController)
		{
			firearmController.FirearmsAnimator.SetMounted(mounted: true);
			firearmController.RecalculateErgonomic();
		}
		_player.ProceduralWeaponAnimation.SetMountingData(isMounting: true, PlayerMountingPointData.MountPointData.MountSideDirection != EMountSideDirection.Forward);
	}

	public void StartExitingMountedState()
	{
		if (OverridenControlsState is IdleWeaponMountingStateClass idleWeaponMountingStateClass)
		{
			_player.ProceduralWeaponAnimation.SetMountingData(isMounting: false);
			idleWeaponMountingStateClass.StartExiting();
			PlayerMountingPointData.OnStartExitMountedState -= StartExitingMountedState;
			_player.HandsController?.FirearmsAnimator.SetMounted(mounted: false);
		}
	}

	public void ExitMountedState()
	{
		if (OverridenControlsState == IdleWeaponMountingState)
		{
			InMountedState = false;
			HandsChangingEvent -= ExitMountedState;
			OverridenControlsState.Exit(toSameState: false);
			PlayerMountingPointData.ClearData();
			OverridenControlsState = null;
			_player.OnMounting(MountingPacketStruct.EMountingCommand.Exit);
			PlayerAnimator.SetProneBipodMount(isProneBipodMounted: false);
			if (_player.HandsController is Player.FirearmController firearmController)
			{
				firearmController.FirearmsAnimator.SetMounted(mounted: false);
				firearmController.RecalculateErgonomic();
			}
			_player.ProceduralWeaponAnimation.SetMountingData(isMounting: false);
		}
	}

	public void ToggleFirearmAimByBodyAction()
	{
		if (_player.HandsController.IsAiming && _player.HandsController is IFirearmHandsController firearmHandsController)
		{
			firearmHandsController.SetAim(value: false);
		}
	}

	public static bool smethod_1(BaseMovementState enteredState, BaseMovementState previousState)
	{
		return previousState.GetType() == enteredState.GetType();
	}

	public void SetPickupInHands(bool isPickup)
	{
		if (isPickup)
		{
			SetInteractInHands(EInteraction.TakeItem);
		}
		_player.HandsController.Pickup(isPickup);
	}

	public void SetSidebackSpeed(float value)
	{
		PlayerAnimator_1.SetSidebackSpeed(value);
	}

	public void SetSidestep(float value)
	{
		PlayerAnimator_1.SetSidestepFloat(value);
	}

	public void SetBlindFireFloat(float value)
	{
		PlayerAnimator_1.SetBlindFireFloat(value);
	}

	public float GetSidestep()
	{
		return PlayerAnimator_1.Animator.GetFloat(PlayerAnimator.SIDESTEP_FLOAT_HASH);
	}

	public void InteractWith(WorldInteractiveObject interactiveObject, InteractionResult interactionResult, Action callback)
	{
		if (interactionResult.InteractionType == EInteractionType.Breach)
		{
			method_18(interactiveObject, interactionResult, callback);
		}
		else
		{
			StartInteraction(interactiveObject, interactionResult, callback);
		}
	}

	public void ExecuteInteraction(WorldInteractiveObject interactiveObject, InteractionResult interactionResult)
	{
		_player.vmethod_1(interactiveObject, interactionResult);
	}

	public void StartInteraction(WorldInteractiveObject interactive, InteractionResult interactionResult, Action callback)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Start interaction with " + interactive);
		}
		InteractionInfo = new GStruct254
		{
			PlayerId = _player.PlayerId,
			Result = interactionResult,
			WorldInteractiveObject = interactive,
			Callback = callback
		};
		InteractionParameters = interactive.GetInteractionParameters(TransformPosition);
		if (IsInPronePose)
		{
			IsInPronePose = false;
			SetPoseLevel(0f);
		}
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.PlaySound(_player, _player.Position, LocalBotSettingsProviderClass.Core.SOUND_DOOR_OPEN_METERS, AISoundType.step);
		}
		bool approached = !InteractionParameters.Snap || !_player.CanBeSnapped;
		PlayerAnimatorSetInteraction(isInteracting: true, approached);
		interactive.LockForInteraction();
	}

	public void method_18(WorldInteractiveObject interactive, InteractionResult interactionResult, Action callback)
	{
		InteractionInfo = new GStruct254
		{
			PlayerId = _player.PlayerId,
			Result = interactionResult,
			WorldInteractiveObject = interactive,
			Callback = callback
		};
		Door door = interactive as Door;
		InteractionParameters = door.GetBreakInParameters(TransformPosition);
		bool approached;
		if (!(approached = !InteractionParameters.Snap) && IsInPronePose)
		{
			IsInPronePose = false;
			SetPoseLevel(0f);
		}
		PlayerAnimatorSetDoorBreak(isInteracting: true, approached);
		NextBreachResult = door.BreachSuccessRoll(InteractionParameters.InteractionPosition);
		PlayerAnimatorSetKickSucceed(NextBreachResult);
		interactive.LockForInteraction();
	}

	public void ReleaseDoorIfInteractingWithOne()
	{
		if (InteractionInfo.WorldInteractiveObject != null && InteractionInfo.WorldInteractiveObject.DoorState == EDoorState.Interacting)
		{
			InteractionInfo.WorldInteractiveObject.DoorState = InteractionInfo.WorldInteractiveObject.FallbackState;
		}
	}

	public Vector3 VelocityProjectionOnRealForward()
	{
		return Vector3.Project(Velocity, PlayerRealForward);
	}

	public void StopAnyInteractions()
	{
		PlayerAnimatorSetInteraction(isInteracting: false);
		PlayerAnimatorSetDoorBreak(isInteracting: false);
		WorldInteractiveObject worldInteractiveObject = InteractionInfo.WorldInteractiveObject;
		if (worldInteractiveObject != null)
		{
			if (worldInteractiveObject.DoorState == EDoorState.Open || worldInteractiveObject.DoorState == EDoorState.Interacting)
			{
				_player.TryInteractionCallback(worldInteractiveObject as LootableContainer);
			}
			if (InteractionInfo.Callback != null)
			{
				InteractionInfo.Callback();
				InteractionInfo.Callback = null;
			}
			InteractionInfo = default(GStruct254);
		}
		_player.UpdateInteractionCast();
	}

	public float GetHandProgress()
	{
		if (!(_player == null) && !(_player.HandsController == null))
		{
			return _player.HandsController.GetAnimatorFloatParam(WeaponAnimationSpeedControllerClass.FLOAT_LEFTHANDPROGRESS);
		}
		return 0f;
	}

	public void SetInteractInHands(EInteraction interaction)
	{
		if (IsInMountedState || IsSprintEnabled || IsStationaryWeaponInHands || (!_player.HandsController.FirearmsAnimator.IsIdling() && interaction != EInteraction.DropBackpack) || ((CurrentState.Name == EPlayerState.Jump || CurrentState.Name == EPlayerState.JumpLanding) && GClass3937.IsPlayerGesture(interaction)))
		{
			return;
		}
		_player.RemoveLeftHandItem(2.5f);
		if (_player.UsedSimplifiedSkeleton)
		{
			PlayerAnimator.EDoorInteractionIndex doorInteraction = PlayerAnimator.EDoorInteractionIndex.Push;
			switch (interaction)
			{
			case EInteraction.PullHingeRight:
				doorInteraction = PlayerAnimator.EDoorInteractionIndex.PullRight;
				break;
			case EInteraction.PushHingeLeft:
			case EInteraction.PushHingeRight:
			case EInteraction.DoorPushForward:
				doorInteraction = PlayerAnimator.EDoorInteractionIndex.Push;
				break;
			case EInteraction.PullHingeLeft:
			case EInteraction.DoorPullBackward:
				doorInteraction = PlayerAnimator.EDoorInteractionIndex.PullLeft;
				break;
			}
			PlayerAnimator.SetDoorInteraction(doorInteraction);
		}
		else
		{
			PlayerAnimator.AnimatedInteractions.SetInteraction(interaction);
		}
		_player.OnAnimatedInteraction(interaction);
	}

	public void SetIKInteraction(GripPose grip)
	{
		_player.HandPosers[0].MapIkPalm(grip);
		_player.LeftHandInteractionTarget = grip;
		if (grip == null)
		{
			IK.Target = 0f;
		}
	}

	public void EndIkInteraction()
	{
	}

	public void RestoreDefaultAlignment(float normalTime)
	{
		if (normalTime < 1f)
		{
			SurfaceDirection = Vector3.Lerp(SurfaceDirection, PlayerTransform.forward, normalTime);
			SurfaceNormalInterpolated = Vector3.Lerp(SurfaceNormalInterpolated, GClass842.Vector3Up, normalTime);
			_player.PlayerBones.AnimatedTransform.LookAt(PlayerTransform.position + SurfaceDirection, SurfaceNormalInterpolated);
			SurfacePitch_1 = _player.PlayerBones.AnimatedTransform.localRotation.eulerAngles.x;
			SurfacePitch_1 = ((SurfacePitch_1 > 180f) ? (SurfacePitch_1 - 360f) : SurfacePitch_1);
		}
		else
		{
			SurfacePitch_1 = 0f;
			_player.PlayerBones.AnimatedTransform.localRotation = Quaternion.identity;
			SurfaceDirection = GClass842.Vector3Zero;
			SurfaceNormalInterpolated = GClass842.Vector3Up;
		}
	}

	public void AlignToSurface(float deltaTime, Vector3? axis = null)
	{
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(PlayerColliderPointOnCenterAxis(1.5f) + TransformForwardVector * 0.5f, GClass842.Vector3Down, out hitInfo, 2f, GroundMask_1);
		_ = hitInfo.point;
		Vector3 normal = hitInfo.normal;
		if (flag && GroundHit_1.hasHit)
		{
			TwoPointAlignment(hitInfo.point, GroundHit_1.point, deltaTime);
		}
		else if (flag)
		{
			OnePointAlignment(normal, deltaTime);
		}
		else if (GroundHit_1.hasHit)
		{
			OnePointAlignment(GroundHit_1.normal, deltaTime);
		}
		else
		{
			OnePointAlignment(SurfaceNormal, deltaTime);
		}
		Vector3 vector = ((SurfaceNormal != Vector3.zero) ? SurfaceNormal : (GroundHit_1.hasHit ? GroundHit_1.normal : ((!flag) ? Vector3.up : normal)));
		if (IsInPronePose_1 && Vector3.Angle(vector, Vector3.up) >= 30f)
		{
			Vector3 axis2 = Vector3.Cross(Vector3.up, vector);
			vector = Quaternion.AngleAxis(30f, axis2) * Vector3.up;
		}
		SurfaceNormalInterpolated = Vector3.Lerp(SurfaceNormalInterpolated, vector, deltaTime * EFTHardSettings.Instance.PRONE_ALIGN_SPEED);
		_player.PlayerBones.AnimatedTransform.LookAt(PlayerTransform_1.position + SurfaceDirection, SurfaceNormalInterpolated);
		Quaternion localRotation = _player.PlayerBones.AnimatedTransform.localRotation;
		if (IsInPronePose_1)
		{
			float z = Mathf.Clamp(localRotation.eulerAngles.z, -8f, 8f);
			localRotation.eulerAngles = new Vector3(localRotation.eulerAngles.x, localRotation.eulerAngles.y, z);
			_player.PlayerBones.AnimatedTransform.localRotation = localRotation;
		}
		PlayerSurfaceUpAlignNormal = _player.PlayerBones.AnimatedTransform.up.normalized;
		SurfacePitch_1 = localRotation.eulerAngles.x;
		SurfacePitch_1 = ((SurfacePitch_1 > 180f) ? (SurfacePitch_1 - 360f) : SurfacePitch_1);
	}

	public void TwoPointAlignment(Vector3 anchorPoint1, Vector3 anchorPoint2, float deltaTime)
	{
		Vector3 forward = _player.PlayerBones.AnimatedTransform.forward;
		SurfaceDirection = Vector3.Lerp(forward, (anchorPoint1 - anchorPoint2).normalized, deltaTime * EFTHardSettings.Instance.PRONE_ALIGN_SPEED);
	}

	public void OnePointAlignment(Vector3 normal, float deltaTime)
	{
		Vector3 forward = _player.PlayerBones.AnimatedTransform.forward;
		Vector3 b = Vector3.Cross(normal, Vector3.Cross(PlayerTransform.forward, normal));
		SurfaceDirection = Vector3.Lerp(forward, b, deltaTime * EFTHardSettings.Instance.PRONE_ALIGN_SPEED);
	}

	public bool method_19(Vector3 test)
	{
		if (Physics.Raycast(test, GClass842.Vector3Down, out var hitInfo, 0.6f, GroundMaskPlusWater))
		{
			if (hitInfo.collider.gameObject.layer == (int)LayerMaskClass.WaterLayer)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual bool CanStandAt(float h)
	{
		if (IsInPronePose && PhysicalConditionIs(EPhysicalCondition.UsingMeds))
		{
			return false;
		}
		if (h < PoseLevel && !IsInPronePose)
		{
			return true;
		}
		h = Mathf.Lerp(0.875f, 1.15f, h);
		float y = h * 1.25f / 2f;
		int num = Physics.OverlapBoxNonAlloc(PlayerTransform.position + Vector3.up * (h * 0.75f), new Vector3(0.21f, y, 0.21f), CollidersAllocated, PlayerTransform.rotation, GroundMaskPlusPlayer, QueryTriggerInteraction.Ignore);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			Collider col = CollidersAllocated[i];
			Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
			if (playerByCollider == null || playerByCollider != _player)
			{
				flag = true;
				break;
			}
		}
		return !flag;
	}

	public void SpawnKeyInHands(Item item, string parentBone)
	{
		_player.SpawnInHands(item, parentBone);
	}

	public void RemoveKeyFromHand()
	{
		_player.ClearHands();
	}

	public bool IsHandsProcessing()
	{
		return _player.HandsController.IsHandsProcessing();
	}

	public void LockedDoorOpened(bool breached)
	{
		_player.StatisticsManager.AddDoorExperience(breached);
	}

	public BaseMovementState GetNewStateFabric(EPlayerState name, int stateNameHash, bool createUnique)
	{
		States_1.TryGetValue(name, out var value);
		if (value == null || createUnique)
		{
			value = GetNewState(name);
			States_1[name] = value;
		}
		return value;
	}

	public BaseMovementState GetNewAIStateFabric(EPlayerState name, int stateNameHash, bool createUnique)
	{
		States_1.TryGetValue(name, out var value);
		if (value == null)
		{
			value = GetNewState(name, isAI: true);
			States_1[name] = value;
		}
		return value;
	}

	public void method_20(IPlayerStateContainerBehaviour[] states)
	{
		StateHashes = states.Select((IPlayerStateContainerBehaviour x) => x.FullNameHash).ToList();
		StateHashes.Sort();
		StatesHashToName = states.ToDictionary((IPlayerStateContainerBehaviour x) => x.FullNameHash, (IPlayerStateContainerBehaviour x) => x.PlayerStateName);
		StatesHashToIndex = new Dictionary<int, int>();
		StatesIndexToName.Clear();
		for (int num = 0; num < StateHashes.Count; num++)
		{
			StatesHashToIndex[StateHashes[num]] = num;
			StatesIndexToName[num] = StatesHashToName[StateHashes[num]];
		}
	}

	public int GetStateHashTableIndex(int hash)
	{
		try
		{
			return StatesHashToIndex[hash];
		}
		catch (Exception)
		{
			Debug.LogError("hash=" + hash);
			throw;
		}
	}

	public int GetStateHash(int index)
	{
		return StateHashes[index];
	}

	public EPlayerState GetStateNameByHash(int hash)
	{
		return StatesHashToName[hash];
	}

	public static EPlayerState GetStateNameByIndex(int index)
	{
		if (StatesIndexToName.TryGetValue(index, out var value))
		{
			return value;
		}
		return EPlayerState.None;
	}

	public virtual BaseMovementState GetNewState(EPlayerState name, bool isAI = false)
	{
		BaseMovementState result = null;
		switch (name)
		{
		case EPlayerState.Idle:
			result = new IdleStateClass(this);
			break;
		case EPlayerState.ProneIdle:
			result = ((!isAI) ? new ProneIdleStateClass(this) : new ProneAIIdleStateClass(this));
			break;
		case EPlayerState.ProneMove:
			result = ((!isAI) ? new ProneMoveStateClass(this) : new ProneAIMoveStateClass(this));
			break;
		case EPlayerState.Run:
			result = new RunStateClass(this);
			break;
		case EPlayerState.Sprint:
			result = new SprintStateClass(this);
			break;
		case EPlayerState.Jump:
			result = new JumpStateClass(this);
			break;
		case EPlayerState.Transition:
			result = new TransitionStateClass(this);
			break;
		case EPlayerState.BreachDoor:
			result = new BreachDoorStateClass(this);
			break;
		case EPlayerState.Loot:
			result = new LootingStateClass(this);
			break;
		case EPlayerState.Pickup:
			result = new PickupStateClass(this);
			break;
		default:
			throw new ArgumentOutOfRangeException(name.ToString() + " has no association with state class");
		case EPlayerState.Sidestep:
			result = new SideStepStateClass(this);
			break;
		case EPlayerState.Approach:
			result = new ApproachStateClass(this);
			break;
		case EPlayerState.Prone2Stand:
			result = new Prone2StandStateClass(this);
			break;
		case EPlayerState.Transit2Prone:
			result = new Transit2ProneStateClass(this);
			break;
		case EPlayerState.Plant:
			result = new PlantStateClass(this);
			break;
		case EPlayerState.Stationary:
			result = new StationaryStateClass(this);
			break;
		case EPlayerState.Roll:
			result = new RollStateClass(this);
			break;
		case EPlayerState.JumpLanding:
			result = new JumpLandingStateClass(this);
			break;
		case EPlayerState.ClimbOver:
			result = new ClimbOverStateClass(this);
			break;
		case EPlayerState.ClimbUp:
			result = new ClimbUpStateClass(this);
			break;
		case EPlayerState.VaultingFallDown:
			result = new GClass2136(this);
			break;
		case EPlayerState.VaultingLanding:
			result = new GClass2126(this);
			break;
		case EPlayerState.BlindFire:
			result = new BlindFireStateClass(this);
			break;
		case EPlayerState.IdleWeaponMounting:
			result = new IdleWeaponMountingStateClass(this, _player);
			break;
		case EPlayerState.IdleZombieState:
			result = new IdleZombieStateClass(this);
			break;
		case EPlayerState.MoveZombieState:
			result = new MoveZombieStateClass(this);
			break;
		case EPlayerState.TurnZombieState:
			result = new GClass2141(this);
			break;
		case EPlayerState.StartMoveZombieState:
			result = new GClass2144(this);
			break;
		case EPlayerState.EndMoveZombieState:
			result = new GClass2143(this);
			break;
		case EPlayerState.DoorInteraction:
		case EPlayerState.DoorInteractionZombieState:
			result = new DoorInteractionStateClass(this);
			break;
		case EPlayerState.FallDown:
			break;
		}
		return result;
	}

	public void FovCompensationOn(bool on)
	{
		if (on)
		{
			_player.SetCompensationScale();
		}
		else
		{
			_player.RestoreRibcageScale();
		}
	}

	public virtual void ProjectMotionToSurface(ref Vector3 motion)
	{
		float avarage = NormalDotHistory.Avarage;
		if (!(avarage < 0.995f) || !(avarage > 0.5f))
		{
			return;
		}
		Vector3 normalized = Vector3.Cross(new Vector3(motion.z, 0f, 0f - motion.x), SurfaceNormal).normalized;
		if (normalized.y < 0f)
		{
			float num = Vector3.Dot(normalized, motion.normalized);
			if (0.5f <= num && num <= 1f)
			{
				motion = normalized * (motion.magnitude / num);
			}
		}
	}

	public virtual void ApplyGravity(ref Vector3 motion, float deltaTime, bool stickToGround)
	{
		if (!_player.CharacterController.ShouldStickToGround)
		{
			return;
		}
		if (stickToGround)
		{
			if (!(FreefallTime < 0.25f) && !IsGrounded)
			{
				motion *= 1f / (1f + 2f * FreefallTime);
				motion += MovementState.G * (FreefallTime * deltaTime);
			}
			else if (Slope.sqrMagnitude > 0f)
			{
				motion += Slope * (MovementState.G_y * deltaTime);
			}
			else
			{
				motion += MovementState.G * (FreefallTime * deltaTime);
			}
		}
		if (IsGrounded)
		{
			FreefallTime = 0f;
			HasLastHeight = false;
		}
		else
		{
			FreefallTime += deltaTime;
			LastPlayerHeight = TransformPosition.y;
			HasLastHeight = true;
		}
	}

	public virtual void LimitMotionXZ(ref Vector3 motion, float deltaTime, float threshold = 0.0001f)
	{
		InputMotionBeforeLimit = motion / deltaTime;
		if (!(_player.POM == null))
		{
			_player.POM.Calculate(motion);
			Vector3 pOMOffset = POMOffset;
			if (!(pOMOffset.sqrMagnitude < threshold))
			{
				pOMOffset = Vector3.ClampMagnitude(pOMOffset, Mathf.Max(0.05f, motion.magnitude + 0.01f));
				motion += pOMOffset;
			}
		}
	}

	public Vector3 RotationOverlapPrediction(Vector3 motion, Quaternion rotation, Transform rotationPivot)
	{
		if (_player.POM == null)
		{
			return Vector3.zero;
		}
		Vector3 position = _player.POM.Collider.transform.position;
		Vector3 eulerAngles = _player.PlayerBones.AnimatedTransform.eulerAngles;
		Quaternion quaternion = Quaternion.Euler(eulerAngles.x, Yaw, eulerAngles.z);
		Matrix4x4 matrix4x = Matrix4x4.TRS(rotationPivot.position, quaternion, Vector3.one);
		Vector3 point = rotation * matrix4x.inverse.MultiplyPoint3x4(position);
		Vector3 vector = matrix4x.MultiplyPoint3x4(point);
		Vector3 motion2 = motion + vector - position;
		_player.POM.Calculate(motion2, rotation * quaternion);
		return _player.POM.GetOffsetXZ();
	}

	public bool IsAbleToRotateProne(Vector3 motion)
	{
		Quaternion animatorDeltaRotation = AnimatorDeltaRotation;
		Quaternion predictionRotation = animatorDeltaRotation * animatorDeltaRotation * animatorDeltaRotation;
		ECantRotate cause;
		return IsAbleToRotateProne(motion, Yaw - PreviousYaw, predictionRotation, _player.PlayerBones.Spine1, out cause);
	}

	public virtual bool IsAbleToRotateProne(Vector3 motion, float deltaYaw, Quaternion predictionRotation, Transform pivot, out ECantRotate cause)
	{
		bool flag = false;
		if (Math.Abs(deltaYaw) > float.Epsilon)
		{
			if (!(flag = RotationOverlapPrediction(motion, predictionRotation, pivot).sqrMagnitude < 1E-06f))
			{
				cause = ECantRotate.MinAng;
				return false;
			}
			Vector3 vector = TransformRotation * predictionRotation * Vector3.forward;
			Vector3 normalized = Vector3.Cross(new Vector3(vector.z, 0f, 0f - vector.x), SurfaceNormal).normalized;
			if (flag &= HasGround(0.55f, normalized, 0.15f))
			{
				flag &= HasGround(0.75f, -normalized, 0.15f);
			}
			cause = ECantRotate.NotGround;
		}
		cause = ECantRotate.Epsilon;
		return flag;
	}

	public virtual void LimitProneMotion(ref Vector3 motion)
	{
		if (!(motion.magnitude > float.Epsilon))
		{
			return;
		}
		Vector3 vector = InverseTransformVector(motion);
		float x = vector.x;
		float z = vector.z;
		float num = 0f;
		bool flag = false;
		Vector3 playerRealForward = PlayerRealForward;
		Vector3 vector2 = new Vector3(playerRealForward.z, 0f, 0f - playerRealForward.x);
		if (Mathf.Abs(x) > 0.0001f)
		{
			num = Mathf.Sign(x);
			if (!HasGround(0.1f, vector2 * num, 0.15f))
			{
				flag = true;
				vector.x = 0f - x;
			}
		}
		if (Mathf.Abs(z) > 0.0001f)
		{
			num = Mathf.Sign(z);
			Vector3 normalized = Vector3.Cross(vector2, SurfaceNormal).normalized;
			if (!HasGround(0.6f, normalized * num, 0.15f))
			{
				flag = true;
				vector.z = 0f - z;
			}
		}
		if (flag)
		{
			motion = TransformVector(vector);
		}
	}

	public void SetPatrol(bool b)
	{
		IsInPatrol = b;
		_player.HandsAnimator.SetPatrol(IsInPatrol || BlockFirearms_1);
	}

	public void RestorePreviousPatrol()
	{
		SetPatrol(IsInPatrol);
	}

	public void ResetSpeedAfterSprint()
	{
		SetCharacterMovementSpeed(MaxSpeed);
		RaiseChangeSpeedEvent();
	}

	public void RaiseChangeSpeedEvent()
	{
		_player.RaiseChangeSpeedEvent();
	}

	public void IgnoreInteractionCollision(Collider c2, bool ignore)
	{
		if (ignore && c2 != null)
		{
			IgnoredColliders.Add(c2);
			Physics.IgnoreCollision(_player.POM.Collider, c2);
			EFTPhysicsClass.IgnoreCollision(CharacterController.GetCollider(), c2);
			return;
		}
		for (int i = 0; i < CoroutineColliders.Length; i++)
		{
			Physics.IgnoreCollision(_player.POM.Collider, CoroutineColliders[i], ignore: false);
			EFTPhysicsClass.IgnoreCollision(CharacterController.GetCollider(), CoroutineColliders[i], ignore: false);
		}
		CoroutineColliders = new Collider[IgnoredColliders.Count];
		IgnoredColliders.CopyTo(CoroutineColliders);
		IgnoredColliders.Clear();
		_player.StartCoroutine(UnignoreCollision());
	}

	public IEnumerator UnignoreCollision()
	{
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < CoroutineColliders.Length; i++)
		{
			Physics.IgnoreCollision(_player.POM.Collider, CoroutineColliders[i], ignore: false);
			EFTPhysicsClass.IgnoreCollision(CharacterController.GetCollider(), CoroutineColliders[i], ignore: false);
		}
	}

	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (Time.realtimeSinceStartup > TimeSinceLastCCCollision)
		{
			CCHits.Clear();
			CCAllHits.Clear();
		}
		CCAllHits.Add(hit);
		TimeSinceLastCCCollision = Time.realtimeSinceStartup;
		ICharacterController characterController = CharacterController;
		float num = characterController.height / 2f - characterController.radius;
		if (characterController.isGrounded && PlayerColliderCenter.y > hit.point.y + num)
		{
			Debug.DrawRay(hit.point, hit.normal, Color.blue, Time.deltaTime);
			CCHits.Add(hit);
		}
	}

	public string ControllerColliderHits()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < CCAllHits.Count; i++)
		{
			ControllerColliderHit controllerColliderHit = CCAllHits[i];
			if (i != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.AppendFormat("{0}:{1}", controllerColliderHit.gameObject, controllerColliderHit.gameObject.layer);
		}
		return stringBuilder.ToString();
	}

	public void SetBlindFire(int b)
	{
		if (!(_player.HandsController == null) && !_player.UsedSimplifiedSkeleton)
		{
			if (b != 0)
			{
				_player.RemoveLeftHandItem(3f);
			}
			if (!(_player.HandsController is Player.FirearmController firearmController))
			{
				PlayerAnimatorSetBlindFire(0f);
				_player.HandsController.BlindFire(0);
				BlindFire = 0;
			}
			else if (firearmController.CanSetBlindFire || b == 0)
			{
				BlindFire = b;
				firearmController.BlindFire(b);
				PlayerAnimatorSetBlindFire(b);
			}
		}
	}

	public void ChangeSpeedLimit(float speedDelta, Player.ESpeedLimit cause)
	{
		if (SpeedLimits.ContainsKey(cause))
		{
			SpeedLimits[cause] = Mathf.Min(SpeedLimits[cause], speedDelta);
		}
		else
		{
			SpeedLimits.Add(cause, speedDelta);
		}
		method_5();
	}

	public void ChangeSpeedLimit(float speedDelta, Player.ESpeedLimit cause, float duration)
	{
		if (SpeedLimitTimings.ContainsKey(cause))
		{
			SpeedLimitTimings[cause] = Mathf.Max(SpeedLimitTimings[cause], Time.realtimeSinceStartup + duration);
		}
		else
		{
			SpeedLimitTimings.Add(cause, Time.realtimeSinceStartup + duration);
		}
		ChangeSpeedLimit(speedDelta, cause);
	}

	public void RotateFail(ECantRotate cantRotate)
	{
		this.OnCantRotate?.Invoke(cantRotate);
	}

	public virtual void PlayerAnimatorEnableInert(bool enabled)
	{
		PlayerAnimator_1.EnableInert(enabled);
	}

	public virtual void PlayerAnimatorEnableSprint(bool enabled, bool dontChangeLeftStanceValue = false)
	{
		PlayerAnimator_1.EnableSprint(enabled);
	}

	public virtual void PlayerAnimatorSetSprintToIdleSpeed(float value)
	{
		PlayerAnimator_1.Animator.speed = value;
	}

	public virtual void PlayerAnimatorEnableKick(bool enabled)
	{
		PlayerAnimator_1.EnableKick(enabled);
	}

	public virtual void PlayerAnimatorEnableLanding(bool enabled)
	{
		PlayerAnimator_1.EnableLanding(enabled);
	}

	public virtual void PlayerAnimatorEnableJump(bool enabled)
	{
		PlayerAnimator_1.EnableJump(enabled);
	}

	public virtual void PlayerAnimatorEnableLoot(bool enabled)
	{
		PlayerAnimator_1.EnableLoot(enabled);
	}

	public virtual void PlayerAnimatorSetAimAngle(float movementContextPitch)
	{
		PlayerAnimator_1.SetAimAngle(movementContextPitch);
	}

	public virtual void PlayerAnimatorSetDeltaRotation(float movementContextPitch)
	{
		PlayerAnimator_1.SetDeltaRotation(movementContextPitch);
	}

	public virtual void PlayerAnimatorEnableFallingDown(bool enabled)
	{
		PlayerAnimator_1.EnableFallingDown(enabled);
	}

	public virtual void PlayerAnimatorEnableProneReload(bool enabled)
	{
		PlayerAnimator_1.EnableProneReload(enabled);
	}

	public virtual void PlayerAnimatorSetDiscreteDirection(EMovementDirection discreteDirection)
	{
		PlayerAnimator_1.SetDiscreteDirection(discreteDirection);
	}

	public virtual void PlayerAnimatorEnableProne(bool enabled)
	{
		PlayerAnimator_1.EnableProne(enabled);
	}

	public virtual void PlayerAnimatorSetCharacterMovementSpeed(float smoothedCharacterMovementSpeed)
	{
		PlayerAnimator_1.SetCharacterMovementSpeed(smoothedCharacterMovementSpeed);
	}

	public virtual void PlayerAnimatorSetPoseLevel(float smoothedPoseLevel)
	{
		if (!_player.UsedSimplifiedSkeleton)
		{
			PlayerAnimator_1.SetPoseLevel(smoothedPoseLevel);
		}
	}

	public virtual void PlayerAnimatorSetTilt(float smoothedTilt)
	{
		PlayerAnimator_1.SetTilt(smoothedTilt);
	}

	public virtual void PlayerAnimatorSetBlindFire(float blindFire)
	{
		PlayerAnimator_1.SetBlindFire(blindFire);
	}

	public virtual void PlayerAnimatorSetMovementDirection(Vector2 movementDirection)
	{
		PlayerAnimator_1.SetMovementDirection(movementDirection);
	}

	public virtual void PlayerAnimatorSetStep(int step)
	{
		PlayerAnimator_1.SetStep(step);
	}

	public virtual void PlayerAnimatorSetFallingDownFloat(float val)
	{
		PlayerAnimator_1.SetFallingDownFloat(val);
	}

	public virtual void PlayerAnimatorSetInteraction(bool isInteracting, bool approached = false)
	{
		PlayerAnimator_1.SetInteraction(isInteracting, approached);
	}

	public virtual void PlayerAnimatorSetKickSucceed(bool nextBreachResult)
	{
		PlayerAnimator_1.SetKickSucceed(nextBreachResult);
	}

	public virtual void PlayerAnimatorSetDoorBreak(bool isInteracting, bool approached = false)
	{
		PlayerAnimator_1.SetDoorBreak(isInteracting, approached);
	}

	public virtual void PlayerAnimatorSetDeflected(bool deflected)
	{
		PlayerAnimator_1.SetDeflected(deflected);
	}

	public virtual void PlayerAnimatorSetSwingSpeed(float speed)
	{
		PlayerAnimator_1.SetSwingSpeed(speed);
	}

	public void PlayerAnimatorSetWeaponId(PlayerAnimator.EWeaponAnimationType index)
	{
		if (_player.HandsController == null)
		{
			PlayerAnimator_1.SetWeaponId(index);
		}
		else
		{
			_player.StartCoroutine(method_21(index));
		}
	}

	public IEnumerator method_21(PlayerAnimator.EWeaponAnimationType index)
	{
		float num = PlayerAnimator_1.GetWeaponTypeFloat();
		float num2 = (float)index;
		PlayerAnimator_1.SetWeaponType(index);
		while (Mathf.Abs(num - num2) > 0.01f)
		{
			num = Mathf.Lerp(num, num2, Time.deltaTime * 7f);
			PlayerAnimator_1.SetWeaponTypeFloat(num);
			yield return null;
		}
		PlayerAnimator_1.SetWeaponTypeFloat(num2);
	}

	public void PlayerAnimatorSetApproached(bool b)
	{
		PlayerAnimator_1.SetApproached(b);
	}

	public void PlayerAnimatorSetStationary(bool b)
	{
		PlayerAnimator_1.SetStationary(b);
	}

	public void PlayerAnimatorSetStationaryAnimation(int index)
	{
		PlayerAnimator_1.SetStationaryType(index);
	}

	public void PlayerAnimatorSetObstacleHeight(float height)
	{
		PlayerAnimator_1.SetVaultingHeight(height);
	}

	public void PlayerAnimatorSetObstacleLength(float length)
	{
		PlayerAnimator_1.SetVaultingLength(length);
	}

	public void PlayerAnimatorSetDoVault(bool doVault)
	{
		PlayerAnimator_1.SetDoVault(doVault);
	}

	public void PlayerAnimatorSetDoClimb(bool doClimb)
	{
		PlayerAnimator_1.SetDoClimb(doClimb);
	}

	public void PlayerAnimatorSetAbsoluteForwardVelocity(float velocity)
	{
		PlayerAnimator_1.SetAbsoluteForwardVelocity(velocity);
	}

	public void PlayerAnimatorSetBehindObstacleRatio(float height)
	{
		PlayerAnimator_1.SetBehindObstacleRatio(height);
	}

	public float PlayerAnimatorGetObstacleLength()
	{
		return PlayerAnimator_1.GetVaultingLength();
	}

	public float PlayerAnimatorGetObstacleHeight()
	{
		return PlayerAnimator_1.GetVaultingHeight();
	}

	public void PlayerAnimatorSetIsVaulting(bool isVaulting)
	{
		PlayerAnimator_1.SetIsVaulting(isVaulting);
	}

	public bool PlayerAnimatorGetIsVaulting()
	{
		return PlayerAnimator_1.GetIsVaulting();
	}

	public bool PlayerAnimatorIsJumpSetted()
	{
		return PlayerAnimator_1.IsJumpSetted();
	}

	public void UpdateVaultingSpeedLimit()
	{
		SpeedLimiter?.Update(CurrentState.Name, SkillManager.Strength.SummaryLevel, 1f);
	}

	public bool IsAnimatorInTransitionState(int layer)
	{
		return PlayerAnimator_1.Animator.IsInTransition(layer);
	}

	public void PlayerAnimatorPointOfView(EPointOfView pointOfView)
	{
		bool flag = pointOfView == EPointOfView.ThirdPerson;
		PlayerAnimator_1.SetIsThirdPerson(flag);
		PlayerAnimator_1.SetLayerWeight(1, flag ? 1 : 0);
	}

	public void ToggleBlockInputPlayerRotation(bool block)
	{
		if (block)
		{
			RotationAction = null;
		}
		else
		{
			RotationAction = DefaultRotationFunction;
		}
	}

	public void ResetInventory()
	{
		_player.SetInventoryOpened(_player.IsInventoryOpened);
	}

	public void CancelInventoryAnimation()
	{
		if (_player.HandsController != null)
		{
			_player.HandsController.SetInventoryOpened(opened: false);
		}
	}

	public Vector3 GetProjectionOnRealForwardSurface(Vector3 point)
	{
		Vector3 vector = -PlayerRealForward;
		Vector3 rhs = TransformPosition - point;
		float num = Vector3.Dot(vector, rhs);
		return point + vector * num;
	}

	public virtual void SetStationaryWeapon(Action<Player.AbstractHandsController, Player.AbstractHandsController> callback)
	{
		Class1397 CS_0024_003C_003E8__locals13 = new Class1397();
		CS_0024_003C_003E8__locals13.movementContext_0 = this;
		CS_0024_003C_003E8__locals13.callback = callback;
		if (_player.HandsController.Item == StationaryWeapon.Item)
		{
			CS_0024_003C_003E8__locals13.callback(_player.HandsController, _player.HandsController);
			return;
		}
		OnHandsControllerChanged += delegate(Player.AbstractHandsController controller, Player.AbstractHandsController handsController)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (CS_0024_003C_003E8__locals13.movementContext_0.StationaryWeapon == null)
			{
				CS_0024_003C_003E8__locals13.movementContext_0.OnHandsControllerChanged -= CS_0024_003C_003E8__locals13.method_0;
			}
			else if (handsController != null && handsController.Item == CS_0024_003C_003E8__locals13.movementContext_0.StationaryWeapon.Item)
			{
				CS_0024_003C_003E8__locals13.movementContext_0.OnHandsControllerChanged -= CS_0024_003C_003E8__locals13.method_0;
				if (CS_0024_003C_003E8__locals13.movementContext_0._player.AIData != null)
				{
					CS_0024_003C_003E8__locals13.movementContext_0._player.AIData.SetStationaryWeapon((Weapon)CS_0024_003C_003E8__locals13.movementContext_0.StationaryWeapon.Item);
				}
				CS_0024_003C_003E8__locals13.callback(null, handsController);
			}
		};
		_player.SetStationaryWeapon((Weapon)StationaryWeapon.Item);
	}

	public virtual void DropStationary(StationaryPacketStruct.EStationaryCommand command)
	{
		if (command == StationaryPacketStruct.EStationaryCommand.Occupy)
		{
			Debug.LogError("Attempt to drop Stationary weapon with command OCCUPY");
		}
		_player.OperateStationaryWeapon(StationaryWeapon, command);
	}

	public void UpdateStateValues(ref float stateSensitivity, ref float stateClamp)
	{
		float sprintSensitivity = _player.Physical.SprintSensitivity;
		stateSensitivity *= sprintSensitivity;
		stateClamp *= sprintSensitivity;
	}

	public void AssignAnimatorPose(AnimatorPose animatorPose)
	{
		if (_player.ProceduralWeaponAnimation != null && _player.FirstPersonPointOfView)
		{
			_player.ProceduralWeaponAnimation.AssignAnimatorPose(animatorPose);
		}
	}

	public void SetPOMCollider(PlayerOverlapManager.EExtrusionCollider collider)
	{
		_player.POM.SetCollider(collider);
	}

	public void CreateSpeedLimiter(GClass2175.Config config)
	{
		SpeedLimiter = SpeedLimiterBuilder(config);
		SpeedLimiter.OnSpeedChanged += method_22;
		UpdateCharacterControllerSpeedLimit();
	}

	public virtual GClass2175 SpeedLimiterBuilder(GClass2175.Config config)
	{
		return new GClass2175(config, 60);
	}

	public void UpdateCharacterControllerSpeedLimit()
	{
		if (CurrentState is GClass2137)
		{
			UpdateVaultingSpeedLimit();
			return;
		}
		SpeedLimiter?.Update(CurrentState.Name, SkillManager.Strength.SummaryLevel, (CurrentState.Name == EPlayerState.Sprint) ? 1f : _player.Physical.WalkSpeedLimit);
		SetCharacterMovementSpeed(RelativeSpeed * MaxSpeed);
	}

	public void method_22(float maxSpeed)
	{
		float speed = SpeedLimiter.Speed;
		if (!Mathf.Approximately(_player.CharacterController.SpeedLimit, speed))
		{
			_player.CharacterController.SpeedLimit = speed;
			this.OnCharacterControllerSpeedLimitChanged?.Invoke();
		}
	}

	public void method_23()
	{
		UpdateCharacterControllerSpeedLimit();
	}

	public void ResetCanUsePropState()
	{
		CanUseProp.Value = false;
		CanUseProp.Value = CurrentState.CanInteract;
	}

	public void SetUpDiscreteDirection(EMovementDirection direction)
	{
		DiscreteDirection = direction;
		PlayerAnimatorSetDiscreteDirection(direction);
	}

	public void UpdateSprintInertia()
	{
		PlayerAnimator_1.SetSprintInertia(SprintBrakeInertia);
	}

	public void SubscribeToSprintResetAction()
	{
		UnsubFromSprintExit?.Invoke();
		OnStateChanged += SprintExitDelegate;
		UnsubFromSprintExit = (Action)Delegate.Combine(UnsubFromSprintExit, (Action)delegate
		{
			OnStateChanged -= SprintExitDelegate;
			UnsubFromSprintExit = null;
		});
	}

	public void SprintExitDelegate(EPlayerState previousState, EPlayerState nextState)
	{
		if (nextState != EPlayerState.Sprint)
		{
			UnsubFromSprintExit?.Invoke();
			if (nextState != EPlayerState.Jump)
			{
				SprintSpeed = EFTHardSettings.Instance.StartingSprintSpeed;
			}
		}
	}

	public void method_24(bool enable)
	{
		if (!_player.UsedSimplifiedSkeleton)
		{
			if (!enable)
			{
				PlayerAnimator?.SetLeftStanceForceInOneFrame(enabled: false);
			}
			PlayerAnimator?.SetLeftStance(enable);
			_player.ProceduralWeaponAnimation.LeftStance = enable;
		}
	}

	public void method_25()
	{
		PlayerAnimator?.SetLeftStanceForceInOneFrame(enabled: true);
		PlayerAnimator?.SetLeftStance(enabled: true);
		_player.ProceduralWeaponAnimation.LeftStance = true;
	}

	public void SetStationaryStrategy()
	{
		_player.ProceduralWeaponAnimation.SetStrategy(new GClass911());
	}

	public void method_26()
	{
		_player.ProceduralWeaponAnimation.SetStrategy(MountingStrategy);
	}

	public virtual void SetTurnDeltaAngle(float deltaAngle)
	{
		PlayerAnimator.SetTurnDeltaAngle(deltaAngle);
	}

	[CompilerGenerated]
	public void method_27()
	{
		_player.Skills.PushUp.Complete(new SkillManager.GStruct279
		{
			Overweight = Overweight
		});
	}

	[CompilerGenerated]
	public void method_28(EPhysicalCondition flag, ref Struct333 struct333_0)
	{
		SetPhysicalCondition(flag, (struct333_0.conditions & flag) == flag);
	}

	[CompilerGenerated]
	public float method_29()
	{
		return ActualLinearVelocity;
	}

	[CompilerGenerated]
	public Vector2 method_30()
	{
		return MovementDirection;
	}

	[CompilerGenerated]
	public float method_31()
	{
		return HandsToBodyAngle_1;
	}

	[CompilerGenerated]
	public void method_32()
	{
		_player.Skills.PushUp.Complete(new SkillManager.GStruct279
		{
			Overweight = Overweight
		});
	}

	[CompilerGenerated]
	public void method_33()
	{
		OnStateChanged -= SprintExitDelegate;
		UnsubFromSprintExit = null;
	}
}
