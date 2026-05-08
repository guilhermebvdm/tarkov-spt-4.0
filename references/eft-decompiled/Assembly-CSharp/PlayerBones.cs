using System.Collections.Generic;
using System.Linq;
using CommonAssets.Scripts;
using EFT;
using EFT.WeaponMounting;
using FastAnimatorSystem;
using UnityEngine;

public class PlayerBones : MonoBehaviour
{
	public struct GStruct41
	{
		public ETransitionType type;

		public AnimationCurve c;

		public bool executed;
	}

	public enum ETransitionType
	{
		In,
		Out,
		None
	}

	public Player Player;

	public PlayableAnimator PlayableAnimator;

	public WeaponMountingView weaponMountingView;

	public Transform Weapon_Root_Third;

	public Transform Weapon_Root_Anim;

	public Transform Neck;

	public Transform Weapon_reference_point;

	public Vector3 R_Neck;

	public Vector3 R_Head;

	public Vector3 R_Neck_LeftStance;

	public Vector3 R_Head_LeftStance;

	public Transform[] Shoulders;

	public Transform[] Shoulders_Anim = new Transform[2];

	public Transform[] Upperarms;

	public Transform[] Forearms;

	public Transform LeftPalm;

	public Transform RightPalm;

	public Transform LootRaycastOrigin;

	public Transform RootJoint;

	public Transform VaultingGridRoot;

	public Transform WeaponMeleeRoot;

	public Transform WeaponPistolRoot;

	public Transform WeaponRifleRoot;

	public Transform HolsterPrimary;

	public Transform HolsterPrimaryAlternative;

	public Transform HolsterSecondary;

	public Transform HolsterSecondaryAlternative;

	public Transform ScabbardTagillaMelee;

	public Transform HolsterPistol;

	public Transform LeftLegHolsterPistol;

	public Transform[] BendGoals;

	public Transform KickingFoot;

	public Transform[] FovSpecialTransforms;

	public BifacialTransform WeaponRoot;

	public BifacialTransform Ribcage;

	public BifacialTransform Head;

	public BifacialTransform LeftShoulder;

	public BifacialTransform RightShoulder;

	public BifacialTransform LeftThigh2;

	public BifacialTransform RightThigh2;

	public BifacialTransform BodyTransform;

	public BifacialTransform AnimatedTransform;

	public BifacialTransform Pelvis;

	public BifacialTransform LeftThigh1;

	public BifacialTransform RightThigh1;

	public BifacialTransform Spine3;

	public Vector3 Offset = Vector3.zero;

	public Quaternion DeltaRotation = Quaternion.identity;

	public BodyPartCollider LeftHandCollider;

	[HideInInspector]
	public BifacialTransform Fireport = new BifacialTransform();

	public BifacialTransform LeftMultiBarrelFireport = new BifacialTransform();

	public BifacialTransform RightMultiBarrelFireport = new BifacialTransform();

	public BifacialTransform[] MultiBarrelsFireports;

	public readonly Dictionary<PlayerBoneType, BifacialTransform> BifacialTransforms = GClass866<PlayerBoneType>.GetDictWith<BifacialTransform>();

	public readonly Dictionary<EBodyPartColliderType, BodyPartCollider> BodyPartCollidersDictionary = GClass866<EBodyPartColliderType>.GetDictWith<BodyPartCollider>();

	public static readonly Dictionary<EBodyPartColliderType, EBodyPart> BodyPartCollidersStaticMap = GClass866<EBodyPartColliderType>.GetDictWith<EBodyPart>();

	public readonly Dictionary<EArmorPlateCollider, ArmorPlateCollider> ArmorPlateCollidersDict = GClass866<EArmorPlateCollider>.GetDictWith<ArmorPlateCollider>();

	public BodyPartCollider[] BodyPartColliders;

	public ArmorPlateCollider[] ArmorPlateColliders;

	public Collider HeadCameraCollider;

	public readonly HashSet<Collider> BodyPartCollidersHashSet = new HashSet<Collider>();

	private AnimationCurve _transitionCurve;

	private float _transitionTime;

	private float _transitionLn;

	private ETransitionType _transitionType = ETransitionType.None;

	public AnimationCurve HeadRotationCurve;

