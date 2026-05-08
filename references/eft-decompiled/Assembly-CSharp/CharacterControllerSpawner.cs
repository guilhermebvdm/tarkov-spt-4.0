using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class CharacterControllerSpawner : MonoBehaviour
{
	[Serializable]
	public class Mode
	{
		public ControllerType Type;

		public bool WithRigidbody;

		public bool WithMovementValidation;

		public bool WithTriggerCollider;

		public bool WithKinematicOption;
	}

	public enum ControllerType
	{
		Unity,
		Impostor,
		Simple,
		SteeringImpostor,
		BotAISteeringImpostorWithDoors
	}

	[Serializable]
	[CompilerGenerated]
	public class Class440
	{
		public static readonly Class440 class440_0 = new Class440();

		public static Func<Collider, int> func_0;

		public int method_0(Collider x)
		{
			return Singleton<GameWorld>.Instance.CanPlayerSeepThrough(x);
		}
	}

	[SerializeField]
	private float _slopeLimit;

	[SerializeField]
	private float _stepOffset;

	[SerializeField]
	private float _skinWidth;

	[SerializeField]
	private float _minMoveDistance;

	[SerializeField]
	private Vector3 _center;

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _height;

	[SerializeField]
	[Range(-5f, 5f)]
	private float _tilt;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _step;

	[SerializeField]
	private PlayerSpiritBones.PoseType _pose;

	private CharacterController characterController_0;

	private CapsuleCollider capsuleCollider_0;

	private Rigidbody rigidbody_0;

	private TriggerColliderSearcher triggerColliderSearcher_0;

	private Mode mode_0;

	public float slopeLimit => _slopeLimit;

	public float stepOffset => _stepOffset;

	public float skinWidth => _skinWidth;

	public Vector3 center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
		}
	}

	public float radius => _radius;

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	public float tilt
	{
		get
		{
			return _tilt;
		}
		set
		{
			_tilt = value;
		}
	}

	public float step
	{
		get
		{
			return _step;
		}
		set
		{
			_step = value;
		}
	}

	public PlayerSpiritBones.PoseType pose
	{
		get
		{
			return _pose;
		}
		set
		{
			_pose = value;
		}
	}

	public CharacterController CharacterController => characterController_0;

	public CapsuleCollider CapsuleCollider => capsuleCollider_0;

	public Rigidbody Rigidbody => rigidbody_0;

	public TriggerColliderSearcher TriggerColliderSearcher => triggerColliderSearcher_0;

	public ICharacterController Spawn(Mode settings, IPlayer player, GameObject gameObject, bool isSpirit, bool isThirdPerson)
	{
		ICharacterController characterController = null;
		mode_0 = settings;
		Collider collider = null;
		if (mode_0.Type == ControllerType.Unity)
		{
			if (characterController_0 == null)
			{
				characterController_0 = GClass6.GetOrAddComponent<CharacterController>(gameObject);
			}
			characterController_0.slopeLimit = _slopeLimit;
			characterController_0.stepOffset = _stepOffset;
			characterController_0.skinWidth = _skinWidth;
			characterController_0.minMoveDistance = _minMoveDistance;
			characterController_0.center = _center;
			characterController_0.radius = _radius;
			characterController_0.height = _height;
			collider = characterController_0;
			characterController = ((!isSpirit) ? ((ICharacterController)new GClass778(CharacterController)) : ((ICharacterController)new GClass785(CharacterController)));
		}
		else if (mode_0.Type == ControllerType.Impostor)
		{
			method_1();
			collider = capsuleCollider_0;
			characterController = new GClass782(mode_0.WithMovementValidation, CapsuleCollider, Rigidbody, player, skinWidth, LayerMaskClass.PlayerStaticCollisionsMask);
		}
		else if (mode_0.Type == ControllerType.SteeringImpostor)
		{
			method_1();
			collider = capsuleCollider_0;
			characterController = new GClass783(mode_0.WithMovementValidation, CapsuleCollider, Rigidbody, player, skinWidth, LayerMaskClass.PlayerCollisionsMask);
		}
		else if (mode_0.Type == ControllerType.BotAISteeringImpostorWithDoors)
		{
			method_1();
			collider = capsuleCollider_0;
			characterController = new GClass783(mode_0.WithMovementValidation, CapsuleCollider, Rigidbody, player, skinWidth, LayerMaskClass.PlayerStaticDoorMask);
		}
		else if (mode_0.Type == ControllerType.Simple)
		{
			method_1();
			collider = capsuleCollider_0;
			SimpleCharacterController orAddComponent = GClass6.GetOrAddComponent<SimpleCharacterController>(gameObject);
			orAddComponent.Init(CapsuleCollider.transform, new Collider[1] { CapsuleCollider }, CapsuleCollider, EFTHardSettings.Instance.SIMPLE_CHARACTER_CONTROLLER_FIXED_DELTA_DISTANCE, LayerMaskClass.PlayerCollisionsMask, stepOffset, slopeLimit);
			if (Singleton<GameWorld>.Instantiated)
			{
				orAddComponent.RegisterCanSeepThroughDelegate((Collider x) => Singleton<GameWorld>.Instance.CanPlayerSeepThrough(x));
			}
			orAddComponent.AddNoSpeedLimitCollisionColliders(LocationScene.DoorsCollisionColliders);
			characterController = orAddComponent;
		}
		method_0(collider, isThirdPerson);
		if (capsuleCollider_0 != null)
		{
			capsuleCollider_0.isTrigger = mode_0.WithTriggerCollider;
		}
		if (mode_0.WithRigidbody)
		{
			CreateRigidbody();
		}
		if (characterController == null)
		{
			throw new Exception("Character Controller uninitialized");
		}
		return characterController;
	}

	public ICharacterController ReplaceSpawn(Mode settings, Player player, GameObject gameObject, bool isSpirit, bool isThirdPerson)
	{
		ICharacterController component = gameObject.GetComponent<ICharacterController>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component as Component);
			component = null;
		}
		return Spawn(settings, player, gameObject, isSpirit, isThirdPerson);
	}

	public void method_0(Collider collider, bool thirdPerson)
	{
		triggerColliderSearcher_0 = GClass6.GetOrAddComponent<TriggerColliderSearcher>(base.gameObject);
		if (!triggerColliderSearcher_0.IsInited)
		{
			if (thirdPerson)
			{
				triggerColliderSearcher_0.Init(collider, EFTHardSettings.Instance.TriggersCastLayerMaskForObservedPlayers);
			}
			else
			{
				triggerColliderSearcher_0.Init(collider, EFTHardSettings.Instance.TriggersCastLayerMask);
			}
		}
	}

	public void method_1()
	{
		if (capsuleCollider_0 == null)
		{
			capsuleCollider_0 = GClass6.GetOrAddComponent<CapsuleCollider>(base.gameObject);
			capsuleCollider_0.center = _center;
			capsuleCollider_0.radius = _radius;
			capsuleCollider_0.height = _height;
		}
		capsuleCollider_0.enabled = true;
	}

	public void CreateRigidbody()
	{
		if (rigidbody_0 == null)
		{
			rigidbody_0 = GClass6.GetOrAddComponent<Rigidbody>(base.gameObject);
		}
		rigidbody_0.isKinematic = mode_0.WithKinematicOption;
		rigidbody_0.useGravity = false;
		rigidbody_0.constraints = (RigidbodyConstraints)80;
	}
}
