using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT.Ballistics;
using UnityEngine;

namespace EFT;

public class KnifeCollider : MonoBehaviour
{
	public BoxCollider Collider;

	public Vector3 CastDirection;

	public RaycastHit[] hits = new RaycastHit[2];

	public Action<Player.GStruct182> OnHit;

	public float MaxDistance = 1f;

	internal Player.BaseKnifeController baseKnifeController_0;

	internal Player player_0;

	public int _hitMask;

	public int _spiritMask;

	private Collider[] collider_0 = new Collider[16];

	private bool bool_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Quaternion quaternion_0;

	private HashSet<Player> hashSet_0 = new HashSet<Player>();

	private Vector3 vector3_2 = Vector3.one;

	public Func<Vector3> GetPlayerOrientation;

	public Func<PlayerBones> GetPlayerBones;

	public bool isInfectedPlayer;

	private HashSet<IPlayer> hashSet_1 = new HashSet<IPlayer>();

	[Header("Server setting")]
	[SerializeField]
	private float _weaponRootExtension = 0.7f;

	public void Awake()
	{
		Collider.enabled = false;
	}

	public void OnFire()
	{
	}

	public void OnFireEnd()
	{
		hashSet_0.Clear();
		hashSet_1.Clear();
	}

	public void SetBotParameters(Vector3 colliderScaleMultiplier)
	{
		vector3_2 = colliderScaleMultiplier;
	}

	public void ManualUpdate()
	{
		if (OnHit != null)
		{
			if (player_0 != null && player_0.IsAI)
			{
				method_2();
			}
			else
			{
				method_0();
			}
		}
	}

