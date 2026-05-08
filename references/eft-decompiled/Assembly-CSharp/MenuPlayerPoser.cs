using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AnimationEventSystem;
using EFT;
using JetBrains.Annotations;
using RootMotion.FinalIK;
using UnityEngine;

public class MenuPlayerPoser : MonoBehaviour, IActorEvents
{
	public enum EMenuPlayerAnimationType
	{
		Idle,
		RandomAnimation,
		WeaponAnimation,
		GestureAnimation
	}

	[Serializable]
	[CompilerGenerated]
	public class Class582
	{
		public static readonly Class582 class582_0 = new Class582();

		public static Func<GripPose, bool> func_0;

		public static Func<GripPose, bool> func_1;

		public static Func<GripPose, bool> func_2;

		public static Func<GripPose, bool> func_3;

		public static Func<GripPose, bool> func_4;

		public static Func<GripPose, bool> func_5;

		public static Func<EInteraction, bool> func_6;

		public bool method_0(GripPose x)
		{
			return x.Hand == GripPose.EHand.Left;
		}

		public bool method_1(GripPose x)
		{
			return x.GripType == GripPose.EGripType.UnderbarrelWeapon;
		}

		public bool method_2(GripPose x)
		{
			return x.GripType != GripPose.EGripType.Common;
		}

		public bool method_3(GripPose x)
		{
			return x.GripType == GripPose.EGripType.Common;
		}

		public bool method_4(GripPose x)
		{
			return x.Hand == GripPose.EHand.Right;
		}

		public bool method_5(GripPose x)
		{
			return x.GripType != GripPose.EGripType.Alternative;
		}

		public bool method_6(EInteraction g)
		{
			return g != EInteraction.None;
		}
	}

	private const int MIN_AMMO_COUNT = 2;

	private const int MAX_AMMO_COUNT = 10;

	public LimbIK[] Limbs;

	public PlayerBones PBones;

	public TwistRelax[] Twists;

	public HandPoser LeftHandPoser;

	public HandPoser RightHandPoser;

	public GameObject BottomShadow;

	private bool _initialized;

	private Transform[] _markers = new Transform[2];

	private readonly Transform[] _gripReferences = new Transform[2];

	private HandPoser[] _handPosers;

	private Quaternion _ikRotation;

	private Vector3 _ikPosition;

	private Transform[] _elbowBends = new Transform[2];

	private IAnimator _weaponAnimator;

	private int _randomAnimationsCount;

	private bool _patrolDelayed;

	private bool _canReload;

	private bool _canCheckChamber;

	private int _ammoToLoad = -1;

	private int _weaponAnimationType = -1;

	private EInteraction _gestureAnimationType;

	private AnimationEventsEmitter _animEventEmitter;

	private List<IActorEvents> _eventConsumers = new List<IActorEvents>();

	private GInterface26[] _grips = new GInterface26[2];

	private GClass703 _blender;

	private Player.ValueBlender _crutchBlender = new Player.ValueBlender(1)
	{
		Speed = 2f
	};

	private List<EInteraction> _gestures = (from EInteraction g in Enum.GetValues(typeof(EInteraction))
		where g != EInteraction.None
		select g).ToList();

	private List<(int Parameter, Action<IAnimator> AnimateAction)> _weaponAnimations = new List<(int, Action<IAnimator>)>();

	private bool _animate;

	public Animator PlayerAnimatorController { get; set; }

	public bool Boolean_0
	{
		set
		{
			WeaponAnimationSpeedControllerClass.SetPatrol(_weaponAnimator, value);
			if (_blender != null)
			{
				_blender.Blender.Target = ((!value) ? 1 : 0);
			}
			_crutchBlender.Target = (value ? 1 : 0);
		}
	}

	public static int Int32_0 => UnityEngine.Random.Range(2, 11);

