using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.PrefabSettings;
using UnityEngine;

namespace EFT;

public class FlareCartridge : Throwable
{
	private const float float_3 = 3f;

	private const float float_4 = 10f;

	private readonly Vector3 vector3_0 = new Vector3(5f, 10f, 20f);

	private static PhysicMaterial physicMaterial_0;

	private FlareColorType flareColorType_0;

	private FlareCartridgeSettings flareCartridgeSettings_0;

	private GameObject gameObject_0;

	private IPlayer iplayer_0;

	private AmmoItemClass ammoItemClass;

	private Weapon weapon_0;

	private CapsuleCollider capsuleCollider_0;

	private SphereCollider sphereCollider_0;

	private bool bool_2;

	private bool bool_3;

	private bool bool_4;

	private float float_5;

	private Vector3 vector3_1;

	private GameObject gameObject_1;

	private float float_6;

	private float float_7;

	private bool bool_5;

	private bool bool_6;

	private const float float_8 = 0.2f;

	private const float float_9 = 0.5f;

	public static PhysicMaterial PhysicMaterial_0
	{
		get
		{
			if (physicMaterial_0 == null)
			{
				physicMaterial_0 = new PhysicMaterial("flare");
				physicMaterial_0.dynamicFriction = 0.4f;
				physicMaterial_0.staticFriction = 0.4f;
				physicMaterial_0.frictionCombine = PhysicMaterialCombine.Average;
				physicMaterial_0.bounciness = 0.1f;
				physicMaterial_0.bounceCombine = PhysicMaterialCombine.Average;
			}
			return physicMaterial_0;
		}
	}

	public override int Id => method_2();

	public override bool HasNetData => false;

	public void Init(FlareCartridgeSettings flareCartridgeSettings, IPlayer player, AmmoItemClass flareCartridge, Weapon weapon)
	{
		base.gameObject.layer = LayerMask.NameToLayer("Triggers");
		base.Init(flareCartridgeSettings);
		flareCartridgeSettings_0 = flareCartridgeSettings;
		float_6 = flareCartridgeSettings.SuccesfullHeight;
		flareColorType_0 = flareCartridgeSettings.FlareColorType;
		ammoItemClass = flareCartridge;
		weapon_0 = weapon;
		iplayer_0 = player;
		float_7 = base.transform.position.y;
		bool_2 = false;
		bool_4 = false;
		bool_5 = false;
		bool_3 = false;
		bool_6 = false;
		float_5 = Time.time;
		base.CollisionNumber = 0;
		method_10();
		method_7();
		Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		if (Rigidbody == null)
		{
			Rigidbody = base.gameObject.AddComponent<Rigidbody>();
		}
		EFTPhysicsClass.GClass745.SupportRigidbody(Rigidbody);
		Rigidbody.isKinematic = false;
		Rigidbody.mass = flareCartridgeSettings.Weight;
		Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		Rigidbody.drag = 0f;
		gameObject_1 = Object.Instantiate(flareCartridgeSettings.ProjectileAmmo, base.transform, worldPositionStays: false);
		gameObject_1.transform.forward = base.transform.forward;
		flareCartridgeSettings_0.SwitchMeshRenderersActive(value: false);
		if (flareCartridgeSettings.FlareEffectPrefab != null)
		{
			gameObject_0 = Object.Instantiate(flareCartridgeSettings.FlareEffectPrefab, flareCartridgeSettings.transform);
			FlareShotEffectSelector component = gameObject_0.GetComponent<FlareShotEffectSelector>();
			if (component != null)
			{
				component.SetFlareEffect(flareCartridgeSettings.FlareColorType, flareCartridgeSettings.FlareLifetime);
			}
			gameObject_0.SetActive(value: false);
		}
		base.enabled = true;
	}

	public int method_2()
	{
		if (!weapon_0.IsOneOff)
		{
			return GClass1298.GetStableHashCode(ammoItemClass.Id);
		}
		return GClass1298.GetStableHashCode(weapon_0.Id);
	}