	public void method_0()
	{
		hits = new RaycastHit[2];
		Vector3 vector = GetPlayerOrientation();
		Vector3 size = Collider.size;
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		Physics.BoxCastNonAlloc(position + rotation * Collider.center - vector * size.y, new Vector3(size.x / 2f, size.y / 4f, size.z / 2f), vector, orientation: rotation, maxDistance: (MaxDistance >= float.Epsilon) ? MaxDistance : (Collider.size.y * 2.5f), layerMask: _hitMask, results: hits, queryTriggerInteraction: QueryTriggerInteraction.Collide);
		GClass1457.DrawBox(position + rotation * Collider.center - vector * Collider.size.y, size / 2f, rotation, Color.cyan);
		GClass1457.DrawBox(position + rotation * Collider.center - vector * Collider.size.y + vector * (size.y * 2f), size / 2f, rotation, Color.red);
		RaycastHit[] array = hits;
		int num = 0;
		RaycastHit hit;
		while (true)
		{
			if (num < array.Length)
			{
				hit = array[num];
				if (!(hit.collider == null) && method_1(hit.collider))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		OnHit(new Player.GStruct182(hit));
	}

	public bool method_1(Collider hitCollider)
	{
		BodyPartCollider[] bodyPartColliders = GetPlayerBones().BodyPartColliders;
		int num = 0;
		while (true)
		{
			if (num < bodyPartColliders.Length)
			{
				if (bodyPartColliders[num].gameObject == hitCollider.gameObject)
				{
					break;
				}
				num++;
				continue;
			}
			ArmorPlateCollider[] armorPlateColliders = GetPlayerBones().ArmorPlateColliders;
			num = 0;
			while (true)
			{
				if (num < armorPlateColliders.Length)
				{
					if (armorPlateColliders[num].gameObject == hitCollider.gameObject)
					{
						break;
					}
					num++;
					continue;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void method_2()
	{
		if (player_0.IsAI && OnHit != null)
		{
			Vector3 position = player_0.WeaponRoot.position;
			Vector3 lookDirection = player_0.LookDirection;
			Vector3 size = GClass842.GetColliderBoundsWithoutRotation(Collider).size;
			size.x *= vector3_2.x;
			size.z *= vector3_2.z;
			size.y += _weaponRootExtension;
			method_3(position, lookDirection, size);
		}
	}

	public void method_3(Vector3 startPoint, Vector3 direction, Vector3 castSize)
	{
		castSize.y = method_7(startPoint, direction, castSize.y) * vector3_2.y;
		Vector3 halfExtents = castSize * 0.5f;
		Physics.OverlapBoxNonAlloc(startPoint + direction * halfExtents.y, orientation: Quaternion.LookRotation(direction) * Quaternion.AngleAxis(90f, Vector3.left), halfExtents: halfExtents, results: collider_0, mask: _hitMask, queryTriggerInteraction: QueryTriggerInteraction.Collide);
		Collider collider = null;
		Collider[] array = collider_0;
		foreach (Collider collider2 in array)
		{
			if (collider2 == null || !collider2.TryGetComponent<BallisticCollider>(out var component) || component is ArmorPlateCollider || player_0.HasBodyPartCollider(collider2))
			{
				continue;
			}
			if (component is BodyPartCollider bodyPartCollider)
			{
				if ((isInfectedPlayer && bodyPartCollider.playerBridge.UsingSimplifiedSkeleton) || hashSet_1.Contains(bodyPartCollider.Player))
				{
					continue;
				}
				hashSet_1.Add(bodyPartCollider.Player);
			}
			collider = collider2;
			break;
		}
		if (!(collider == null))
		{
			OnHit(new Player.GStruct182
			{
				collider = collider,
				point = collider.bounds.center,
				normal = Vector3.zero
			});
		}
	}

	public void method_4(PlayerSpirit victimSpirit)
	{
		if (!hashSet_0.Contains(victimSpirit._player))
		{
			hashSet_0.Add(victimSpirit._player);
		}
	}

	public void method_5(Vector3 startPoint, Vector3 direction, Player victimPlayer, Vector3 castSize)
	{
		int num = method_8(startPoint, direction, LayerMasksDataAbstractClass.SpiritHitMaskSecondPass, collider_0, castSize);
		Collider collider = null;
		for (int i = 0; i < num; i++)
		{
			Collider collider2 = collider_0[i];
			BodyPartCollider component = collider2.GetComponent<BodyPartCollider>();
			if (component != null && component.playerBridge.iPlayer == victimPlayer)
			{
				collider = collider2;
				break;
			}
		}
		if (collider != null)
		{
			OnHit(new Player.GStruct182
			{
				collider = collider,
				point = collider.bounds.center,
				normal = Vector3.zero
			});
		}
	}

	public PlayerSpirit method_6(Vector3 startPoint, Vector3 direction, Vector3 castSize)
	{
		int num = method_8(startPoint, direction, LayerMasksDataAbstractClass.PlayerSpiritAuraLayerMask, collider_0, castSize);
		if (num > 0)
		{
			Collider collider = null;
			Collider collider2 = player_0.Spirit.PlayerSpiritAura.GetCollider();
			for (int i = 0; i < num; i++)
			{
				Collider collider3 = collider_0[i];
				if (collider3 != collider2)
				{
					Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider3);
					if (!(playerByCollider != null) || !isInfectedPlayer || !playerByCollider.UsedSimplifiedSkeleton)
					{
						collider = collider3;
						break;
					}
				}
			}
			if (collider != null)
			{
				Player playerByCollider2 = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
				if (!(playerByCollider2 == null) && !(playerByCollider2.Spirit == null) && playerByCollider2.Spirit.IsActive)
				{
					return playerByCollider2.Spirit;
				}
				return null;
			}
		}
		return null;
	}

	public float method_7(Vector3 startPoint, Vector3 direction, float maxDistance)
	{
		if (Physics.Raycast(new Ray(startPoint, direction), out var hitInfo, maxDistance, LayerMasksDataAbstractClass.StaticObjectsHitMask, QueryTriggerInteraction.Ignore))
		{
			return hitInfo.distance;
		}
		return maxDistance;
	}

	public int method_8(Vector3 startPoint, Vector3 direction, int mask, Collider[] colliders, Vector3 castSize)
	{
		Vector3 halfExtents = castSize * 0.5f;
		Vector3 center = startPoint + direction * halfExtents.y;
		Quaternion orientation = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(90f, Vector3.left);
		int result = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, orientation, mask, QueryTriggerInteraction.Ignore);
		bool_0 = true;
		vector3_0 = center;
		vector3_1 = castSize;
		quaternion_0 = orientation;
		return result;
	}

	public void OnDrawGizmos()
	{
		if (bool_0)
		{
			Gizmos.matrix = Matrix4x4.TRS(vector3_0, quaternion_0, Vector3.one);
			Gizmos.color = Color.red;
			Gizmos.DrawCube(Vector3.zero, vector3_1);
			bool_0 = false;
		}
	}
}