	public void Init(Animator playerAnimatorController)
	{
		_eventConsumers.Add(this);
		LimbIK obj = Limbs[0];
		Limbs[1].enabled = false;
		obj.enabled = false;
		_handPosers = new HandPoser[2] { LeftHandPoser, RightHandPoser };
		LeftHandPoser.enabled = false;
		RightHandPoser.enabled = false;
		PlayerAnimatorController = playerAnimatorController;
		_randomAnimationsCount = PlayerAnimatorController.GetInteger(PlayerAnimator.UI_RANDOM_ANIMATIONS_COUNT);
		_initialized = true;
	}

	public void ChangeWeapon([CanBeNull] AnimationEventsEmitter eventsEmitter, [CanBeNull] IAnimator animator, [CanBeNull] TransformLinks links, bool canReload, bool canCheckChamber, bool animate = true)
	{
		if (!_initialized)
		{
			Init(GetComponent<Animator>());
		}
		method_6();
		_canReload = canReload;
		_canCheckChamber = canCheckChamber;
		_weaponAnimationType = -1;
		_gestureAnimationType = EInteraction.None;
		_animate = animate;
		_weaponAnimations = new List<(int, Action<IAnimator>)> { (WeaponAnimationSpeedControllerClass.TRIGGER_LOOK, WeaponAnimationSpeedControllerClass.TriggerLook) };
		if (_canCheckChamber)
		{
			_weaponAnimations.Add((WeaponAnimationSpeedControllerClass.TRIGGER_CHECKCHAMBER, WeaponAnimationSpeedControllerClass.TriggerCheckChamber));
		}
		if (_canReload)
		{
			_weaponAnimations.Add((WeaponAnimationSpeedControllerClass.TRIGGER_CHECKAMMO, WeaponAnimationSpeedControllerClass.TriggerCheckAmmo));
			_weaponAnimations.Add((WeaponAnimationSpeedControllerClass.TRIGGER_RELOAD, WeaponAnimationSpeedControllerClass.TriggerReload));
		}
		if (links != null)
		{
			PBones.UpdateImportantBones(links);
		}
		_animEventEmitter = eventsEmitter;
		if (_animEventEmitter != null)
		{
			_animEventEmitter.OnEventAction += method_4;
		}
		if (animator != null)
		{
			_weaponAnimator = animator;
			_weaponAnimator.enabled = true;
			WeaponAnimationSpeedControllerClass.SetActive(_weaponAnimator, active: true);
			Boolean_0 = true;
			_weaponAnimator.Play("PATROL", 1, 0f);
			WeaponAnimationSpeedControllerClass.SetCanReload(_weaponAnimator, _canReload);
		}
		if (links != null)
		{
			links.GatherIK(_markers, _gripReferences, _elbowBends);
		}
		method_5(links);
		LateUpdate();
	}

	public void OnWeaponOperationStart()
	{
		if (_initialized)
		{
			if (_weaponAnimationType >= 0)
			{
				_weaponAnimations[_weaponAnimationType].AnimateAction(_weaponAnimator);
				return;
			}
			if (_gestureAnimationType != EInteraction.None)
			{
				method_1(_gestureAnimationType);
				return;
			}
			_patrolDelayed = false;
			PlayerAnimatorController.SetBool(PlayerAnimator.UI_WEAPON_OPERATION, value: false);
			OnWeaponOperationEnd();
		}
	}

	public void OnWeaponOperationEnd()
	{
		if (_initialized)
		{
			Boolean_0 = true;
			WeaponAnimationSpeedControllerClass.SetCanReload(_weaponAnimator, _canReload);
		}
	}

