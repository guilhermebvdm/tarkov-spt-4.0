using System;
using System.Collections.Generic;
using System.Reflection;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Dismemberment.Classes;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Dismemberment.Patches;

public class BleedPatch : ModulePatch
{
	public static Dictionary<string, float> calibers = new Dictionary<string, float>();

	public static List<string> light_calibers = new List<string>();

	public static List<string> heavy_calibers = new List<string>();

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("CreateShot", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(BallisticsCalculator __instance, EftBulletClass __result, AmmoItemClass __0, Vector3 __1, Vector3 __2, int __3, string __4, Item __5, float __6, int __7)
	{
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.EnableBloodEffects.Value && __result != null)
		{
			((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(__result));
		}
	}

	private static System.Collections.IEnumerator WatchShot(EftBulletClass shot)
	{
		if (shot == null) yield break;

		float timeout = 3.0f;
		while (!shot.IsShotFinished && timeout > 0f)
		{
			timeout -= Time.deltaTime;
			yield return null;
		}

		if (shot != null && shot.IsShotFinished && shot.HitCollider != null)
		{
			ProcessWatchShot(shot);
		}
	}

	public static void ProcessWatchShot(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Item ammo = shot.Ammo;
		AmmoItemClass bulletClass = (ammo is AmmoItemClass ammoItem) ? ammoItem : null;
		if (bulletClass == null) return;

		IPlayerOwner playerOwner = shot.Player;
		EPointOfView? pov = playerOwner?.iPlayer?.PlayerBones?.Player?.PointOfView;
		if (pov != null && pov.Value != EPointOfView.FirstPerson) return;

		Collider col = shot.HitCollider;
		GameObject colGO = col.gameObject;
		if (colGO.GetComponent<ObservedLootItem>() != null) return;

		int colLayer = colGO.layer;
		int playerLayer = LayerMask.NameToLayer("Player");
		int hitColliderLayer = LayerMask.NameToLayer("HitCollider");
		int deadBodyLayer = LayerMask.NameToLayer("Deadbody");
		bool isValidLayer = colLayer == playerLayer || colLayer == hitColliderLayer || colLayer == deadBodyLayer;

		if (!isValidLayer)
		{
			if (VisceralCombat.Dismemberment.Classes.Utils.CheckNameInHierarchyRecursive(colGO, "generated") || VisceralCombat.Dismemberment.Classes.Utils.CheckNameInHierarchyRecursive(colGO, "weapon"))
			{
				return;
			}
			return;
		}

		Player targetPlayer = VisceralCombat.Dismemberment.Classes.Utils.GetComponentInParentRecursive<Player>(colGO);
		if (targetPlayer != null)
		{
			List<BodyRendererDataStruct> renderers = Traverse.Create(targetPlayer).Field<List<BodyRendererDataStruct>>("_preAllocatedRenderersList").Value;
			if (renderers != null && renderers.Count > 0 && Singleton<Effects>.Instantiated)
			{
				Singleton<Effects>.Instance.PlayerMeshesHit(renderers, shot.HitPoint, -shot.HitNormal);
			}

			string caliber = bulletClass.Caliber;
			if (!calibers.TryGetValue(caliber, out float chance)) return;

			if (Random.value < chance)
			{
				float randomTime = Random.Range(1f, 5f);
				int diceRoll = Random.Range(0, 10);
				bool isAlive = targetPlayer.HealthController != null && targetPlayer.HealthController.IsAlive;

				if (light_calibers.Contains(caliber))
				{
					HitEffect(targetPlayer, col, shot, isAlive, randomTime, 18);
				}
				else if (heavy_calibers.Contains(caliber))
				{
					HitEffect(targetPlayer, col, shot, isAlive, randomTime, 17);
				}

				int bloodIndex = Random.Range(0, 10);
				BleedEffect(col, shot, isAlive, diceRoll, randomTime, (bloodIndex <= 7) ? 10 : 11);
			}
		}
	}

	public static void HitEffect(Player player, Collider col, EftBulletClass shot, bool isAlive, float time, int bundleIndex)
	{
		if (VisceralEntry.Instance?.effectContainer == null) return;
		EffectContainer container = VisceralEntry.Instance.effectContainer;

		GameObject val = null;
		switch (bundleIndex)
		{
		case 18:
			val = container.lightBleedEffect;
			break;
		case 17:
			val = container.heavyBleedEffect;
			break;
		default:
			if (container.blood3dFxEffects != null && bundleIndex >= 0 && bundleIndex < container.blood3dFxEffects.Count)
			{
				val = container.blood3dFxEffects[bundleIndex];
			}
			break;
		}

		if (val != null)
		{
			GameObject bloodParticleObject = Object.Instantiate<GameObject>(val);
			bloodParticleObject.AddComponent<ParticleFloorPainter>();
			bloodParticleObject.transform.SetParent(col.transform, false);
			Vector3 hitNorm1 = -shot.HitNormal;
			if (hitNorm1.sqrMagnitude < 0.001f) hitNorm1 = Vector3.forward;
			bloodParticleObject.transform.localRotation = Quaternion.LookRotation(hitNorm1);
			ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in componentsInChildren)
			{
				var main = ps.main;
				main.duration = time;
				var collision = ps.collision;
				collision.sendCollisionMessages = true;
				if (ps.gameObject.GetComponent<ParticleFloorPainter>() == null)
				{
					ps.gameObject.AddComponent<ParticleFloorPainter>();
				}
				RagdollHelperClass.ApplyDarkCoagulatedBloodFx(ps);
				ps.Play();
			}
			Object.Destroy(bloodParticleObject, time + 1f);
		}
	}

	public static void BleedEffect(Collider col, EftBulletClass shot, bool isAlive, float chance, float time, int bundleIndex)
	{
		if (chance < 8f || VisceralEntry.Instance?.effectContainer == null) return;
		EffectContainer container = VisceralEntry.Instance.effectContainer;

		GameObject val = null;
		switch (bundleIndex)
		{
		case 10:
			val = container.squirtEffect1;
			break;
		case 11:
			val = container.squirtEffect2;
			break;
		default:
			if (container.blood3dFxEffects != null && bundleIndex >= 0 && bundleIndex < container.blood3dFxEffects.Count)
			{
				val = container.blood3dFxEffects[bundleIndex];
			}
			break;
		}

		if (val != null)
		{
			GameObject bloodParticleObject = Object.Instantiate<GameObject>(val);
			bloodParticleObject.AddComponent<ParticleFloorPainter>();
			bloodParticleObject.transform.SetParent(col.transform, false);
			Vector3 hitNorm2 = -shot.HitNormal;
			if (hitNorm2.sqrMagnitude < 0.001f) hitNorm2 = Vector3.forward;
			bloodParticleObject.transform.localRotation = Quaternion.LookRotation(hitNorm2);
			ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in componentsInChildren)
			{
				var main = ps.main;
				main.duration = time;
				var collision = ps.collision;
				collision.sendCollisionMessages = true;
				if (ps.gameObject.GetComponent<ParticleFloorPainter>() == null)
				{
					ps.gameObject.AddComponent<ParticleFloorPainter>();
				}
				RagdollHelperClass.ApplyDarkCoagulatedBloodFx(ps);
				ps.Play();
			}
			Object.Destroy(bloodParticleObject, time + 1f);
		}
	}
}