	public Transform Spine1;

	private GStruct41 _transition;

	public float HeadChain = 0.5f;

	public float NeckChain = 0.5f;

	private float _offsetForRootAimInLeftStance = 0.2f;

	private readonly Vector3 _offsterDirrectionForLeftStanceInAim = new Vector3(0.13f, -0.305f, 0.023f);

	public float leftStanceRotationAndPositionChangeSpeed = 10f;

	public float leftStanceReturnBackSpeed = 20f;

	private Quaternion _lastWeaponRotationInLeftStance;

	private Vector3 targetLSPosition;

	private bool _gotFullLeftStanceLastFrame = true;

	private bool _wasMovingInLsAim;

	private Vector3 _targetLSInAimPosition;

	public void RotateHead(float blendValue, Vector3 offset, bool leftStance, float leftStanceCurveValue, bool isAiming)
	{
		if (!(blendValue <= 0f) || !(offset.sqrMagnitude < 0.01f))
		{
			float num = HeadRotationCurve.Evaluate(blendValue);
			Quaternion quaternion = Quaternion.Euler(offset * NeckChain);
			Quaternion quaternion2 = Quaternion.Euler(offset * HeadChain);
			Vector3 euler = (leftStance ? R_Neck_LeftStance : R_Neck);
			Vector3 euler2 = (leftStance ? R_Head_LeftStance : R_Head);
			Quaternion a = Quaternion.identity;
			Quaternion a2 = Quaternion.identity;
			if (isAiming && (leftStance || leftStanceCurveValue > 0f))
			{
				a = ((!leftStance) ? ((blendValue >= 1f) ? Quaternion.Euler(R_Neck_LeftStance) : Quaternion.identity) : ((blendValue >= 1f) ? Quaternion.Euler(R_Neck) : Quaternion.identity));
				a2 = ((!leftStance) ? ((blendValue >= 1f) ? Quaternion.Euler(R_Head_LeftStance) : Quaternion.identity) : ((blendValue >= 1f) ? Quaternion.Euler(R_Head) : Quaternion.identity));
				num *= leftStanceCurveValue;
				num = (leftStance ? num : (1f - num));
			}
			Neck.transform.localRotation *= Quaternion.Lerp(a, Quaternion.Euler(euler), num) * quaternion;
			Head.Original.transform.localRotation *= Quaternion.Lerp(a2, Quaternion.Euler(euler2), num) * quaternion2;
		}
	}

	public void SetArmorPlateCollidersState(EArmorPlateCollider activeCollidersMask)
	{
		ArmorPlateCollider[] armorPlateColliders = ArmorPlateColliders;
		foreach (ArmorPlateCollider armorPlateCollider in armorPlateColliders)
		{
			bool active;
			if ((active = activeCollidersMask.HasFlag(armorPlateCollider.ArmorPlateColliderType)) != armorPlateCollider.gameObject.activeSelf)
			{
				armorPlateCollider.gameObject.SetActive(active);
			}
		}
	}

	public void Awake()
	{
		CreateBifacialTransformsCollection();
		BodyPartCollidersHashSet.Clear();
		BodyPartCollidersDictionary.Clear();
		BodyPartCollider[] bodyPartColliders = BodyPartColliders;
		foreach (BodyPartCollider bodyPartCollider in bodyPartColliders)
		{
			BodyPartCollidersHashSet.Add(bodyPartCollider.Collider);
			BodyPartCollidersDictionary[bodyPartCollider.BodyPartColliderType] = bodyPartCollider;
			BodyPartCollidersStaticMap[bodyPartCollider.BodyPartColliderType] = bodyPartCollider.BodyPartType;
		}
		ArmorPlateCollider[] armorPlateColliders = ArmorPlateColliders;
		foreach (ArmorPlateCollider armorPlateCollider in armorPlateColliders)
		{
			BodyPartCollidersHashSet.Add(armorPlateCollider.Collider);
			Collider[] componentsInChildren = armorPlateCollider.GetComponentsInChildren<Collider>();
			foreach (Collider item in componentsInChildren)
			{
				BodyPartCollidersHashSet.Add(item);
			}
		}
		_transition.executed = true;
		armorPlateColliders = ArmorPlateColliders;
		foreach (ArmorPlateCollider armorPlateCollider2 in armorPlateColliders)
		{
			ArmorPlateCollidersDict[armorPlateCollider2.ArmorPlateColliderType] = armorPlateCollider2;
		}
		SetArmorPlateCollidersState((EArmorPlateCollider)0);
	}

