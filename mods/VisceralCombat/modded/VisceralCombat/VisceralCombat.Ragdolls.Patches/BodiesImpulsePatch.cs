using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class BodiesImpulsePatch : ModulePatch
{
	private static Dictionary<string, float> _dictionary = new Dictionary<string, float>
	{
		{ "Caliber12g", 150f },
		{ "Caliber762x51", 65f },
		{ "Caliber762x39", 45f },
		{ "Caliber9x39", 33f },
		{ "Caliber545x39", 35f },
		{ "Caliber9x18PM", 12f },
		{ "Caliber762x35", 60f },
		{ "Caliber556x45NATO", 30f },
		{ "Caliber127x55", 100f },
		{ "Caliber127x108", 350f },
		{ "Caliber366TKM", 60f },
		{ "Caliber40x46", 200f },
		{ "Caliber26x75", 70f },
		{ "Caliber30x29", 350f },
		{ "Caliber762x54R", 95f },
		{ "Caliber86x70", 800f },
		{ "Caliber9x19PARA", 12f },
		{ "Caliber1143x23ACP", 12f },
		{ "Caliber9x21", 5f },
		{ "Caliber57x28", 40f },
		{ "Caliber23x75", 200f },
		{ "Caliber25x59mm", 180f },
		{ "Caliber12.7x99", 110f },
		{ "Caliber68x51", 40f }
	};

	private static Dictionary<string, float> _bonedictionary = new Dictionary<string, float>
	{
		{ "Base HumanSpine3", 0.65f },
		{ "Base HumanSpine2", 0.65f },
		{ "Base HumanSpine1", 0.65f },
		{ "Base HumanPelvis", 0.65f },
		{ "Base HumanHead", 3.5f },
		{ "Base HumanNeck", 3.5f },
		{ "Base HumanLUpperarm", 0.75f },
		{ "Base HumanLForearm1", 1f },
		{ "Base HumanRUpperarm", 0.75f },
		{ "Base HumanRForearm1", 1f },
		{ "Base HumanLThigh1", 0.75f },
		{ "Base HumanLThigh2", 0.8f },
		{ "Base HumanLCalf", 1f },
		{ "Base HumanLFoot", 1f },
		{ "Base HumanLToe", 1f },
		{ "Base HumanRThigh1", 0.75f },
		{ "Base HumanRThigh2", 0.8f },
		{ "Base HumanRCalf", 1f },
		{ "Base HumanRFoot", 1f },
		{ "Base HumanRToe", 1f }
	};

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(EftBulletClass) }, null);
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(shot));
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
			ProcessImpulse(shot);
		}
	}

	public static void ProcessImpulse(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Collider hitCollider = shot.HitCollider;
		Rigidbody rb = hitCollider.attachedRigidbody;
		if (rb == null) return;

		Item ammo = shot.Ammo;
		AmmoItemClass bulletClass = (ammo is AmmoItemClass ammoItem) ? ammoItem : null;
		if (bulletClass == null) return;

		if (!_dictionary.TryGetValue(bulletClass.Caliber, out float modifier))
		{
			modifier = 25f;
		}

		modifier /= Mathf.Max(bulletClass.ProjectileCount, 1);

		if (bulletClass.Caliber != "Caliber12g" && _bonedictionary.TryGetValue(hitCollider.name, out float boneModifier))
		{
			modifier *= boneModifier;
		}

		if (rb.gameObject.GetComponent<ObservedLootItem>() != null)
		{
			if (VisceralEntry.Instance != null && VisceralEntry.Instance.ItemForce.Value)
			{
				modifier *= VisceralEntry.Instance.objectIntensity.Value;
			}
			else
			{
				return;
			}
		}

		string hitName = hitCollider.name.ToLower();
		float bodyPartMult = 1.0f;
		if (VisceralEntry.Instance != null)
		{
			if (hitName.Contains("head")) bodyPartMult = VisceralEntry.Instance.headForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("pelvis") || hitName.Contains("spine") || hitName.Contains("rib")) bodyPartMult = VisceralEntry.Instance.TorsoForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("arm")) bodyPartMult = VisceralEntry.Instance.ArmsForceIntensity?.Value ?? 1f;
			else if (hitName.Contains("thigh") || hitName.Contains("calf") || hitName.Contains("foot")) bodyPartMult = VisceralEntry.Instance.LegsForceIntensity?.Value ?? 1f;
		}

		float totalIntensity = (VisceralEntry.Instance != null && VisceralEntry.Instance.ShotIntensity != null) ? VisceralEntry.Instance.ShotIntensity.Value : 1f;
		Vector3 impulse = shot.Direction * (modifier * bodyPartMult * totalIntensity);
		rb.AddForceAtPosition(impulse, shot.HitPoint, ForceMode.Impulse);
	}
}
