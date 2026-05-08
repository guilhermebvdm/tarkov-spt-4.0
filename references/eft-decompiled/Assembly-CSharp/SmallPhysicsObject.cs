using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using UnityEngine;

public class SmallPhysicsObject : MonoBehaviour
{
	public enum EPhysicsObjectType
	{
		Books,
		CardBoard,
		CoolerBottle,
		Cup,
		GlassBottle,
		HardPlastic,
		MediumGeneric,
		MetalCandlestick,
		Monitor,
		SmallBox,
		SmallPaper,
		SmallPlastic,
		TincanEmpty
	}

	[Serializable]
	[CompilerGenerated]
	public class Class369
	{
		public static readonly Class369 class369_0 = new Class369();

		public static Action<Rigidbody> action_0;

		public void method_0(Rigidbody x)
		{
			x.Sleep();
		}
	}

	private const float float_0 = 0.5f;

	private const float float_1 = 0.3f;

	private const float float_2 = 0.2f;

	private const float float_3 = 0.5f;

	private static readonly Collider[] collider_0 = new Collider[60];

	public EPhysicsObjectType SoundType;

	public List<BallisticCollider> BallisticColliders = new List<BallisticCollider>();

	public List<Rigidbody> Rigidbodies = new List<Rigidbody>();

	public Collider Collider;

	public GameObject SolidObject;

	public GameObject FragmentedObject;

	public float TimeOfLife = -1f;

	public float OnHitForceMultiplier = 1f;

	public float OnTouchForceMultiplier = 1f;

	public float OnBlowUpForceMultiplier = 1f;

	public float BlowUpThreshold = 8f;

	public string ParticlesEffectName;

	public List<InitialPosition> InitialPositions = new List<InitialPosition>();

	private byte byte_0;

	private float float_4;

	protected float IgnoreCollisionTrackingTimer;

	private float float_5 = -1f;

	public Vector3 TestBlowPosition;

	private Vector3 vector3_0;

	private Coroutine coroutine_0;

	private SoundBank soundBank_0;

	private readonly Vector3 vector3_1 = new Vector3(0f, 0.1f, 0f);

