using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using UnityEngine;

public class BodyPartCollider : BallisticCollider
{
	public interface IPlayerBridge
	{
		IPlayer iPlayer { get; }

		float WorldTime { get; }

		bool UsingSimplifiedSkeleton { get; }

		ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPart, EBodyPartColliderType bodyPartCollider, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId);

		void ApplyDamageInfo(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType bodyPartCollider, float absorbed);

		bool TryGetArmorResistData(BodyPartCollider bodyPart, float penetrationPower, out ArmorResistanceStruct armorResistanceData);

		bool SetShotStatus(BodyPartCollider bodypart, EftBulletClass shot, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection);

		bool CheckArmorHitByDirection(BodyPartCollider bodypart, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection);

		bool IsShotDeflectedByHeavyArmor(EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, int shotSeed);
	}

	public class PlayerBridge : IPlayerBridge
	{
		[NonSerialized]
		public Player Player_0;

		public IPlayer iPlayer => Player_0;

		public float WorldTime => Singleton<AbstractGame>.Instance.LastServerTimeStamp;

		public bool UsingSimplifiedSkeleton => Player_0.UsedSimplifiedSkeleton;

		public ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPart, EBodyPartColliderType bodyPartCollider, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId)
		{
			return Player_0.ApplyShot(damageInfo, bodyPart, bodyPartCollider, armorPlateCollider, shotId);
		}

		public void ApplyDamageInfo(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType bodyPartCollider, float absorbed)
		{
			Player_0.ApplyDamageInfo(damageInfo, bodyPartType, bodyPartCollider, absorbed);
		}

		public bool TryGetArmorResistData(BodyPartCollider bodyPart, float penetrationPower, out ArmorResistanceStruct armorResistanceData)
		{
			return Player_0.TryGetArmorResistData(bodyPart, penetrationPower, out armorResistanceData);
		}

		public bool SetShotStatus(BodyPartCollider bodypart, EftBulletClass shot, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection)
		{
			return Player_0.SetShotStatus(bodypart, shot, hitpoint, shotNormal, shotDirection);
		}

		public bool CheckArmorHitByDirection(BodyPartCollider bodypart, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection)
		{
			return Player_0.CheckArmorHitByDirection(bodypart);
		}

		public bool IsShotDeflectedByHeavyArmor(EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, int shotSeed)
		{
			return Player_0.IsShotDeflectedByHeavyArmor(colliderType, armorPlateCollider, shotSeed);
		}

		public PlayerBridge(Player player)
		{
			Player_0 = player;
		}
	}

	public class ObserverBridge : IPlayerBridge
	{
		[NonSerialized]
		public ObservedPlayerView ObservedPlayerView_0;

		[NonSerialized]
		public GClass3727 Gclass3727_0 = new GClass3727(512, 0);

		public IPlayer iPlayer => ObservedPlayerView_0;

		public float WorldTime => ObservedPlayerView_0.WorldTime;

		public bool UsingSimplifiedSkeleton => ObservedPlayerView_0.UsedSimplifiedSkeleton;

		public IEnumerable<GClass2903> IEnumerable_0 => ObservedPlayerView_0.ObservedPlayerController.ArmorInfoController.ObservedPlayerArmors.Values;

		public void ApplyDamageInfo(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType bodyPartColliderType, float absorbed)
		{
		}

		public bool TryGetArmorResistData(BodyPartCollider bodyPart, float penetrationPower, out ArmorResistanceStruct armorResistanceData)
		{
			armorResistanceData = default(ArmorResistanceStruct);
			ArmorPlateCollider armorPlateCollider = bodyPart as ArmorPlateCollider;
			EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
			foreach (GClass2903 item in IEnumerable_0)
			{
				if (item.ShotMatches(bodyPart.BodyPartColliderType, armorPlateCollider2))
				{
					armorResistanceData = GClass659.RealResistance(item.durability, item.templateDurability, item.armorClass, penetrationPower);
					return true;
				}
			}
			return false;
		}

		public bool CheckArmorHitByDirection(BodyPartCollider bodypart, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection)
		{
			return false;
		}

		public ShotInfoClass ApplyShot(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, ShotIdStruct shotId)
		{
			if (!ObservedPlayerView_0.HealthController.IsAlive)
			{
				return null;
			}
			bool hasValue = damageInfo.DeflectedBy.HasValue;
			MaterialType material = MaterialType.Body;
			MongoID? hitArmorID = null;
			if (hasValue)
			{
				material = MaterialType.HelmetRicochet;
			}
			else
			{
				foreach (GClass2903 item in IEnumerable_0)
				{
					if (item.ShotMatches(colliderType, armorPlateCollider))
					{
						material = item.material;
						hitArmorID = item.itemID;
						break;
					}
				}
			}
			ShotInfoClass result = new ShotInfoClass
			{
				PoV = EPointOfView.ThirdPerson,
				Penetrated = damageInfo.Penetrated,
				Material = material,
				HitArmorID = hitArmorID
			};
			ObservedPlayerView_0.ReactionOnShot(damageInfo, bodyPartType);
			return result;
		}

		public bool SetShotStatus(BodyPartCollider bodypart, EftBulletClass shot, Vector3 hitpoint, Vector3 shotNormal, Vector3 shotDirection)
		{
			ArmorPlateCollider armorPlateCollider = bodypart as ArmorPlateCollider;
			EArmorPlateCollider armorPlateCollider2 = ((!(armorPlateCollider == null)) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0));
			foreach (GClass2903 item in IEnumerable_0)
			{
				if (!item.ShotMatches(bodypart.BodyPartColliderType, armorPlateCollider2))
				{
					continue;
				}
				if (item.ricochetValues.x > 0f)
				{
					float num = Vector3.Angle(-shotDirection, shotNormal);
					if (num > item.ricochetValues.z)
					{
						float t = Mathf.InverseLerp(90f, item.ricochetValues.z, num);
						float num2 = Mathf.Lerp(item.ricochetValues.x, item.ricochetValues.y, t);
						if (!(shot.Randoms.GetRandomFloat(shot.RandomSeed) >= num2))
						{
							shot.DeflectedBy = item.itemID;
							return true;
						}
					}
				}
				if (!shot.BlockedBy.HasValue && item.durability > 0f)
				{
					float penetrationPower = shot.PenetrationPower;
					float penetrationChance = GClass659.RealResistance(item.durability, item.templateDurability, item.armorClass, penetrationPower).GetPenetrationChance(penetrationPower);
					if (shot.Randoms.GetRandomFloat(shot.RandomSeed) * 100f > penetrationChance)
					{
						shot.BlockedBy = item.itemID;
					}
				}
			}
			return false;
		}

		public bool IsShotDeflectedByHeavyArmor(EBodyPartColliderType colliderType, EArmorPlateCollider armorPlateCollider, int shotSeed)
		{
			if (!ObservedPlayerView_0.ObservedPlayerController.InfoContainer.HeavyVestNoBodyDamageDeflectChance)
			{
				return false;
			}
			BackendConfigSettingsClass.GClass1790 heavyVests = Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.HeavyVests;
			foreach (GClass2903 item in IEnumerable_0)
			{
				if (item.armorType == EArmorType.Heavy && !(item.durability < heavyVests.RicochetChanceHVestsCurrentDurabilityThreshold * item.maxDurability) && !(item.durability < heavyVests.RicochetChanceHVestsMaxDurabilityThreshold * (float)item.templateDurability) && item.ShotMatches(colliderType, armorPlateCollider) && Gclass3727_0.GetRandom(shotSeed) < heavyVests.RicochetChanceHVestsEliteLevel)
				{
					return true;
				}
			}
			return false;
		}

		public ObserverBridge(ObservedPlayerView player)
		{
			ObservedPlayerView_0 = player;
		}
	}

	private const float float_0 = 0.5f;

	private const float float_1 = 2f;

	private const float float_2 = 1f;

	private const float float_3 = 7.5f;

	private const float float_4 = 0.5f;

	private const float float_5 = 1f;

	private const float float_6 = 9999f;

	[Range(0f, 1f)]
	public float penetrationDamageMod;

	private float float_7;

	private float float_8;

	private float float_9;

	private Vector3 vector3_0;

	public EBodyPart BodyPartType;

	public EBodyPartColliderType BodyPartColliderType;

	public Collider Collider;

	public IPlayerBridge playerBridge;

	public string PlayerProfileID;

	public Transform ColliderTransformCached;

	public Transform TransformCached;

	private Func<Vector3> func_0;

	public IPlayer Player => playerBridge?.iPlayer;

	public Vector3 Center => Collider.bounds.center;

	public Vector3 RealCenter => Collider.transform.position + Collider.transform.rotation * func_0();

	public static Bounds smethod_0(Collider collider)
	{
		BoxCollider boxCollider = collider as BoxCollider;
		if (boxCollider != null)
		{
			return new Bounds(Vector3.zero, boxCollider.size);
		}
		SphereCollider sphereCollider = collider as SphereCollider;
		if (sphereCollider != null)
		{
			return new Bounds(Vector3.zero, Vector3.one * sphereCollider.radius * 2f);
		}
		CapsuleCollider capsuleCollider = collider as CapsuleCollider;
		if (capsuleCollider != null)
		{
			Vector3 size = Vector3.one * capsuleCollider.radius * 2f;
			size[capsuleCollider.direction] = capsuleCollider.height;
			return new Bounds(Vector3.zero, size);
		}
		Debug.LogError("Unknown collier " + collider, collider);
		return collider.bounds;
	}

	public override void Awake()
	{
		base.TypeOfMaterial = MaterialType.Body;
		TransformCached = base.transform;
		if (Collider != null)
		{
			ColliderTransformCached = Collider.transform;
		}
		base.Awake();
	}

	public void SetUpPlayer(IPlayer iPlayer)
	{
		InitColliderSettings();
		if (!(iPlayer is Player player))
		{
			if (iPlayer is ObservedPlayerView player2)
			{
				playerBridge = new ObserverBridge(player2);
			}
		}
		else
		{
			playerBridge = new PlayerBridge(player);
		}
	}

	public virtual void InitColliderSettings()
	{
		if (Singleton<BackendConfigSettingsClass>.Instance.BodyPartColliderSettings.TryGetValue(BodyPartColliderType, out var value))
		{
			PenetrationLevel = value.PenetrationLevel;
			PenetrationChance = value.PenetrationChance;
			penetrationDamageMod = value.PenetrationDamageMod;
		}
	}

	public override ShotInfoClass ApplyHit(DamageInfoStruct damageInfo, ShotIdStruct shotID)
	{
		if (playerBridge != null && damageInfo.IsForwardHit)
		{
			return playerBridge.ApplyShot(damageInfo, BodyPartType, BodyPartColliderType, (EArmorPlateCollider)0, shotID);
		}
		return null;
	}

	public void ApplyEnvironmentalDamage(DamageInfoStruct damageInfo)
	{
		if (playerBridge != null)
		{
			playerBridge.ApplyDamageInfo(damageInfo, BodyPartType, BodyPartColliderType, 0f);
		}
	}

	public void ApplyInstantKill(DamageInfoStruct damageInfo)
	{
		if (playerBridge != null)
		{
			playerBridge.ApplyDamageInfo(damageInfo, BodyPartType, BodyPartColliderType, 0f);
		}
	}

	public bool ProceedBarb()
	{
		bool result = false;
		float num = Vector3.Distance(vector3_0, base.transform.position);
		if (num < 0.5f)
		{
			float_7 += num * 2f;
			if (float_7 >= 1f)
			{
				DamageInfoStruct damageInfo = new DamageInfoStruct
				{
					DamageType = EDamageType.Barbed,
					Damage = float_7,
					Direction = Vector3.zero,
					HitCollider = Collider,
					HitNormal = Vector3.zero,
					HitPoint = Vector3.zero,
					HittedBallisticCollider = this,
					Player = null
				};
				float_7 %= 1f;
				ApplyEnvironmentalDamage(damageInfo);
				result = true;
			}
		}
		else
		{
			float_7 = 0f;
		}
		vector3_0 = base.transform.position;
		return result;
	}

	public void ProceedFlame()
	{
		float_8 -= Time.deltaTime;
		if (float_8 <= 0f)
		{
			DamageInfoStruct damageInfo = new DamageInfoStruct
			{
				DamageType = EDamageType.Flame,
				Damage = 7.5f,
				Direction = Vector3.zero,
				HitCollider = Collider,
				HitNormal = Vector3.zero,
				HitPoint = Vector3.zero,
				HittedBallisticCollider = this,
				Player = null
			};
			ApplyEnvironmentalDamage(damageInfo);
			float_8 = 0.5f;
		}
	}

	public void ProceedPlatformImpact(float damage)
	{
		if (!(float_9 > Time.time))
		{
			DamageInfoStruct damageInfo = new DamageInfoStruct
			{
				DamageType = EDamageType.Impact,
				Damage = damage,
				Direction = Vector3.zero,
				HitCollider = Collider,
				HitNormal = Vector3.zero,
				HitPoint = Vector3.zero,
				HittedBallisticCollider = this,
				Player = null
			};
			ApplyEnvironmentalDamage(damageInfo);
			float_9 = Time.time + 1f;
		}
	}

	public void ProceedInstantKill()
	{
		DamageInfoStruct damageInfo = new DamageInfoStruct
		{
			Damage = 9999f,
			Direction = Vector3.zero,
			HitCollider = Collider,
			HitNormal = Vector3.zero,
			HitPoint = Vector3.zero,
			DamageType = EDamageType.Undefined,
			HittedBallisticCollider = this,
			Player = null
		};
		ApplyInstantKill(damageInfo);
	}

	public override bool IsPenetrated(EftBulletClass shot, Vector3 hitPoint)
	{
		float num = shot.PenetrationPower;
		if (playerBridge != null && playerBridge.TryGetArmorResistData(this, shot.PenetrationPower, out var armorResistanceData))
		{
			num *= armorResistanceData.CF;
		}
		if (!shot.BlockedBy.HasValue)
		{
			return num > PenetrationLevel;
		}
		return false;
	}

	public override void TakeSettingsFrom(BaseBallistic collider)
	{
		base.TakeSettingsFrom(collider);
		if (collider is BodyPartCollider bodyPartCollider)
		{
			penetrationDamageMod = bodyPartCollider.penetrationDamageMod;
		}
	}

	public override bool Deflects(float _hitCosDirectionToNormal, EftBulletClass shot, Vector3 hitPoint, Vector3 shotNormal, Vector3 shotDirection)
	{
		if (playerBridge != null)
		{
			return playerBridge.SetShotStatus(this, shot, hitPoint, shotNormal, shotDirection);
		}
		return base.Deflects(_hitCosDirectionToNormal, shot, hitPoint, shotNormal, shotDirection);
	}

	public bool IsHitToArmor(Vector3 hitPoint, Vector3 shotNormal, Vector3 shotDirection)
	{
		if (playerBridge == null)
		{
			return false;
		}
		return playerBridge.CheckArmorHitByDirection(this, hitPoint, shotNormal, shotDirection);
	}

	public Vector3 GetRandomPointToCastLocal(Vector3 lookFromPoint)
	{
		Vector3 zero = Vector3.zero;
		if (!(Collider is SphereCollider sphereCollider))
		{
			throw new NotImplementedException();
		}
		zero += sphereCollider.center;
		Vector3 direction = lookFromPoint - TransformCached.position;
		Vector3 forward = TransformCached.InverseTransformDirection(direction);
		Vector2 vector = UnityEngine.Random.insideUnitCircle * sphereCollider.radius;
		Vector3 vector2 = Quaternion.LookRotation(forward) * new Vector3(vector.x, vector.y, 0f);
		return zero + vector2;
	}
}
