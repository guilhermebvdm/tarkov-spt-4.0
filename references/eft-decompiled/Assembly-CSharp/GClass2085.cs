using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass2085
{
	[CompilerGenerated]
	public class Class1387
	{
		public Vector3 grenadePosition;

		public LayerMask limbsMask;
	}

	[CompilerGenerated]
	public class Class1388
	{
		public Vector3 hitPosition;

		public RaycastHit hit;

		public float distance;

		public Vector3 forwardHitPoint;

		public Class1387 class1387_0;

		public float method_0()
		{
			Physics.Raycast(hitPosition, class1387_0.grenadePosition - hitPosition, out hit, distance, class1387_0.limbsMask, QueryTriggerInteraction.UseGlobal);
			return Vector3.Distance(forwardHitPoint, hit.point);
		}
	}

	[CompilerGenerated]
	private static Action<Vector3, float> action_0;

	[NonSerialized]
	public const int Int_0 = 10;

	[NonSerialized]
	public const int Int_1 = 30;

	[NonSerialized]
	public const int Int_2 = 8;

	[NonSerialized]
	public static int Int_3;

	[NonSerialized]
	public static Dictionary<ExplosiveHitArmorColliderStruct, float> Dictionary_0 = new Dictionary<ExplosiveHitArmorColliderStruct, float>(50);

	[NonSerialized]
	public static Collider[] Collider_0 = new Collider[512];

	[NonSerialized]
	public static List<BodyPartCollider> List_0 = new List<BodyPartCollider>();

	[NonSerialized]
	public static HashSet<string> HashSet_0 = new HashSet<string>();

	[NonSerialized]
	public static RaycastHit[] RaycastHit_0 = new RaycastHit[32];

	[NonSerialized]
	public static Dictionary<EBodyPart, (BodyPartCollider limb, DamageInfoStruct damageInfo)> Dictionary_1 = new Dictionary<EBodyPart, (BodyPartCollider, DamageInfoStruct)>();

	[NonSerialized]
	public static Dictionary<IPlayerOwner, GStruct230> Dictionary_2 = new Dictionary<IPlayerOwner, GStruct230>(8);

	public static event Action<Vector3, float> OnExplosion
	{
		[CompilerGenerated]
		add
		{
			Action<Vector3, float> action = action_0;
			Action<Vector3, float> action2;
			do
			{
				action2 = action;
				Action<Vector3, float> value2 = (Action<Vector3, float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Vector3, float> action = action_0;
			Action<Vector3, float> action2;
			do
			{
				action2 = action;
				Action<Vector3, float> value2 = (Action<Vector3, float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static void smethod_0(IReadOnlyDictionary<IPlayerOwner, GStruct230> playerAndDistances, IExplosiveItem grenadeItem, Vector3 grenadePosition, Func<DamageInfoStruct> getDamageInfo, float directionalDamageMultiplier, float directionalDamageAngle, Vector3? explosionDirection, bool deadlyMinDistance)
	{
		HashSet_0.Clear();
		Dictionary_0.Clear();
		Vector3 vector = new Vector3(grenadePosition.x, 0f, grenadePosition.z);
		LayerMask limbsMask = LayerMaskClass.HighPolyWithTerrainMask;
		Func<float> func = null;
		AmmoItemClass ammoItemClass = grenadeItem.CreateFragment();
		DamageInfoStruct damageInfoStruct = getDamageInfo();
		foreach (KeyValuePair<IPlayerOwner, GStruct230> playerAndDistance in playerAndDistances)
		{
			playerAndDistance.Deconstruct(out var key, out var value);
			IPlayerOwner playerOwner = key;
			GStruct230 gStruct = value;
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(playerOwner.iPlayer.ProfileId);
			bool flag = false;
			bool flag2 = false;
			float num = Mathf.InverseLerp(grenadeItem.MaxExplosionDistance * 3f, grenadeItem.MinExplosionDistance / 2f, gStruct.Distance);
			if (playerOwner.iPlayer.IsYourPlayer && num > 0f && alivePlayerByProfileID != null)
			{
				alivePlayerByProfileID.ProceduralWeaponAnimation.StartCoroutine(alivePlayerByProfileID.ProceduralWeaponAnimation.ForceReact.GrenadeShake_CO(num));
			}
			if (gStruct.Distance > grenadeItem.MaxExplosionDistance && gStruct.Distance > grenadeItem.Contusion.y)
			{
				continue;
			}
			Dictionary_1.Clear();
			List_0.Clear();
			List_0.AddRange(playerOwner.iPlayer.PlayerBones.BodyPartColliders);
			ArmorPlateCollider[] armorPlateColliders = playerOwner.iPlayer.PlayerBones.ArmorPlateColliders;
			foreach (ArmorPlateCollider armorPlateCollider in armorPlateColliders)
			{
				if (armorPlateCollider.gameObject.activeSelf)
				{
					List_0.Add(armorPlateCollider);
				}
			}
			foreach (BodyPartCollider item in List_0)
			{
				Vector3 hitPosition = item.transform.position;
				float distance = Vector3.Distance(hitPosition, grenadePosition);
				RaycastHit hit = default(RaycastHit);
				bool flag3 = false;
				int num2 = Physics.RaycastNonAlloc(grenadePosition, hitPosition - grenadePosition, RaycastHit_0, distance, limbsMask, QueryTriggerInteraction.UseGlobal);
				float num3 = 0f;
				for (int j = 0; j < num2; j++)
				{
					if (ammoItemClass == null)
					{
						break;
					}
					if (smethod_2(RaycastHit_0[j], out var ballisticCollider))
					{
						if ((ballisticCollider.PenetrationChance < Mathf.Epsilon) | ((float)ammoItemClass.PenetrationPower < ballisticCollider.PenetrationLevel))
						{
							flag3 = true;
							hit = RaycastHit_0[j];
							break;
						}
						if (!(ballisticCollider.PenetrationLevel < Mathf.Epsilon))
						{
							num3 += ballisticCollider.PenetrationLevel + 2f;
						}
					}
				}
				float value2 = (float)ammoItemClass.PenetrationPower - num3;
				value2 = Mathf.Clamp(value2, 0f, ammoItemClass.PenetrationPower);
				float num4 = smethod_1(Mathf.InverseLerp(0f, ammoItemClass.PenetrationPower, value2));
				if (flag3)
				{
					if (item.BodyPartType == EBodyPart.Chest)
					{
						Vector3 forwardHitPoint = hit.point;
						func = delegate
						{
							Physics.Raycast(hitPosition, grenadePosition - hitPosition, out hit, distance, limbsMask, QueryTriggerInteraction.UseGlobal);
							return Vector3.Distance(forwardHitPoint, hit.point);
						};
					}
					continue;
				}
				flag2 = true;
				if (!(grenadeItem.GetStrength > 0f))
				{
					continue;
				}
				float num5 = (1f - GClass2298.InverseLerp(grenadeItem.ArmorDistanceDistanceDamage, distance)) * grenadeItem.ArmorDistanceDistanceDamage.z;
				flag = flag || num5 > 0f;
				if (num5 > 0f)
				{
					EArmorPlateCollider armorPlateCollider2 = (EArmorPlateCollider)0;
					if (item is ArmorPlateCollider armorPlateCollider3)
					{
						armorPlateCollider2 = armorPlateCollider3.ArmorPlateColliderType;
					}
					ExplosiveHitArmorColliderStruct key2 = new ExplosiveHitArmorColliderStruct(item.BodyPartColliderType, armorPlateCollider2);
					Dictionary_0[key2] = num5;
				}
				Vector3 vector2 = new Vector3(playerOwner.iPlayer.Transform.position.x, 0f, playerOwner.iPlayer.Transform.position.z);
				damageInfoStruct.HitPoint = hitPosition;
				damageInfoStruct.HitNormal = vector - vector2;
				if (!deadlyMinDistance)
				{
					damageInfoStruct.Damage = Mathf.InverseLerp(grenadeItem.MaxExplosionDistance, grenadeItem.MinExplosionDistance, distance) * grenadeItem.GetStrength;
				}
				else
				{
					damageInfoStruct.Damage = 100f;
				}
				if (explosionDirection.HasValue && directionalDamageMultiplier > Mathf.Epsilon && directionalDamageAngle > Mathf.Epsilon)
				{
					Vector3 vector3 = GClass1675.WithY(-gStruct.DirectionToEmitter, 0f);
					Vector3 to = GClass1675.WithY(explosionDirection.Value, 0f);
					if (Vector3.Angle(vector3, to) < directionalDamageAngle)
					{
						damageInfoStruct.Damage *= directionalDamageMultiplier;
					}
				}
				damageInfoStruct.HitCollider = item.Collider;
				damageInfoStruct.HittedBallisticCollider = item;
				damageInfoStruct.Damage *= num4;
				damageInfoStruct.ArmorDamage *= num4;
				if (!Dictionary_1.TryGetValue(item.BodyPartType, out (BodyPartCollider, DamageInfoStruct) value3) || value3.Item2.Damage < damageInfoStruct.Damage)
				{
					Dictionary_1[item.BodyPartType] = (item, damageInfoStruct);
				}
				HashSet_0.Add(playerOwner.iPlayer.ProfileId);
			}
			foreach (var (bodyPartCollider, damageInfo) in Dictionary_1.Values)
			{
				bodyPartCollider.ApplyEnvironmentalDamage(damageInfo);
			}
			if (flag && alivePlayerByProfileID != null)
			{
				alivePlayerByProfileID.ApplyExplosionDamageToArmor(Dictionary_0, damageInfoStruct);
			}
			if (flag2 || func != null)
			{
				float num6 = Mathf.InverseLerp(grenadeItem.Contusion.y, grenadeItem.Contusion.x, gStruct.Distance) * grenadeItem.Contusion.z;
				if (num6 > 2f)
				{
					float num7 = 1f;
					if (!flag2)
					{
						float t = func();
						num7 = GClass2298.InverseLerp(Singleton<BackendConfigSettingsClass>.Instance.WallContusionAbsorption, t);
					}
					num6 *= num7;
					if (num6 > 2f && (playerOwner.AIData == null || !playerOwner.AIData.IsAI) && alivePlayerByProfileID != null)
					{
						alivePlayerByProfileID.ActiveHealthController?.DoContusion(num6, 1f);
					}
				}
			}
			Dictionary_0.Clear();
		}
		Dictionary_2.Clear();
		HashSet_0.Clear();
	}

	public static float smethod_1(float p)
	{
		return 1f - Mathf.Sqrt(1f - p * p);
	}

	public static bool smethod_2(RaycastHit hit, out BallisticCollider ballisticCollider)
	{
		ballisticCollider = null;
		if (hit.collider == null)
		{
			return false;
		}
		if (hit.collider.TryGetComponent<BallisticCollider>(out ballisticCollider))
		{
			return true;
		}
		if (hit.collider.transform.parent == null)
		{
			return false;
		}
		if (hit.collider.transform.parent.TryGetComponent<BallisticCollider>(out ballisticCollider))
		{
			return true;
		}
		return false;
	}

	public static void Explosion(this IExplosiveItem grenadeItem, Vector3 explosionPosition, [CanBeNull] string playerProfileIDWhoThrew, ISharedBallisticsCalculator ballisticsCalculator, [CanBeNull] Item originalWeaponItem, Func<DamageInfoStruct> getDamageInfo, float directionalDamageMultiplier, float directionalDamageAngle, [CanBeNull] Vector3? explosionDirection, bool deadlyMinDistance)
	{
		if (grenadeItem.IsDummy)
		{
			return;
		}
		Dictionary_2.Clear();
		float radius = Mathf.Max(grenadeItem.MaxExplosionDistance * 2f, grenadeItem.Blindness.y, grenadeItem.Contusion.y);
		int num = Physics.OverlapSphereNonAlloc(explosionPosition, radius, Collider_0, LayerMaskClass.GrenadeAffectedMask);
		GameWorld instance = Singleton<GameWorld>.Instance;
		GStruct230 value;
		for (int i = 0; i < num; i++)
		{
			Collider collider = Collider_0[i];
			IPlayerOwner alivePlayerBridgeByCollider = instance.GetAlivePlayerBridgeByCollider(collider);
			if (alivePlayerBridgeByCollider != null && alivePlayerBridgeByCollider.IsAI && alivePlayerBridgeByCollider.AIData?.BotOwner?.Settings?.FileSettings?.Mind != null && alivePlayerBridgeByCollider.AIData.BotOwner.Settings.FileSettings.Mind.GRENADE_DAMAGE_IGNORE)
			{
				continue;
			}
			if (alivePlayerBridgeByCollider != null)
			{
				Dictionary<IPlayerOwner, GStruct230> dictionary_ = Dictionary_2;
				value = new GStruct230
				{
					Distance = Vector3.Distance(alivePlayerBridgeByCollider.iPlayer.Position, explosionPosition),
					DirectionToEmitter = (explosionPosition - alivePlayerBridgeByCollider.iPlayer.PlayerBones.Head.position).normalized,
					TryToApplyStun = true,
					TryToApplyBurnEyes = true
				};
				dictionary_.Add(alivePlayerBridgeByCollider, value);
			}
			else
			{
				if (grenadeItem.MaxExplosionDistance <= 0f)
				{
					continue;
				}
				LampController component = collider.gameObject.GetComponent<LampController>();
				if (component != null)
				{
					component.TryToBlowUp(grenadeItem, in explosionPosition);
					continue;
				}
				Transform parent = collider.transform.parent;
				WindowBreaker windowBreaker = ((parent != null) ? parent.GetComponent<WindowBreaker>() : null);
				if (windowBreaker != null)
				{
					windowBreaker.TryToBlowUp(grenadeItem, in explosionPosition);
					continue;
				}
				Transform root = collider.transform.root;
				EventObject eventObject = ((root != null) ? root.GetComponent<EventObject>() : null);
				if (eventObject != null)
				{
					eventObject.ExplosionCheck(grenadeItem, in explosionPosition);
				}
			}
		}
		int num2 = 0;
		if (Dictionary_2.Any())
		{
			try
			{
				GClass2080.ApplyLightAndSoundHealthEffects(Dictionary_2.Keys.ToList(), Dictionary_2, explosionPosition, grenadeItem.Blindness);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (grenadeItem.FragmentsCount > 0)
			{
				AmmoItemClass ammo = grenadeItem.CreateFragment();
				foreach (KeyValuePair<IPlayerOwner, GStruct230> item in Dictionary_2)
				{
					item.Deconstruct(out var key, out value);
					IPlayerOwner playerOwner = key;
					GStruct230 gStruct = value;
					float num3 = Mathf.Clamp((float)grenadeItem.FragmentsCount / (MathF.PI * 4f * gStruct.Distance * gStruct.Distance) * 2f, 0f, 3f);
					while (num3 > 0.3f)
					{
						float num4 = Mathf.Clamp(num3, 0.3f, 2f);
						num3 -= num4;
						Int_3++;
						num2++;
						Vector3 vector = playerOwner.iPlayer.Position + Vector3.up * 0.8f + 0.5f * Grenade.GrenadeRandoms.GetRandomDirection(Int_3) / num4 - explosionPosition;
						ballisticsCalculator.Shoot(ammo, explosionPosition, vector.normalized, 1, playerProfileIDWhoThrew, originalWeaponItem, 1f, 0);
					}
				}
			}
			if (grenadeItem.GetStrength > 0f || grenadeItem.Contusion.z > 0f)
			{
				smethod_0(Dictionary_2, grenadeItem, explosionPosition, getDamageInfo, directionalDamageMultiplier, directionalDamageAngle, explosionDirection, deadlyMinDistance);
			}
		}
		Array.Clear(Collider_0, 0, num);
		if (grenadeItem.FragmentsCount > 0)
		{
			bool skyExpl;
			RaycastHit hitInfo;
			bool groundExpl = !(skyExpl = !Physics.Raycast(explosionPosition, Vector3.down, out hitInfo, 4f, LayerMaskClass.TerrainLowPoly)) && hitInfo.distance < 0.3f;
			StaticManager.Instance.StartCoroutine(smethod_3(groundExpl, skyExpl, Mathf.Min(30, grenadeItem.FragmentsCount) - num2, ballisticsCalculator, grenadeItem, explosionPosition, playerProfileIDWhoThrew, originalWeaponItem));
		}
		action_0?.Invoke(explosionPosition, grenadeItem.MaxExplosionDistance);
	}

	public static IEnumerator smethod_3(bool groundExpl, bool skyExpl, int fragmentsCount, ISharedBallisticsCalculator calculator, IExplosiveItem grenadeItem, Vector3 grenadePosition, [CanBeNull] string playerProfileID, [CanBeNull] Item originalWeaponItem)
	{
		AmmoItemClass ammo = grenadeItem.CreateFragment();
		for (int i = 0; i < fragmentsCount; i++)
		{
			Vector3 randomDirection = Grenade.GrenadeRandoms.GetRandomDirection(Int_3);
			if (skyExpl)
			{
				if (randomDirection.y > 0f && i % 4 < 3)
				{
					randomDirection.y = 0f - randomDirection.y;
				}
			}
			else if (groundExpl && randomDirection.y < 0f && i % 4 < 3)
			{
				randomDirection.y = 0f - randomDirection.y;
			}
			calculator.Shoot(ammo, grenadePosition, randomDirection.normalized, 1, playerProfileID, originalWeaponItem, 0.5f + (float)(Int_3 % 10) / 20f, 0);
			Int_3++;
			if (i % 9 >= 8)
			{
				yield return null;
			}
		}
	}
}