	public byte CollisionNumber
	{
		get
		{
			return byte_0;
		}
		set
		{
			if (value > byte_0)
			{
				byte_0 = value;
				if (Time.time > IgnoreCollisionTrackingTimer)
				{
					OnCollisionHandler();
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position + TestBlowPosition, 0.1f);
	}

	public void Awake()
	{
		GetComponentsInChildren(includeInactive: true, BallisticColliders);
		if (BallisticColliders.Count == 0)
		{
			Debug.LogError("Missing Ballistics Collider", this);
			return;
		}
		GetComponentsInChildren(includeInactive: true, Rigidbodies);
		if (Rigidbodies.Count == 0)
		{
			Debug.LogError("Missing Rigidbodies", this);
			return;
		}
		Collider = BallisticColliders[0].GetComponent<Collider>();
		if (Collider == null)
		{
			Collider = GetComponentInChildren<Collider>();
		}
		if (Collider == null)
		{
			Debug.LogError("Collider Rigidbody", this);
			return;
		}
		foreach (BallisticCollider ballisticCollider in BallisticColliders)
		{
			ballisticCollider.OnHitAction += method_6;
		}
		method_5(base.transform);
		Rigidbodies.ForEach(delegate(Rigidbody x)
		{
			x.Sleep();
		});
		SmallPhysicsSystem.Instance.AddObject(this);
	}

	public void UpdatePhysics()
	{
		method_0();
	}

	public void method_0()
	{
		if (OnTouchForceMultiplier < 0f || Time.time < float_4)
		{
			return;
		}
		float_4 = Time.time + 0.2f;
		if (Physics.OverlapBoxNonAlloc(Collider.bounds.center, Collider.bounds.extents, collider_0, base.transform.rotation, LayerMaskClass.PlayerMask) > 0)
		{
			Vector3 position = collider_0[0].transform.position;
			Vector3 force = (base.transform.position - position).normalized * OnTouchForceMultiplier;
			float onTouchForceMultiplier = OnTouchForceMultiplier;
			method_1(force, position, onTouchForceMultiplier);
			if (!((position - vector3_0).sqrMagnitude < 0.5f))
			{
				vector3_0 = position;
				CollisionNumber++;
			}
		}
	}

	public void method_1(Vector3 force, Vector3 position, float forceMultiplier)
	{
		if (SolidObject != null)
		{
			SolidObject.SetActive(value: false);
		}
		bool flag = false;
		if (FragmentedObject != null && !FragmentedObject.activeInHierarchy)
		{
			FragmentedObject.SetActive(value: true);
			flag = true;
		}
		foreach (Rigidbody rigidbody in Rigidbodies)
		{
			if (rigidbody.IsSleeping() || flag)
			{
				if (rigidbody.isKinematic)
				{
					rigidbody.isKinematic = false;
				}
				rigidbody.AddForceAtPosition(force, position, ForceMode.Impulse);
				rigidbody.AddTorque(UnityEngine.Random.onUnitSphere * forceMultiplier, ForceMode.Impulse);
				EFTPhysicsClass.GClass745.SupportRigidbody(rigidbody, 0f);
				method_2(force);
			}
		}
		if (flag)
		{
			base.enabled = false;
		}
		if (TimeOfLife > 0f)
		{
			coroutine_0 = StaticManager.BeginCoroutine(method_3());
		}
	}

	public void method_2(Vector3 force)
	{
		if (!string.IsNullOrEmpty(ParticlesEffectName))
		{
			Singleton<Effects>.Instance.EmitGrenade(ParticlesEffectName, Collider.bounds.center, force, 0f);
		}
	}

	public IEnumerator method_3()
	{
		yield return new WaitForSeconds(TimeOfLife);
		foreach (Rigidbody rigidbody in Rigidbodies)
		{
			rigidbody.gameObject.SetActive(value: false);
		}
		coroutine_0 = null;
	}

	public void OnCollisionEnter(Collision collision)
	{
		float sqrMagnitude = collision.impulse.sqrMagnitude;
		if (!(sqrMagnitude > 0.5f) && !(sqrMagnitude < 0.2f) && !(Time.time < IgnoreCollisionTrackingTimer))
		{
			CollisionNumber++;
		}
	}

	public void OnCollisionHandler(bool resetSound = false)
	{
		Vector3 position = base.transform.position + vector3_1;
		if (soundBank_0 == null || resetSound)
		{
			method_4();
		}
		Singleton<BetterAudio>.Instance.PlayAtPoint(position, soundBank_0, CameraClass.Instance.Distance(position), 1f, -1f, EnvironmentType.Outdoor, EOcclusionTest.Fast);
		IgnoreCollisionTrackingTimer = Time.time + 0.5f;
	}

	public void method_4()
	{
		WeaponSounds weaponSounds = GClass1857.InstantiateAsset<WeaponSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/shells/weaponsounds.bundle");
		switch (SoundType)
		{
		default:
			throw new ArgumentOutOfRangeException();
		case EPhysicsObjectType.Books:
			soundBank_0 = weaponSounds.BooksImpactSoundBank;
			break;
		case EPhysicsObjectType.CardBoard:
			soundBank_0 = weaponSounds.CardBoardImpactSoundBank;
			break;
		case EPhysicsObjectType.CoolerBottle:
			soundBank_0 = weaponSounds.CoolerBottleImpactSoundBank;
			break;
		case EPhysicsObjectType.Cup:
			soundBank_0 = weaponSounds.CupImpactSoundBank;
			break;
		case EPhysicsObjectType.GlassBottle:
			soundBank_0 = weaponSounds.GlassBottleImpactSoundBank;
			break;
		case EPhysicsObjectType.HardPlastic:
			soundBank_0 = weaponSounds.HardPlasticImpactSoundBank;
			break;
		case EPhysicsObjectType.MediumGeneric:
			soundBank_0 = weaponSounds.MediumGenericImpactSoundBank;
			break;
		case EPhysicsObjectType.MetalCandlestick:
			soundBank_0 = weaponSounds.MetalCandlestickImpactSoundBank;
			break;
		case EPhysicsObjectType.Monitor:
			soundBank_0 = weaponSounds.MonitorImpactSoundBank;
			break;
		case EPhysicsObjectType.SmallBox:
			soundBank_0 = weaponSounds.SmallBoxImpactSoundBank;
			break;
		case EPhysicsObjectType.SmallPaper:
			soundBank_0 = weaponSounds.SmallPaperImpactSoundBank;
			break;
		case EPhysicsObjectType.SmallPlastic:
			soundBank_0 = weaponSounds.SmallPlasticImpactSoundBank;
			break;
		case EPhysicsObjectType.TincanEmpty:
			soundBank_0 = weaponSounds.TincanEmptyImpactSoundBank;
			break;
		}
	}

	public void method_5(Transform t)
	{
		InitialPositions.Add(new InitialPosition(t, t.GetComponent<Rigidbody>()));
		for (int i = 0; i < t.childCount; i++)
		{
			method_5(t.GetChild(i));
		}
	}

	public void method_6(DamageInfoStruct damageInfo)
	{
		if (!(OnHitForceMultiplier < 0f) && !(Time.time - float_5 < 0.3f))
		{
			float_5 = Time.time;
			method_1(damageInfo.Direction * OnHitForceMultiplier, damageInfo.HitPoint, OnHitForceMultiplier);
		}
	}

	public void OnBlowUp(Vector3 grenadePosition)
	{
		if (!(OnBlowUpForceMultiplier < 0f))
		{
			Vector3 position = base.transform.position;
			Vector3 vector = position - grenadePosition;
			if (!(vector.magnitude > BlowUpThreshold))
			{
				Vector3 force = vector.normalized * OnBlowUpForceMultiplier / vector.magnitude;
				method_1(force, position, OnBlowUpForceMultiplier);
			}
		}
	}

	public void ResetToDefaultPositions()
	{
		foreach (InitialPosition initialPosition in InitialPositions)
		{
			initialPosition.Reset();
			if (initialPosition.Rigidbody != null)
			{
				initialPosition.Rigidbody.gameObject.SetActive(value: true);
			}
		}
		if (SolidObject != null)
		{
			SolidObject.SetActive(value: true);
		}
		if (FragmentedObject != null)
		{
			FragmentedObject.SetActive(value: false);
		}
		if (coroutine_0 != null)
		{
			StaticManager.KillCoroutine(coroutine_0);
			coroutine_0 = null;
		}
		soundBank_0 = null;
		base.enabled = true;
	}

	public void method_7()
	{
		OnCollisionHandler(resetSound: true);
	}

	public void method_8()
	{
		Vector3 force = base.transform.position + Collider.bounds.center;
		method_2(force);
	}

	public void method_9()
	{
		OnBlowUp(base.transform.position + TestBlowPosition);
	}
}