	public void OnRandomizeAnimation()
	{
		if (!_initialized)
		{
			return;
		}
		if (_animate && _weaponAnimator != null)
		{
			switch ((EMenuPlayerAnimationType)UnityEngine.Random.Range(0, 3))
			{
			default:
				throw new ArgumentOutOfRangeException();
			case EMenuPlayerAnimationType.RandomAnimation:
				PlayerAnimatorController.SetInteger(PlayerAnimator.UI_RANDOM_TRANSITION, UnityEngine.Random.Range(1, _randomAnimationsCount + 1));
				return;
			case EMenuPlayerAnimationType.WeaponAnimation:
			{
				_weaponAnimationType = UnityEngine.Random.Range(0, _weaponAnimations.Count);
				(int, Action<IAnimator>) tuple = _weaponAnimations[_weaponAnimationType];
				if (!_weaponAnimator.HasParameter(tuple.Item1))
				{
					_weaponAnimationType = -1;
					PlayerAnimatorController.SetInteger(PlayerAnimator.UI_RANDOM_TRANSITION, 0);
				}
				else
				{
					method_0();
				}
				return;
			}
			case EMenuPlayerAnimationType.GestureAnimation:
				_gestureAnimationType = _gestures[UnityEngine.Random.Range(1, _gestures.Count)];
				method_0();
				return;
			case EMenuPlayerAnimationType.Idle:
				break;
			}
		}
		PlayerAnimatorController.SetInteger(PlayerAnimator.UI_RANDOM_TRANSITION, 0);
	}

	public void method_0()
	{
		PlayerAnimatorController.SetBool(PlayerAnimator.UI_WEAPON_OPERATION, value: true);
		PlayerAnimatorController.SetInteger(PlayerAnimator.UI_RANDOM_TRANSITION, 0);
		Boolean_0 = false;
		_patrolDelayed = true;
	}

	public void method_1(EInteraction index)
	{
		WeaponAnimationSpeedControllerClass.TriggerGesture(_weaponAnimator);
		WeaponAnimationSpeedControllerClass.SetUseLeftHand(_weaponAnimator, useLeftHand: true);
		WeaponAnimationSpeedControllerClass.SetLActionIndex(_weaponAnimator, 0);
		WeaponAnimationSpeedControllerClass.SetGestureIndex(_weaponAnimator, (int)(index - 1));
	}

	public void method_2()
	{
		if (_ammoToLoad < 0)
		{
			_ammoToLoad = Int32_0;
		}
		_ammoToLoad--;
		if (_ammoToLoad == 0)
		{
			WeaponAnimationSpeedControllerClass.SetCanReload(_weaponAnimator, canReload: false);
			_ammoToLoad = Int32_0;
		}
	}

	public void method_3()
	{
		if (_patrolDelayed)
		{
			_patrolDelayed = false;
		}
		else
		{
			PlayerAnimatorController.SetBool(PlayerAnimator.UI_WEAPON_OPERATION, value: false);
		}
	}

	public void method_4(int functionNameHash, AnimationEventParameter parameter)
	{
		GClass756.AnimatorEventHandler(_eventConsumers, functionNameHash, parameter);
	}

	public void method_5(TransformLinks transforms = null)
	{
		_handPosers[0].GripWeight = 0f;
		float weight;
		if (transforms == null)
		{
			HandPoser obj = _handPosers[0];
			HandPoser obj2 = _handPosers[1];
			weight = 0f;
			obj2.weight = 0f;
			obj.weight = weight;
			return;
		}
		_handPosers[0].MapGrip(transforms.GetTransform(ECharacterWeaponBones.HumanLPalm));
		_handPosers[1].MapGrip(transforms.GetTransform(ECharacterWeaponBones.HumanRPalm));
		HandPoser obj3 = _handPosers[0];
		HandPoser obj4 = _handPosers[1];
		weight = 1f;
		obj4.weight = 1f;
		obj3.weight = weight;
		GripPose[] source = transforms.Self.GetComponentsInChildren<GripPose>().ToArray();
		GripPose[] source2 = (from x in source
			where x.Hand == GripPose.EHand.Left
			orderby x.GripType == GripPose.EGripType.UnderbarrelWeapon descending, HandPoser.NumParents(x.transform, PBones.WeaponRoot.Original) descending
			select x).ToArray();
		GripPose gripPose = source2.FirstOrDefault((GripPose x) => x.GripType != GripPose.EGripType.Common);
		GripPose gripPose2 = source2.FirstOrDefault((GripPose x) => x.GripType == GripPose.EGripType.Common);
		GripPose gripPose3 = (from x in source
			where x.Hand == GripPose.EHand.Right
			orderby x.GripType != GripPose.EGripType.Alternative, HandPoser.NumParents(x.transform, PBones.WeaponRoot.Original) descending
			select x).FirstOrDefault();
		if (gripPose != null && gripPose2 != null)
		{
			if (gripPose.GripType == GripPose.EGripType.UnderbarrelWeapon)
			{
				gripPose2 = gripPose;
			}
			_blender = new GClass703(gripPose, gripPose2, 2f, 0f);
			_grips = new GInterface26[2] { _blender, gripPose3 };
		}
		else
		{
			_grips = new GInterface26[2]
			{
				gripPose ? gripPose : gripPose2,
				gripPose3
			};
		}
		_handPosers[0].SetGrip(_grips[0]);
		_handPosers[1].SetGrip(_grips[1]);
	}

