using System.Collections;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT;

public class Grenade : Throwable
{
	[CompilerGenerated]
	public class Class1390
	{
		public Item originalWeaponItem;

		public string playerProfileIDWhoThrew;

		public Vector3 grenadePosition;

		public bool isPlanted;

		public DamageInfoStruct method_0()
		{
			return smethod_0(originalWeaponItem, playerProfileIDWhoThrew, grenadePosition, isPlanted);
		}
	}

	public const float SHIFT_DISTANCE = 0.08f;

	private const int int_2 = 300;

	private static readonly Vector3 vector3_0 = new Vector3(0f, 0.08f, 0f);

	public ISharedBallisticsCalculator Calculator;

	[CompilerGenerated]
	private IPlayerOwner iPlayerOwner;

	[CompilerGenerated]
	private string string_0;

	[CompilerGenerated]
	private ThrowWeapItemClass throwWeapItemClass;

	public static GClass3725 GrenadeRandoms;

	protected GrenadeSettings _grenadeSettings;

	protected IEnumerator _behaviourTimerCoroutine;

	private bool bool_2;

	private float float_3;

	private float float_4;

	private Collider collider_0;

	private Transform transform_0;

	private static int int_3;

	private bool bool_3;

	private BetterSource betterSource_0;

	private SoundBank soundBank_0;

	private readonly Vector3 vector3_1 = new Vector3(0f, 0.1f, 0f);

	private readonly Vector3 vector3_2 = new Vector3(0f, 0.5f, 0f);

	private const float float_5 = 0.2f;

	private const float float_6 = 0.5f;

	public IPlayerOwner Player
	{
		[CompilerGenerated]
		get
		{
			return iPlayerOwner;
		}
		[CompilerGenerated]
		set
		{
			iPlayerOwner = value;
		}
	}