	public void CreateBifacialTransformsCollection()
	{
		BifacialTransforms.Clear();
		BifacialTransforms.Add(PlayerBoneType.WeaponRoot, WeaponRoot);
		BifacialTransforms.Add(PlayerBoneType.Ribcage, Ribcage);
		BifacialTransforms.Add(PlayerBoneType.Head, Head);
		BifacialTransforms.Add(PlayerBoneType.LeftShoulder, LeftShoulder);
		BifacialTransforms.Add(PlayerBoneType.RightShoulder, RightShoulder);
		BifacialTransforms.Add(PlayerBoneType.Body, BodyTransform);
		BifacialTransforms.Add(PlayerBoneType.LeftThigh2, LeftThigh2);
		BifacialTransforms.Add(PlayerBoneType.RightThigh2, RightThigh2);
		BifacialTransforms.Add(PlayerBoneType.Fireport, Fireport);
		BifacialTransforms.Add(PlayerBoneType.Pelvis, Pelvis);
		BifacialTransforms.Add(PlayerBoneType.LeftThigh1, LeftThigh1);
		BifacialTransforms.Add(PlayerBoneType.RightThigh1, RightThigh1);
		BifacialTransforms.Add(PlayerBoneType.Spine, Spine3);
		BifacialTransforms.Add(PlayerBoneType.LeftFirePort, LeftMultiBarrelFireport);
		BifacialTransforms.Add(PlayerBoneType.RightFirePort, RightMultiBarrelFireport);
	}

	public void EnableBoneImitation(bool useImitation)
	{
		foreach (BifacialTransform value in BifacialTransforms.Values)
		{
			value.UseImitation = useImitation;
		}
	}

	public void AdjustPistolHolster(Transform holsterTransform, bool isRightLeg)
	{
		if (isRightLeg && HolsterPistol != null)
		{
			HolsterPistol.localPosition = holsterTransform.localPosition;
			HolsterPistol.localRotation = holsterTransform.localRotation;
		}
		else if (!isRightLeg && LeftLegHolsterPistol != null)
		{
			LeftLegHolsterPistol.localPosition = holsterTransform.localPosition;
			LeftLegHolsterPistol.localRotation = holsterTransform.localRotation;
		}
	}

	public void UpdateImportantBones(TransformLinks tLinks)
	{
		Weapon_Root_Anim = tLinks.GetTransform(ECharacterWeaponBones.Weapon_root_anim);
		Shoulders_Anim[0] = tLinks.GetTransform(ECharacterWeaponBones.HumanLUpperarm);
		Shoulders_Anim[1] = tLinks.GetTransform(ECharacterWeaponBones.HumanRUpperarm);
	}

	public void UpdateLauncherRightHand()
	{
		AlternativePropBone[] array = WeaponRoot.Original.GetComponentsInChildren<AlternativePropBone>().ToArray();
		if (array.Length != 0)
		{
			Weapon_Root_Anim = array[0].transform;
		}
	}

	public void SetShoulders(float str, float strRight)
	{
		TransformHelperClass.LerpPositionAndRotation(Shoulders[0], Shoulders_Anim[0].position, Shoulders_Anim[0].rotation, str);
		TransformHelperClass.LerpPositionAndRotation(Shoulders[1], Shoulders_Anim[1].position, Shoulders_Anim[1].rotation, strRight);
	}

	public void Kinematics(Transform target, float weight)
	{
		Quaternion quaternion = Quaternion.Lerp(RightPalm.rotation, target.rotation, weight);
		Vector3 vector = Vector3.Lerp(RightPalm.position, target.position, weight);
		Weapon_Root_Anim.transform.SetPositionAndRotation(vector, quaternion);
		Quaternion quaternion2 = Quaternion.Inverse(target.rotation) * quaternion;
		Transform obj = Weapon_Root_Anim.transform;
		obj.rotation *= quaternion2;
		obj.position += vector - target.position;
	}