	public void LateUpdate()
	{
		if (!_initialized)
		{
			return;
		}
		_animEventEmitter?.EmitEvents();
		if (_weaponAnimator == null || _weaponAnimator.runtimeAnimatorController == null)
		{
			return;
		}
		float num = 1f - PlayerAnimatorController.GetFloat(PlayerAnimator.RIGHT_HAND_WEIGHT);
		float weight = 1f - PlayerAnimatorController.GetFloat(PlayerAnimator.LEFT_HAND_WEIGHT);
		float num2 = PlayerAnimatorController.GetFloat(PlayerAnimator.WEAPON_ROOT_3RD);
		float num3 = 1f - PlayerAnimatorController.GetFloat(PlayerAnimator.ELBOW_LEFT_WEIGHT);
		float num4 = 1f - PlayerAnimatorController.GetFloat(PlayerAnimator.ELBOW_RIGHT_WEIGHT);
		if (num < 1f && _markers[1] != null)
		{
			PBones.Kinematics(_markers[1], 0f);
		}
		else if (num2 > 0.01f)
		{
			PBones.ShiftWeaponRoot(Time.deltaTime, EPointOfView.ThirdPerson, num2);
		}
		else
		{
			PBones.WeaponRoot.Original.localPosition = Vector3.zero;
			PBones.WeaponRoot.Original.localRotation = Quaternion.identity;
		}
		Limbs[0].solver.IKRotationWeight = (Limbs[0].solver.IKPositionWeight = (_handPosers[0].weight = weight));
		Limbs[1].solver.IKRotationWeight = (Limbs[1].solver.IKPositionWeight = num);
		if (_elbowBends[0] != null && _elbowBends[1] != null)
		{
			for (int i = 0; i < PBones.BendGoals.Length; i++)
			{
				Transform obj = PBones.BendGoals[i];
				float t = ((i == 0) ? num3 : num4);
				obj.position = Vector3.Lerp(obj.position, _elbowBends[i].position, t);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (_markers[j] == null || Math.Abs(Limbs[j].solver.IKPositionWeight) < float.Epsilon)
			{
				continue;
			}
			if (_grips[j] != null)
			{
				float num5 = Vector3.Distance(_markers[j].position, _gripReferences[j].position);
				if (_grips[j].IsAlternative)
				{
					num5 *= 1f - _crutchBlender.Value;
				}
				float num6 = Mathf.InverseLerp(0.1f, 0f, num5);
				_handPosers[j].GripWeight = num6;
				_ikPosition = Vector3.Lerp(_markers[j].position, _grips[j].Position, num6);
				_ikRotation = Quaternion.Lerp(_markers[j].rotation, _grips[j].Rotation, num6);
			}
			else
			{
				_ikPosition = _markers[j].position;
				_ikRotation = _markers[j].rotation;
			}
			Limbs[j].solver.SetIKPosition(_ikPosition);
			Limbs[j].solver.SetIKRotation(_ikRotation);
		}
		LimbIK[] limbs = Limbs;
		for (int k = 0; k < limbs.Length; k++)
		{
			limbs[k].solver.Update();
		}
		_handPosers[0].ManualUpdate();
		_handPosers[1].ManualUpdate();
		_blender?.Update();
		TwistRelax[] twists = Twists;
		for (int k = 0; k < twists.Length; k++)
		{
			twists[k].Relax();
		}
	}

	public void OnDestroy()
	{
		method_6();
	}

	public void method_6()
	{
		if (_animEventEmitter != null)
		{
			_animEventEmitter.OnEventAction -= method_4;
		}
	}

	public void OnAddAmmoInChamber()
	{
		method_2();
	}

	void IActorEvents.OnAddAmmoInChamber()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInChamber
		this.OnAddAmmoInChamber();
	}

