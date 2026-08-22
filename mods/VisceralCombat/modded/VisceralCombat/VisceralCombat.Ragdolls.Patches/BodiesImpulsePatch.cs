using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

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
		{ "Caliber9x19PARA", 18f },
		{ "Caliber40mmRU", 100f },
		{ "Caliber9x21", 20f },
		{ "Caliber1143x23ACP", 22f },
		{ "Caliber46x30", 25f },
		{ "Caliber762x25TT", 20f },
		{ "Caliber20g", 110f },
		{ "Caliber57x28", 22f },
		{ "Caliber762x54R", 70f },
		{ "Caliber366TKM", 50f },
		{ "Caliber23x75", 160f },
		{ "Caliber86x70", 120f },
		{ "Caliber9x33R", 40f },
		{ "Caliber26x75", 80f },
		{ "Caliber68x51", 80f }
	};

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(EftBulletClass) }, null);
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		VisceralCombat.Combined.Classes.VisceralShotProcessor.RegisterShot(shot);
	}

	public static void ProcessImpulse(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Collider hitCollider = shot.HitCollider;

		// Calculate realistic physical momentum: p = m * v (mass in kg * speed in m/s) with 0.25f scale
		float massKg = (shot.BulletMassGram > 0f) ? (shot.BulletMassGram / 1000f) : 0.008f;
		float speed = (shot.Speed > 0f) ? shot.Speed : 400f;
		float physicalImpulse = (massKg * speed) * 0.25f;

		// Check if hitting dropped loot item
		Rigidbody lootRb = hitCollider.attachedRigidbody ?? hitCollider.GetComponentInParent<Rigidbody>();
		if (lootRb != null && lootRb.gameObject.GetComponent<ObservedLootItem>() != null)
		{
			if (VisceralEntry.Instance != null && VisceralEntry.Instance.ItemForce.Value)
			{
				physicalImpulse *= VisceralEntry.Instance.objectIntensity.Value;
				lootRb.AddForceAtPosition(shot.Direction * physicalImpulse, shot.HitPoint, ForceMode.Impulse);
			}
			return;
		}

		// Wake the corpse's rigidbodies and re-support them in EFT physics
		Rigidbody[] corpseRbs = RagdollHelperClass.WakeCorpse(hitCollider, 2.5f);

		// Resolve the target Rigidbody to apply impulse
		Rigidbody targetRb = hitCollider.attachedRigidbody ?? hitCollider.GetComponentInParent<Rigidbody>();
		if (targetRb == null && corpseRbs != null && corpseRbs.Length > 0)
		{
			targetRb = corpseRbs.FirstOrDefault(r => r != null && !RagdollHelperClass.ParentIsDismembered(r.transform));
		}

		if (targetRb == null) return;

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
		Vector3 impulse = shot.Direction * (physicalImpulse * bodyPartMult * totalIntensity);

		targetRb.AddForceAtPosition(impulse, shot.HitPoint, ForceMode.Impulse);
	}
}