	public string ProfileId
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
		[CompilerGenerated]
		set
		{
			string_0 = value;
		}
	}

	public ThrowWeapItemClass WeaponSource
	{
		[CompilerGenerated]
		get
		{
			return throwWeapItemClass;
		}
		[CompilerGenerated]
		set
		{
			throwWeapItemClass = value;
		}
	}

	public GrenadeSettings GrenadeSettings => _grenadeSettings;

	public Vector3 Offset => _grenadeSettings.Offset;

	public override int Id
	{
		get
		{
			ThrowWeapItemClass weaponSource = WeaponSource;
			if (weaponSource == null)
			{
				return -1;
			}
			return GClass1298.GetStableHashCode(weaponSource.Id);
		}
	}

	public virtual float PhysicsQuality => 1f;

	public static float PhysicsQualityForObserved => 0f;

	public override bool HasNetData => false;

	public void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		collider_0 = GetComponent<Collider>();
		EFTPhysicsClass.GClass745.SupportRigidbody(Rigidbody, visibilityChecker: GetVisibilityChecker(), quality: PhysicsQuality);
	}

	public virtual GClass833 GetVisibilityChecker()
	{
		return null;
	}

	public void OnCollisionEnter(Collision collision)
	{
		float sqrMagnitude = collision.impulse.sqrMagnitude;
		if (sqrMagnitude > 0.5f && method_5(collision))
		{
			Physics.IgnoreCollision(collision.collider, collider_0);
			Rigidbody.velocity = Velocity;
		}
		else if (!(sqrMagnitude < 0.2f) && !(Time.time < IgnoreCollisionTrackingTimer))
		{
			base.CollisionNumber++;
			ProcessContactExplodeCollision(collision.impulse.magnitude);
		}
	}

	public void method_2()
	{
		if (!MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			Debug.LogWarning($"Grenade {GetType()} can't init audio source because BetterAudio not exist");
			return;
		}
		method_3(betterSource_0);
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		betterSource_0 = instance.GetSource(BetterAudio.AudioSourceGroupType.Collisions);
		betterSource_0.SetMixerGroup(instance.GunsInstrumentalMixer);
		betterSource_0.StartTrackingPosition(base.transform, vector3_1);
		if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(base.gameObject, betterSource_0, EOcclusionTest.ContinuousPropagated, 30000f, vector3_2);
		}
	}

	public void method_3(BetterSource source)
	{
		if (!(source == null) && MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			source.Stop();
			source.StopTrackingPosition();
			source.Release();
		}
	}

	public virtual void ProcessContactExplodeCollision(float impulse)
	{
		if (!(WeaponSource.MinTimeToContactExplode < 0f) && float_3 >= WeaponSource.MinTimeToContactExplode)
		{
			StopTimer();
			bool_2 = true;
		}
	}

	public override void OnCollisionHandler()
	{
		method_4();
		base.OnCollisionHandler();
	}

	public void method_4()
	{
		if (betterSource_0 == null)
		{
			method_2();
		}
		if (GClass2313.IsInRange(betterSource_0.Position, soundBank_0.Rolloff))
		{
			AudioClip clip = soundBank_0.PickSingleClip(0);
			betterSource_0.SetRolloff(soundBank_0.Rolloff);
			betterSource_0.Play(clip, null, 1f, soundBank_0.RandomVolume);
		}
	}

	public bool method_5(Collision collision)
	{
		WindowBreaker componentInParent = collision.gameObject.GetComponentInParent<WindowBreaker>();
		if (componentInParent == null)
		{
			return false;
		}
		BallisticCollider ballisticCollider = (componentInParent.IsDamaged ? collision.gameObject.GetComponent<BallisticCollider>() : componentInParent.GlassBallisticCollider);
		if (!(ballisticCollider == null) && ballisticCollider.TypeOfMaterial == MaterialType.Glass)
		{
			ballisticCollider.ApplyHit(new DamageInfoStruct
			{
				DamageType = EDamageType.Blunt,
				HitPoint = collision.contacts[0].point,
				Direction = Velocity
			}, ShotIdStruct.EMPTY_SHOT_ID);
			return true;
		}
		return false;
	}

	public static GClass833 GetVisibilityCheckerForObserved(Grenade grenade)
	{
		if (CameraClass.Instance.Camera != null)
		{
			return new GClass833(CameraClass.Instance.Camera, grenade.gameObject, BackendConfigAbstractClass.Config.Physics.CullingForGrenade);
		}
		return null;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (Calculator != null && Singleton<GInterface169>.Instantiated)
		{
			Singleton<GInterface169>.Instance.RemoveBallisticCalculator(WeaponSource);
		}
		method_3(betterSource_0);
	}

	public void SetRigidbodyMass(float mass)
	{
		Rigidbody.mass = mass;
	}

	public virtual void SetThrowForce(Vector3 force)
	{
		Velocity = force;
		Rigidbody.AddForce(force, ForceMode.Impulse);
	}

	public virtual void Init(GrenadeSettings settings, string profileId, ThrowWeapItemClass throwWeap, float timeSpent, ISharedBallisticsCalculator calculator, bool isBeingPlanted)
	{
		base.Init(settings);
		bool_3 = isBeingPlanted;
		_grenadeSettings = settings;
		ProfileId = profileId;
		Player = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(profileId);
		if (Player != null)
		{
			EFTPhysicsClass.IgnoreCollision(Player.CharacterController.GetCollider(), collider_0);
		}
		WeaponSource = throwWeap;
		Calculator = calculator;
		float_4 = timeSpent;
		WeaponSounds weaponSounds = GClass1857.InstantiateAsset<WeaponSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/shells/weaponsounds.bundle");
		switch (_grenadeSettings.CollisionSound)
		{
		case GrenadeSettings.CollisionSounds.frag:
			soundBank_0 = weaponSounds.GrenadeDropSoundBank;
			break;
		case GrenadeSettings.CollisionSounds.smoke:
			soundBank_0 = weaponSounds.SmokeGrenadeCollisions;
			break;
		case GrenadeSettings.CollisionSounds.stun:
			soundBank_0 = weaponSounds.StunGrenadeCollisions;
			break;
		case GrenadeSettings.CollisionSounds.smokeM18:
			soundBank_0 = weaponSounds.SmokeGrenadeM18Collisions;
			break;
		case GrenadeSettings.CollisionSounds.stunM7920:
			soundBank_0 = weaponSounds.StunGrenadeM7920Collisions;
			break;
		}
		method_2();
		if (!bool_3)
		{
			StartTimer();
		}
		float_3 = 0f;
	}

	public virtual void StartTimer()
	{
		_behaviourTimerCoroutine = GClass7.StartBehaviourTimer(this, WeaponSource.GetExplDelay - float_4, InvokeBlowUpEvent);
	}

	public void StopTimer()
	{
		if (_behaviourTimerCoroutine != null)
		{
			GClass7.StopBehaviourTimer(this, ref _behaviourTimerCoroutine);
		}
		_behaviourTimerCoroutine = null;
	}

	public void ExternalStartTimer()
	{
		if (!bool_3)
		{
			Debug.LogError("You can't call ExternalStartTimer on non-planted item");
		}
		else
		{
			StartTimer();
		}
	}

	public void ExternalInvokeBlowUpEvent()
	{
		if (!bool_3)
		{
			Debug.LogError("You can't call ExternalInvokeBlowUpEvent on non-planted item");
		}
		else
		{
			InvokeBlowUpEvent();
		}
	}

	public void InvokeBlowUpEvent()
	{
		method_6();
		OnExplosion();
	}

	public virtual void OnExplosion()
	{
		Object.DestroyImmediate(base.gameObject);
	}

	public void Attach(Transform t)
	{
		transform_0 = t;
	}

	public virtual void LateUpdate()
	{
		float_3 += Time.deltaTime;
		if (bool_2)
		{
			InvokeBlowUpEvent();
		}
		else if (transform_0 != null)
		{
			base.transform.rotation = transform_0.rotation;
			base.transform.position = transform_0.position + transform_0.rotation * _grenadeSettings.Offset;
		}
	}

	public void method_6()
	{
		if (!string.IsNullOrEmpty(WeaponSource.ExplosionEffectType))
		{
			Singleton<Effects>.Instance.EmitGrenade(WeaponSource.ExplosionEffectType, base.transform.position, Vector3.up);
		}
		Explosion(this, WeaponSource, base.transform.position, ProfileId, Calculator, WeaponSource, vector3_0, bool_3);
	}

	public static void Explosion(Grenade grenade, IExplosiveItem grenadeItem, Vector3 grenadePosition, string playerProfileIDWhoThrew, ISharedBallisticsCalculator grenadeBallisticsCalculator, Item originalWeaponItem, Vector3 shift, bool isPlanted = false)
	{
		if (Singleton<BotEventHandler>.Instantiated)
		{
			SmokeGrenade smokeGrenade = grenade as SmokeGrenade;
			int throwableId = -1;
			if (grenade != null)
			{
				throwableId = grenade.Id;
			}
			Singleton<BotEventHandler>.Instance.GrenadeExplosion(grenadePosition, playerProfileIDWhoThrew, smokeGrenade != null, smokeGrenade?.Radius ?? 0f, smokeGrenade?.LifeTime ?? 0f, throwableId);
			grenadePosition += shift;
			GClass2085.Explosion(grenadeItem, grenadePosition, playerProfileIDWhoThrew, grenadeBallisticsCalculator, originalWeaponItem, () => smethod_0(originalWeaponItem, playerProfileIDWhoThrew, grenadePosition, isPlanted), 0f, 0f, null, deadlyMinDistance: false);
		}
	}

	public static DamageInfoStruct smethod_0(Item originalItemWeapon, string playerWhoThrew, Vector3 explosionPosition, bool isPlanted)
	{
		EftBulletClass shot = EftBulletClass.Create(originalItemWeapon, 0, 0, explosionPosition, Vector3.zero, 0f, 0f, 0.5f, 0.002f, 0f, 0f, 0f, 0f, 0f, 1f, 0, 0, null, null, 1f, playerWhoThrew, originalItemWeapon, -1, null, isPlanted);
		DamageInfoStruct result = new DamageInfoStruct(EDamageType.GrenadeFragment, shot);
		EftBulletClass.Release(shot);
		return result;
	}
}