	public void OnAddAmmoInMag()
	{
		method_2();
	}

	void IActorEvents.OnAddAmmoInMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInMag
		this.OnAddAmmoInMag();
	}

	public void OnArm()
	{
	}

	void IActorEvents.OnArm()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnArm
		this.OnArm();
	}

	public void OnCook()
	{
	}

	void IActorEvents.OnCook()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnCook
		this.OnCook();
	}

	public void OnDelAmmoChamber()
	{
	}

	void IActorEvents.OnDelAmmoChamber()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoChamber
		this.OnDelAmmoChamber();
	}

	public void OnDelAmmoFromMag()
	{
	}

	void IActorEvents.OnDelAmmoFromMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoFromMag
		this.OnDelAmmoFromMag();
	}

	public void OnDisarm()
	{
	}

	void IActorEvents.OnDisarm()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDisarm
		this.OnDisarm();
	}

	public void OnFireEnd()
	{
	}

	void IActorEvents.OnFireEnd()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFireEnd
		this.OnFireEnd();
	}

	public void OnFiringBullet()
	{
	}

	void IActorEvents.OnFiringBullet()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFiringBullet
		this.OnFiringBullet();
	}

	public void OnFoldOff()
	{
	}

	void IActorEvents.OnFoldOff()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFoldOff
		this.OnFoldOff();
	}

	public void OnFoldOn()
	{
	}

	void IActorEvents.OnFoldOn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFoldOn
		this.OnFoldOn();
	}

	public void OnIdleStart()
	{
		method_3();
	}

	void IActorEvents.OnIdleStart()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnIdleStart
		this.OnIdleStart();
	}

	public void OnMagHide()
	{
	}

	void IActorEvents.OnMagHide()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagHide
		this.OnMagHide();
	}

	public void OnMagIn()
	{
	}

	void IActorEvents.OnMagIn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagIn
		this.OnMagIn();
	}

	public void OnMagOut()
	{
	}

	void IActorEvents.OnMagOut()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagOut
		this.OnMagOut();
	}

	public void OnMagShow()
	{
	}

	void IActorEvents.OnMagShow()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagShow
		this.OnMagShow();
	}

	public void OnMessageName()
	{
	}

	void IActorEvents.OnMessageName()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMessageName
		this.OnMessageName();
	}

	public void OnMalfunctionOff()
	{
	}

	void IActorEvents.OnMalfunctionOff()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMalfunctionOff
		this.OnMalfunctionOff();
	}

	public void OnModChanged()
	{
	}

	void IActorEvents.OnModChanged()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnModChanged
		this.OnModChanged();
	}

	public void OnOffBoltCatch()
	{
	}

	void IActorEvents.OnOffBoltCatch()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnOffBoltCatch
		this.OnOffBoltCatch();
	}

	public void OnOnBoltCatch()
	{
	}

	void IActorEvents.OnOnBoltCatch()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnOnBoltCatch
		this.OnOnBoltCatch();
	}

	public void OnPutMagToRig()
	{
	}

	void IActorEvents.OnPutMagToRig()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPutMagToRig
		this.OnPutMagToRig();
	}

	public void OnRemoveShell()
	{
	}

	void IActorEvents.OnRemoveShell()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnRemoveShell
		this.OnRemoveShell();
	}

	public void OnReplaceSecondMag()
	{
	}

	void IActorEvents.OnReplaceSecondMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnReplaceSecondMag
		this.OnReplaceSecondMag();
	}

	public void OnShellEject()
	{
	}

	void IActorEvents.OnShellEject()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShellEject
		this.OnShellEject();
	}

	public void OnShowAmmo(bool BoolParam)
	{
	}

	void IActorEvents.OnShowAmmo(bool BoolParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShowAmmo
		this.OnShowAmmo(BoolParam);
	}

	public void OnSound(string StringParam)
	{
	}

	void IActorEvents.OnSound(string StringParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSound
		this.OnSound(StringParam);
	}

	public void OnSoundAtPoint(string StringParam)
	{
	}

	void IActorEvents.OnSoundAtPoint(string StringParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSoundAtPoint
		this.OnSoundAtPoint(StringParam);
	}

	public void OnStartUtilityOperation()
	{
	}

	void IActorEvents.OnStartUtilityOperation()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnStartUtilityOperation
		this.OnStartUtilityOperation();
	}

	public void OnThirdAction(int IntParam)
	{
	}

	void IActorEvents.OnThirdAction(int IntParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnThirdAction
		this.OnThirdAction(IntParam);
	}

	public void OnUseSecondMagForReload()
	{
	}

	void IActorEvents.OnUseSecondMagForReload()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnUseSecondMagForReload
		this.OnUseSecondMagForReload();
	}

	public void OnWeapIn()
	{
	}

	void IActorEvents.OnWeapIn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnWeapIn
		this.OnWeapIn();
	}

	public void OnWeapOut()
	{
	}

	void IActorEvents.OnWeapOut()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnWeapOut
		this.OnWeapOut();
	}

	public void OnLauncherAppeared()
	{
	}

	void IActorEvents.OnLauncherAppeared()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnLauncherAppeared
		this.OnLauncherAppeared();
	}

	public void OnLauncherDisappeared()
	{
	}

	void IActorEvents.OnLauncherDisappeared()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnLauncherDisappeared
		this.OnLauncherDisappeared();
	}

	public void OnShowMag()
	{
	}

	void IActorEvents.OnShowMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShowMag
		this.OnShowMag();
	}

	public void OnSliderOut()
	{
	}

	void IActorEvents.OnSliderOut()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSliderOut
		this.OnSliderOut();
	}

	public void OnUseProp(bool BoolParam)
	{
	}

	void IActorEvents.OnUseProp(bool BoolParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnUseProp
		this.OnUseProp(BoolParam);
	}

	public void OnBackpackDrop()
	{
	}

	void IActorEvents.OnBackpackDrop()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBackpackDrop
		this.OnBackpackDrop();
	}

	public void OnComboPlanning()
	{
	}

	void IActorEvents.OnComboPlanning()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnComboPlanning
		this.OnComboPlanning();
	}

	public void OnCurrentAnimStateEnded()
	{
	}

	void IActorEvents.OnCurrentAnimStateEnded()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnCurrentAnimStateEnded
		this.OnCurrentAnimStateEnded();
	}

	public void OnSetActiveObject(int objectID)
	{
	}

	void IActorEvents.OnSetActiveObject(int objectID)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSetActiveObject
		this.OnSetActiveObject(objectID);
	}

	public void OnDeactivateObject(int objectID)
	{
	}

	void IActorEvents.OnDeactivateObject(int objectID)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDeactivateObject
		this.OnDeactivateObject(objectID);
	}

	public void OutUse()
	{
	}

	void IActorEvents.OutUse()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OutUse
		this.OutUse();
	}

	public void ReloadTest()
	{
	}

	public void BipodOpen()
	{
	}

	public void BipodClose()
	{
	}

	public void AimReady()
	{
	}

	public void IdleReady()
	{
	}

	public void DropWeapon()
	{
	}

	[CompilerGenerated]
	public int method_7(GripPose x)
	{
		return HandPoser.NumParents(x.transform, PBones.WeaponRoot.Original);
	}

	[CompilerGenerated]
	public int method_8(GripPose x)
	{
		return HandPoser.NumParents(x.transform, PBones.WeaponRoot.Original);
	}
}