	public void ShiftWeaponRoot(float t, EPointOfView pv, float thirdPersonAuthority, bool armsupdated = false, float positionCacheValue = 0f, float leftStanceCurveValue = 0f, bool inSprint = false, bool inLeftStanceAnimValue = false, bool inLeftStanceCacheValue = false, bool isAiming = false, bool isAnimatedInteraction = false, bool isLeftHandUsing = false)
	{
		if (pv == EPointOfView.FirstPerson && Weapon_Root_Anim != null)
		{
			if (leftStanceCurveValue >= 1f && !inSprint)
			{
				if (isAiming && Player.IsForwardInputDirection)
				{
					_wasMovingInLsAim = true;
				}
				if (isAiming && !_wasMovingInLsAim)
				{
					targetLSPosition = Vector3.Lerp(targetLSPosition, Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset, t * leftStanceRotationAndPositionChangeSpeed);
				}
				else
				{
					targetLSPosition = Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset;
				}
				Weapon_Root_Anim.position = targetLSPosition;
				if (isAiming)
				{
					_targetLSInAimPosition = Vector3.Lerp(_targetLSInAimPosition, Ribcage.Original.localPosition + _offsterDirrectionForLeftStanceInAim * _offsetForRootAimInLeftStance, t);
					Weapon_Root_Anim.localPosition = _targetLSInAimPosition;
					Quaternion rotation = Weapon_Root_Anim.rotation;
					Vector3 localEulerAngles = Weapon_Root_Anim.localEulerAngles;
					Weapon_Root_Anim.rotation = Quaternion.Lerp(_lastWeaponRotationInLeftStance, rotation, t * leftStanceRotationAndPositionChangeSpeed);
					Vector3 localEulerAngles2 = new Vector3(localEulerAngles.x, Weapon_Root_Anim.localEulerAngles.y, localEulerAngles.z);
					Weapon_Root_Anim.localEulerAngles = localEulerAngles2;
					Weapon_Root_Anim.localPosition += Player.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.GetHandPositionRecoil();
				}
				else
				{
					Vector3 localEulerAngles3 = Weapon_Root_Anim.localEulerAngles;
					if (_gotFullLeftStanceLastFrame)
					{
						_lastWeaponRotationInLeftStance = Weapon_Root_Third.rotation * DeltaRotation;
					}
					else
					{
						_lastWeaponRotationInLeftStance = Quaternion.Lerp(_lastWeaponRotationInLeftStance, Weapon_Root_Third.rotation * DeltaRotation, t * ((!inLeftStanceAnimValue) ? leftStanceReturnBackSpeed : leftStanceRotationAndPositionChangeSpeed));
					}
					Weapon_Root_Anim.rotation = _lastWeaponRotationInLeftStance;
					Vector3 localEulerAngles4 = new Vector3(localEulerAngles3.x, Weapon_Root_Anim.localEulerAngles.y, localEulerAngles3.z);
					Weapon_Root_Anim.localEulerAngles = localEulerAngles4;
				}
				_lastWeaponRotationInLeftStance = Weapon_Root_Anim.rotation;
				_gotFullLeftStanceLastFrame = false;
				return;
			}
			_gotFullLeftStanceLastFrame = true;
			_wasMovingInLsAim = false;
			Vector3 localEulerAngles5 = Weapon_Root_Anim.localEulerAngles;
			if (positionCacheValue <= 0f)
			{
				if (isAnimatedInteraction || isLeftHandUsing)
				{
					Weapon_Root_Anim.SetPositionAndRotation(Vector3.Lerp(Weapon_Root_Anim.position, Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset, thirdPersonAuthority), Quaternion.Lerp(Weapon_Root_Anim.rotation, Weapon_Root_Third.rotation, thirdPersonAuthority));
					Weapon_Root_Anim.localRotation = DeltaRotation;
				}
				else
				{
					Weapon_Root_Anim.SetPositionAndRotation(Vector3.Lerp(Weapon_Root_Anim.position, Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset, Mathf.Max(leftStanceCurveValue, thirdPersonAuthority)), Quaternion.Lerp(Weapon_Root_Anim.rotation, Weapon_Root_Third.rotation * DeltaRotation, Mathf.Max(leftStanceCurveValue, thirdPersonAuthority)));
				}
				if (leftStanceCurveValue > 0f && !inSprint)
				{
					Weapon_Root_Anim.localEulerAngles = new Vector3(localEulerAngles5.x, Weapon_Root_Anim.localEulerAngles.y, localEulerAngles5.z);
				}
			}
			else
			{
				Weapon_Root_Anim.SetPositionAndRotation(Vector3.Lerp(Weapon_Root_Anim.position, Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset, positionCacheValue), Quaternion.Lerp(Weapon_Root_Anim.rotation, Weapon_Root_Third.rotation * DeltaRotation, positionCacheValue));
				if (leftStanceCurveValue > 0f && !inSprint)
				{
					Weapon_Root_Anim.localEulerAngles = new Vector3(localEulerAngles5.x, Weapon_Root_Anim.localEulerAngles.y, localEulerAngles5.z);
				}
			}
			targetLSPosition = Weapon_Root_Anim.position;
			_targetLSInAimPosition = Weapon_Root_Anim.localPosition;
			_lastWeaponRotationInLeftStance = Weapon_Root_Anim.rotation;
		}
		else if (pv != EPointOfView.FirstPerson)
		{
			_lastWeaponRotationInLeftStance = Weapon_Root_Anim.rotation;
			targetLSPosition = Weapon_Root_Anim.position;
			_targetLSInAimPosition = Weapon_Root_Anim.localPosition;
			if (Weapon_Root_Anim != null && Weapon_Root_Third != null)
			{
				Vector3 position = Vector3.Lerp(Weapon_Root_Anim.position, Weapon_Root_Third.position + Weapon_Root_Third.rotation * Offset, thirdPersonAuthority);
				Quaternion rotation2 = Quaternion.Lerp(Weapon_Root_Anim.rotation, Weapon_Root_Third.rotation * DeltaRotation, thirdPersonAuthority);
				Weapon_Root_Anim.SetPositionAndRotation(position, rotation2);
			}
		}
		else
		{
			_lastWeaponRotationInLeftStance = Weapon_Root_Anim.rotation;
			targetLSPosition = Weapon_Root_Anim.position;
			_targetLSInAimPosition = Weapon_Root_Anim.localPosition;
		}
	}