	public bool method_3(Collider other, out bool contactWithOwnerPlayer)
	{
		contactWithOwnerPlayer = false;
		BodyPartCollider component = other.GetComponent<BodyPartCollider>();
		if (!(component == null) && component.playerBridge != null && component.playerBridge.iPlayer.HealthController.IsAlive)
		{
			if (component.playerBridge.iPlayer == iplayer_0)
			{
				contactWithOwnerPlayer = true;
				return true;
			}
			IPlayer iPlayer = component.playerBridge.iPlayer;
			RaycastHit[] array = new RaycastHit[1];
			int num = Physics.RaycastNonAlloc(new Ray(base.transform.position, base.transform.forward), array, 0.15f, LayerMaskClass.HitColliderMask);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					RaycastHit hit = array[i];
					BodyPartCollider component2 = array[i].collider.GetComponent<BodyPartCollider>();
					if (component2 != null && component2.playerBridge.iPlayer == iPlayer)
					{
						if (iPlayer != null)
						{
							IPlayer player = iPlayer;
							method_4(component2, player, hit);
						}
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public void method_4(BodyPartCollider bodyPart, IPlayer player, RaycastHit hit)
	{
		float damage = method_5(bodyPart, hit);
		DamageInfoStruct damageInfo = new DamageInfoStruct
		{
			DamageType = EDamageType.Bullet,
			Damage = damage,
			ArmorDamage = ammoItemClass.ArmorDamage,
			PenetrationPower = ammoItemClass.PenetrationPower,
			Direction = Velocity.normalized,
			HitCollider = bodyPart.Collider,
			HitNormal = hit.normal,
			HitPoint = hit.point,
			HittedBallisticCollider = bodyPart,
			Player = Singleton<GameWorld>.Instance.GetEverExistedBridgeByProfileID(iplayer_0.ProfileId),
			Weapon = weapon_0,
			LightBleedingDelta = -10f,
			HeavyBleedingDelta = -10f
		};
		IPlayerOwner alivePlayerBridgeByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(player.ProfileId);
		if (bodyPart.BodyPartType == EBodyPart.Head && bool_2)
		{
			Dictionary<IPlayerOwner, GStruct230> dictionary = new Dictionary<IPlayerOwner, GStruct230>();
			List<IPlayerOwner> players = new List<IPlayerOwner> { alivePlayerBridgeByProfileID };
			GStruct230 value = new GStruct230
			{
				Distance = Vector3.Distance(player.Position, base.transform.position),
				DirectionToEmitter = (base.transform.position - player.PlayerBones.Head.position).normalized,
				TryToApplyStun = false,
				TryToApplyBurnEyes = true,
				TryToApplyContusion = false
			};
			dictionary.Add(alivePlayerBridgeByProfileID, value);
			GClass2080.ApplyLightAndSoundHealthEffects(players, dictionary, base.transform.position, vector3_0);
		}
		ArmorPlateCollider armorPlateCollider = bodyPart as ArmorPlateCollider;
		EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
		alivePlayerBridgeByProfileID.ApplyShot(damageInfo, bodyPart.BodyPartType, bodyPart.BodyPartColliderType, armorPlateCollider2, ShotIdStruct.EMPTY_SHOT_ID);
	}

	public float method_5(BodyPartCollider bodyPart, RaycastHit hit)
	{
		if (bodyPart.IsHitToArmor(hit.point, hit.normal, base.transform.forward))
		{
			return 0f;
		}
		return Rigidbody.velocity.magnitude / flareCartridgeSettings_0.InitialSpeed * (float)ammoItemClass.Damage;
	}

	public bool method_6(Collision collision)
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

	public void Launch()
	{
		Vector3 velocity = (Velocity = flareCartridgeSettings_0.InitialSpeed * base.transform.forward);
		Rigidbody.velocity = velocity;
		vector3_1 = iplayer_0.Position;
	}

	public void method_7()
	{
	}

	public void Update()
	{
		method_7();
		if (!bool_2 && !bool_3 && Time.time > float_5 + flareCartridgeSettings_0.FlareTimeAfterStart)
		{
			method_8();
			bool_2 = true;
		}
		if (bool_2 && !bool_4 && base.CollisionNumber <= 0 && base.transform.position.y - float_7 > float_6)
		{
			bool_4 = true;
			iplayer_0.HandleFlareSuccessEvent(base.transform.position, ammoItemClass.AmmoTemplate);
			GlobalEventHandlerClass.Instance.CreateCommonEvent<GClass3553>().Invoke(iplayer_0, vector3_1, base.transform.position, ammoItemClass.AmmoTemplate);
		}
		if (bool_2 && Time.time + 3f > float_5 + flareCartridgeSettings_0.FlareLifetime)
		{
			bool_2 = false;
			bool_3 = true;
		}
		if (bool_3 && Time.time > float_5 + flareCartridgeSettings_0.FlareLifetime)
		{
			method_9();
		}
	}

	public void method_8()
	{
		Rigidbody.drag = flareCartridgeSettings_0.RigidbodyDrag;
		if (gameObject_1 != null)
		{
			gameObject_1.SetActive(value: false);
		}
		if (gameObject_0 != null)
		{
			gameObject_0.SetActive(value: true);
		}
	}

	public void method_9()
	{
		Object.DestroyImmediate(base.gameObject);
	}

	public void method_10()
	{
		capsuleCollider_0 = GetComponent<CapsuleCollider>();
		if (capsuleCollider_0 == null)
		{
			capsuleCollider_0 = base.gameObject.AddComponent<CapsuleCollider>();
		}
		capsuleCollider_0.radius = 0.01f;
		capsuleCollider_0.center = new Vector3(0f, 0f, -0.005f);
		capsuleCollider_0.height = 0.05f;
		capsuleCollider_0.direction = 2;
		capsuleCollider_0.sharedMaterial = PhysicMaterial_0;
		if (iplayer_0 != null)
		{
			EFTPhysicsClass.IgnoreCollision(iplayer_0.CharacterController.GetCollider(), capsuleCollider_0);
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.CollisionNumber > 0 || bool_3)
		{
			return;
		}
		if (method_3(collision.collider, out var contactWithOwnerPlayer))
		{
			if (!contactWithOwnerPlayer)
			{
				base.CollisionNumber++;
			}
		}
		else if (collision.impulse.sqrMagnitude > 0.5f && method_6(collision))
		{
			Physics.IgnoreCollision(collision.collider, capsuleCollider_0);
			Rigidbody.velocity = Velocity;
			base.CollisionNumber++;
		}
		else if (!(Time.time < IgnoreCollisionTrackingTimer))
		{
			base.CollisionNumber++;
		}
	}

	public override void OnCollisionHandler()
	{
		if (!bool_6 && !(Rigidbody == null))
		{
			Rigidbody.drag = flareCartridgeSettings_0.CollisionDrag;
			bool_6 = true;
		}
	}
}