	public void ExecuteTransition(ref GStruct41 trans)
	{
		trans.executed = true;
		ETransitionType type = trans.type;
		AnimationCurve c = trans.c;
		if (_transitionCurve != null && (_transitionTime != _transitionLn || type == _transitionType))
		{
			if (type != _transitionType)
			{
				_transitionTime = 1f - _transitionTime / _transitionLn;
				_transitionTime = c[c.length - 1].time * _transitionTime;
			}
		}
		else
		{
			_transitionTime = 0f;
		}
		_transitionType = type;
		_transitionLn = c[c.length - 1].time;
		_transitionCurve = c;
	}

	public void Transit(AnimationCurve c, ETransitionType type)
	{
		if (!_transition.executed)
		{
			Debug.Log($"Reject {c[0].value} --> {c[1].value}  ({type.ToString()})");
		}
		_transition.c = c;
		_transition.type = type;
		_transition.executed = false;
	}

	public void IKTransit(ETransitionType eTransitionType, float speed = 4f)
	{
		if (!(Player == null))
		{
			Player.ThirdIkWeight.Target = ((eTransitionType == ETransitionType.In) ? 1 : 0);
		}
	}

	public BifacialTransform FindFireport()
	{
		Fireport.Original = TransformHelperClass.FindTransformRecursive(WeaponRoot.Original, "fireport");
		return Fireport;
	}

	public BifacialTransform[] FindMultiBarrelsFireports(bool isMultiBarrel)
	{
		if (!isMultiBarrel)
		{
			return null;
		}
		LeftMultiBarrelFireport.Original = TransformHelperClass.FindTransformRecursive(WeaponRoot.Original, "fireport_l");
		RightMultiBarrelFireport.Original = TransformHelperClass.FindTransformRecursive(WeaponRoot.Original, "fireport_r");
		MultiBarrelsFireports = new BifacialTransform[2] { LeftMultiBarrelFireport, RightMultiBarrelFireport };
		return MultiBarrelsFireports;
	}
}
